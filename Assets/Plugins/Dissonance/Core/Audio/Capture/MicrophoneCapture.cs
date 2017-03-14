using System;
using System.Collections.Generic;
using System.Threading;
using Dissonance.Config;
using Dissonance.Datastructures;
using Dissonance.VAD;
using NAudio.Wave;
using UnityEngine;

namespace Dissonance.Audio.Capture
{
    internal class MicrophoneCapture
        : IDisposable, IMicrophoneCapture
    {
        #region fields and properties
        private static readonly Log Log = Logs.Create(LogCategory.Recording, "Microphone");

        private readonly ConcurrentPool<float[]> _bufferSource;
        private readonly TransferBuffer<float[]> _delayBuffer;
        private readonly TransferBuffer<float[]> _sampleSink;
        private readonly List<IMicrophoneHandler> _subscriptions;

        private readonly POTBuffer _readBuffer = new POTBuffer(12);
        private readonly BufferedSampleProvider _rawMicSamples;
        private readonly IFrameProvider _rawMicFrames;

        private AudioFileWriter _microphoneDiagnosticOutput;

        private readonly WaveFormat _format;
        public WaveFormat Format
        {
            get { return _format; }
        }

        private readonly Preprocessor _speex;

        private readonly List<IVoiceActivationListener> _voiceActivationListeners = new List<IVoiceActivationListener>();
        private bool _vadDirty;
        private bool _vadActive;
        private readonly VoiceDetection _vad;

        internal VoiceDetection VAD
        {
            get { return _vad; }
        }

        private readonly AudioClip _clip;
        private int _readHead;
        private bool _started;

        private readonly int _frameSize;
        public int FrameSize
        {
            get { return _frameSize; }
        }

        private volatile bool _runThread;
        private readonly Thread _thread;
        private readonly AutoResetEvent _threadEvent;
        #endregion

        /// <param name="frameSize">The frame size in samples.</param>
        /// <param name="source">Source to read frames from</param>
        private MicrophoneCapture(int frameSize, AudioClip source)
        {
            if (source == null)
                throw new ArgumentNullException("source", Log.PossibleBugMessage("capture source clip is null", "333E11A6-8026-41EB-9B34-EF9ADC54B651"));

            _clip = source;
            _format = new WaveFormat(1, source.frequency);
            _frameSize = frameSize;
            _subscriptions = new List<IMicrophoneHandler>();

            //Create two buffers for transferring buffers between the two thread
            // - _bufferSource is a pool of empty buffers, ready for the main thread to use
            // - _sampleSink is a queue of samples, waiting to be processed off the main thread
            _bufferSource = new ConcurrentPool<float[]>(4, () => new float[frameSize]);
            _delayBuffer = new TransferBuffer<float[]>(2);
            _sampleSink = new TransferBuffer<float[]>(4);

            _rawMicSamples = new BufferedSampleProvider(_format, _frameSize * 2);
            _rawMicFrames = new SampleToFrameProvider(_rawMicSamples, (uint)_frameSize);

            _runThread = true;
            _threadEvent = new AutoResetEvent(false);
            _thread = new Thread(ThreadEntry);
            _thread.Start();

            _speex = new Preprocessor(frameSize, _format.SampleRate);

            _vad = new VoiceDetection();

            Log.Debug("Mic Sample Rate: " + _format.SampleRate);
        }

        public static MicrophoneCapture Start(int sampleRate, int frameSize)
        {
            //Early exit - check if there are no microphones connected
            if (Microphone.devices.Length == 0)
            {
                Log.Info("No microphone detected; disabling voice capture");
                return null;
            }

            //Get device caps and modify sample rate and frame size to match
            int minFreq;
            int maxFreq;
            Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);

            //if min and max are both zero that's a special signal that *any* rate is fine
            if (!(minFreq == 0 && maxFreq == 0))
            {
                var s = sampleRate;
                if (sampleRate < minFreq)
                    sampleRate = minFreq;
                else if (sampleRate > maxFreq)
                    sampleRate = maxFreq;

                //Rescale framesize in same ratio as sample rate
                if (s != sampleRate)
                    frameSize = (int)((sampleRate / (float)s) * frameSize);
            }

            var clip = Microphone.Start(null, true, 1, sampleRate);
            if (clip == null)
            {
                Log.Warn("Failed to start microphone capture");
                return null;
            }

            var capture = new MicrophoneCapture(frameSize, clip);
            Log.Info("Began microphone capture");
            return capture;
        }

        public void Subscribe(IMicrophoneHandler listener)
        {
            _subscriptions.Add(listener);
        }

        public bool Unsubscribe(IMicrophoneHandler listener)
        {
            return _subscriptions.Remove(listener);
        }

        public void Subscribe(IVoiceActivationListener listener)
        {
            lock (_voiceActivationListeners)
            {
                _voiceActivationListeners.Add(listener);
                _vadActive = true;
            }
            _vadDirty = true;
        }

        public bool Unsubscribe(IVoiceActivationListener listener)
        {
            _vadDirty = true;
            lock (_voiceActivationListeners)
            {
                var removed = _voiceActivationListeners.Remove(listener);
                _vadActive = _voiceActivationListeners.Count > 0;
                return removed;
            }
        }

        public void Update(bool transmit)
        {
            if (!_started)
            {
                _started = Microphone.GetPosition(null) > 0;
                return;
            }

            if ((_subscriptions.Count > 0 || _vadActive) && transmit)
            {
                //Read samples from clip into mic sample buffer
                DrainMicSamples();
            }
            else
            {
                //We're not interested in the audio from the mic, so skip the read head to the current mic position and drop all the audio
                _readHead = Microphone.GetPosition(null);

                if (_microphoneDiagnosticOutput != null)
                {
                    _microphoneDiagnosticOutput.Dispose();
                    _microphoneDiagnosticOutput = null;
                }
            }
        }

        private void DrainMicSamples()
        {
            // read as many frame-sized chunks that are ready
            var writeHead = Microphone.GetPosition(null);
            uint samplesToRead = (uint)((_clip.samples + writeHead - _readHead) % _clip.samples);

            if (samplesToRead == 0)
                return;

            //If we're trying to read more data than we have buffer space expand the buffer (up to a max limit)
            //If we're at the max limit, just clamp to buffer size (hopefully we'll catch up with future, smaller, samples)
            while (samplesToRead > _readBuffer.MaxCount)
            {
                //absolute max buffer size, we will refuse to expand beyond this
                if (_readBuffer.MaxCount > 16 || !_readBuffer.Expand(_rawMicSamples.Count))
                {
                    Log.Debug(string.Format("Insufficient buffer space, requested {0}, clamped to {1}", samplesToRead, _readBuffer.MaxCount));
                    samplesToRead = _readBuffer.MaxCount;
                    break;
                }
                else
                {
                    Log.Debug(string.Format("Growing buffer space to {0}", _readBuffer.MaxCount));
                }
            }

            //Inform the buffer how many samples we want to read
            _readBuffer.Alloc(samplesToRead);
            try
            {
                while (samplesToRead > 0)
                {
                    //Read from mic
                    var buffer = _readBuffer.GetBuffer(ref samplesToRead, true);
                    _clip.GetData(buffer, _readHead);
                    _readHead = (_readHead + buffer.Length) % _clip.samples;

                    //Send samples downstream
                    ConsumeSamples(new ArraySegment<float>(buffer, 0, buffer.Length));
                }
            }
            finally
            {
                _readBuffer.Free();
            }
        }

        /// <summary>
        /// Given some samples consume them (as many as possible at a time) and send frames downstream (as frequently as possible)
        /// </summary>
        /// <param name="samples"></param>
        private void ConsumeSamples(ArraySegment<float> samples)
        {
            while (samples.Count > 0)
            {
                //Write as many samples as possible (up to capacity of buffer)
                var written = _rawMicSamples.Write(samples.Array, samples.Offset, samples.Count);
                samples = new ArraySegment<float>(samples.Array, samples.Offset + written, samples.Count - written);

                //Drain as many of those samples as possible in frame sized chunks
                SendFrame();
            }
        }

        /// <summary>
        /// Read as many frames as possible from the mic sample buffer and pass them to the encoding thread
        /// </summary>
        private void SendFrame()
        {
            while (_rawMicSamples.Count > _frameSize)
            {
                //Get an empty buffer from the pool of buffers (sent back from the audio processing thread)
                var frameBuffer = _bufferSource.Get();

                //Read a complete frame
                _rawMicFrames.Read(new ArraySegment<float>(frameBuffer));

                //Create diagnostic writer (if necessary)
                if (DebugSettings.Instance.EnableRecordingDiagnostics && DebugSettings.Instance.RecordMicrophoneRawAudio)
                {
                    if (_microphoneDiagnosticOutput == null)
                    {
                        var filename = string.Format("Dissonance_Diagnostics/MicrophoneRawAudio_{0}", DateTime.UtcNow.ToFileTime());
                        _microphoneDiagnosticOutput = new AudioFileWriter(filename, _format);
                    }
                }
                else if (_microphoneDiagnosticOutput != null)
                {
                    _microphoneDiagnosticOutput.Dispose();
                    _microphoneDiagnosticOutput = null;
                }

                //Write out the diagnostic info
                if (_microphoneDiagnosticOutput != null)
                {
                    _microphoneDiagnosticOutput.WriteSamples(new ArraySegment<float>(frameBuffer));
                    _microphoneDiagnosticOutput.Flush();
                }

                //Send the full buffer to the audio thread for processing (no copying, just pass the entire buffer across by ref)
                if (!_sampleSink.Write(frameBuffer))
                    Log.Warn("Failed to write microphone samples into transfer buffer");
                _threadEvent.Set();
            }
        }

        private void ThreadEntry()
        {
            try
            {
                while (_runThread)
                {
                    //Wait for an event(s) to arrive which we need to process
                    _threadEvent.WaitOne(100);

                    //Get frames of audio sent over from the main thread
                    float[] buffer;
                    while (_sampleSink.Read(out buffer))
                    {
                        //Process the buffer with speex and VAD, calculate the target size of the delay line (1 frame delay is VAD is active)
                        var targetDelay = ProcessVAD(buffer) ? 1 : 0;

                        //Add this buffer to the delay line
                        if (!_delayBuffer.Write(buffer))
                            Log.Warn("Failed to write audio into delay buffer, dropping audio");

                        //Pull out enough items from the delay line until it is the target size
                        float[] r;
                        while (_delayBuffer.EstimatedUnreadCount > targetDelay && _delayBuffer.Read(out r))
                        {
                            SendSamplesToSubscribers(r);
                            _bufferSource.Put(buffer);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.PossibleBug("Unhandled exception killed the microphone capture thread: " + e, "E1A24AFE-0456-4922-A2D6-3BE22D17DA0C");
            }
        }

        /// <summary>
        /// Preprocess a buffer of audio. Updates the VAD (if necessary).
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns>true, if the audio should be delayed by a frame. Otherwise false</returns>
        private bool ProcessVAD(float[] buffer)
        {
            //Run voice detector on samples (only if there are any listeners)
            lock (_voiceActivationListeners)
            {
                if (_vadDirty && _voiceActivationListeners.Count > 0)
                {
                    _vadDirty = false;
                    _vad.Reset();

                    Log.Trace("Reset VAD");
                }

                //If we are running VAD we want to delay 
                if (_voiceActivationListeners.Count > 0)
                {
                    bool talk = _vad.IsSpeaking;
                    _vad.Handle(new ArraySegment<float>(buffer), _format);
                    if (talk != _vad.IsSpeaking)
                    {
                        for (int i = 0; i < _voiceActivationListeners.Count; i++)
                        {
                            if (_vad.IsSpeaking)
                                _voiceActivationListeners[i].VoiceActivationStart();
                            else
                                _voiceActivationListeners[i].VoiceActivationStop();
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        private void SendSamplesToSubscribers(float[] buffer)
        {
            //Process the audio through the speex preprocessor
            _speex.Process(new ArraySegment<float>(buffer));

            for (var i = 0; i < _subscriptions.Count; i++)
            {
                try
                {
                    _subscriptions[i].Handle(new ArraySegment<float>(buffer), _format);
                }
                catch (Exception ex)
                {
                    Log.Error("Microphone subscriber '{0}' threw: {1}", _subscriptions[i].GetType().Name, ex);
                }
            }
        }

        #region disposal
        private bool _disposed;
        public void Dispose()
        {
            if (!_disposed)
            {
                Log.Debug("Stopping audio capture thread");

                _runThread = false;
                _threadEvent.Set();
                if (!_thread.Join(100))
                {
                    Log.Error("Failed to stop audio capture thread gracefully, aborting");

                    while (_thread.IsAlive)
                    {
                        _thread.Abort();
                        _threadEvent.Set();
                        if (!_thread.Join(100))
                            Log.Error("Failed to abort audio capture thread, trying again");
                    }
                }

                if (_microphoneDiagnosticOutput != null)
                {
                    _microphoneDiagnosticOutput.Dispose();
                    _microphoneDiagnosticOutput = null;
                }

                Microphone.End(null);

                _speex.Dispose();

                Log.Info("Stopped microphone capture");
            }
            _disposed = true;
        }
        #endregion
    }
}
