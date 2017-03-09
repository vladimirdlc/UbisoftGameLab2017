using System;
using Dissonance.Audio.Codecs;
using Dissonance.Config;
using Dissonance.Networking;
using NAudio.Wave;

namespace Dissonance.Audio.Capture
{
    internal class EncoderPipeline : IMicrophoneHandler
    {
        private static readonly Log _log = Logs.Create(LogCategory.Recording, typeof(EncoderPipeline).Name);

        private readonly byte[] _encodedBytes;
        private readonly float[] _plainSamples;

        private readonly Func<int> _channelCount;
        private readonly IMicrophoneCapture _mic;
        private readonly IVoiceEncoder _encoder;
        private readonly ICommsNetwork _net;

        private readonly BufferedSampleProvider _input;
        private readonly Resampler _resampler;
        private readonly IFrameProvider _output;

        private readonly WaveFormat _inputFormat;
        private readonly uint _inputFrameSize;

        private bool _resetRequired;
        private bool _subscribed;

        private AudioFileWriter _microphoneDiagnosticOutput;
        private AudioFileWriter _preEncodeDiagnosticOutput;

        public EncoderPipeline(IMicrophoneCapture mic, IVoiceEncoder encoder, ICommsNetwork net, Func<int> channelCount)
        {
            _mic = mic;
            _encoder = encoder;
            _net = net;
            _channelCount = channelCount;
            
            _encodedBytes = new byte[encoder.FrameSize * sizeof(float)];
            _plainSamples = new float[encoder.FrameSize];
            _inputFormat = mic.Format;
            _inputFrameSize = (uint)mic.FrameSize;

            //Create an input buffer with plenty of spare space
            _input = new BufferedSampleProvider(_inputFormat, (int)(_inputFrameSize * 3));

            //Check for the case where we don't need to resample anything. In this case we can just directly return the input samples
            if (_encoder.SampleRate != _inputFormat.SampleRate)
                _resampler = new Resampler(_input, _encoder.SampleRate);

            //Whatever we did above, we need to read in frame size chunks
            _output = new SampleToFrameProvider((ISampleProvider)_resampler ?? _input, (uint)encoder.FrameSize);
        }

        public void Update()
        {
            var count = _channelCount();
            if (count > 0 && !_subscribed)
            {
                StartCapture();
            }
            else if (count == 0 && _subscribed)
            {
                StopCapture();
            }
        }

        public void Handle(ArraySegment<float> inputSamples, WaveFormat format)
        {
            if (_resetRequired)
            {
                _log.Trace("Resetting encoder pipeline");

                if (_resampler != null)
                    _resampler.Reset();
                _input.Reset();
                _output.Reset();

                _resetRequired = false;
            }

            if (!format.Equals(_inputFormat))
                throw new ArgumentException(string.Format("Samples expected in format {0}, but supplied with format {1}", _inputFormat, format), "format");
            if (inputSamples.Count != _inputFrameSize)
                throw new ArgumentException(string.Format("Incorrect number of samples, expected {0} but got {1}", _inputFrameSize, inputSamples.Count), "inputSamples");

            if (_microphoneDiagnosticOutput != null)
                _microphoneDiagnosticOutput.WriteSamples(inputSamples);
            
            //Write samples to the pipeline (keep a running total of how many we have sent)
            //Keep sending until we've sent all of these samples
            var offset = 0;
            while (offset != inputSamples.Count)
            {
                offset += _input.Write(inputSamples.Array, offset + inputSamples.Offset, inputSamples.Count - offset);

                //Drain some of those samples just written, encode them and send them off
                EncodeFrames();
            }
        }

        private void StartCapture()
        {
            if (_subscribed)
                throw _log.PossibleBug("Cannot subscribe encoder to mic: already subscribed", "B1F845C2-3A9F-48F0-B9D2-4E5457CFDCB8");
            _log.Trace("Subscribing encoder to microphone");

            if (DebugSettings.Instance.EnableRecordingDiagnostics && DebugSettings.Instance.RecordEncoderPipelineInputAudio)
            {
                var filename = string.Format("Dissonance_Diagnostics/EncoderPipelineInput_{0}", DateTime.UtcNow.ToFileTime());
                _microphoneDiagnosticOutput = new AudioFileWriter(filename, _inputFormat);
            }

            if (DebugSettings.Instance.EnableRecordingDiagnostics && DebugSettings.Instance.RecordEncoderPipelineOutputAudio)
            {
                var filename = string.Format("Dissonance_Diagnostics/EncoderPipelineOutput_{0}", DateTime.UtcNow.ToFileTime());
                _preEncodeDiagnosticOutput = new AudioFileWriter(filename, _output.WaveFormat);
            }

            _mic.Subscribe(this);
            _subscribed = true;
        }

        private void StopCapture()
        {
            if (!_subscribed)
                throw _log.PossibleBug("Cannot unsubscribe encoder from mic: not subscribed", "8E0EAC83-BF44-4BE3-B132-BFE02AD1FADB");
            _log.Trace("Unsubscribing encoder from microphone");

            _mic.Unsubscribe(this);
            _subscribed = false;
            _resetRequired = true;

            //Disposing the output writers is racy, because the audio thread is trying to write to them at the same time.
            //That's fine, because the writer have an internal SpinLock ensuring that doing this is safe
            if (_microphoneDiagnosticOutput != null)
            {
                _microphoneDiagnosticOutput.Dispose();
                _microphoneDiagnosticOutput = null;
            }

            if (_preEncodeDiagnosticOutput != null)
            {
                _preEncodeDiagnosticOutput.Dispose();
                _preEncodeDiagnosticOutput = null;
            }
        }

        private void EncodeFrames()
        {
            //Read frames of resampled samples (as many as we can, we want to keep this buffer empty and latency low)
            var encoderInput = new ArraySegment<float>(_plainSamples, 0, _encoder.FrameSize);
            while (_output.Read(encoderInput))
            {
                if (_preEncodeDiagnosticOutput != null)
                    _preEncodeDiagnosticOutput.WriteSamples(encoderInput);

                //Encode it
                var encoded = _encoder.Encode(encoderInput, new ArraySegment<byte>(_encodedBytes));

                //Transmit it
                _net.SendVoice(encoded);
            }
        }
    }
}
