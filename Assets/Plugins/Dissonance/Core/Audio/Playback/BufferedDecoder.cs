using System;
using Dissonance.Audio.Codecs;
using Dissonance.Config;
using NAudio.Wave;

namespace Dissonance.Audio.Playback
{
    /// <summary>
    ///     Buffers encoded frames with an internal <see cref="EncodedAudioBuffer" />, and decodes frames in sequence as they
    ///     are requested.
    /// </summary>
    internal class BufferedDecoder : IFrameSource
    {
        private static readonly Log Log = Logs.Create(LogCategory.Playback, "Decoder");

        private readonly EncodedAudioBuffer _buffer;
        private readonly IVoiceDecoder _decoder;
        private readonly uint _frameSize;
        private readonly WaveFormat _waveFormat;
        private readonly Action<EncodedAudio> _recycleFrame;

        private AudioFileWriter _diagnosticOutput;
        
        public int BufferCount
        {
            get { return _buffer.Count; }
        }

        public uint SequenceNumber
        {
            get { return _buffer.SequenceNumber; }
        }

        public BufferedDecoder(IVoiceDecoder decoder, uint frameSize, WaveFormat waveFormat, Action<EncodedAudio> recycleFrame)
        {
            _decoder = decoder;
            _frameSize = frameSize;
            _waveFormat = waveFormat;
            _recycleFrame = recycleFrame;
            _buffer = new EncodedAudioBuffer(recycleFrame);
        }

        public uint FrameSize
        {
            get { return _frameSize; }
        }

        public WaveFormat WaveFormat
        {
            get { return _waveFormat; }
        }

        public void Prepare(SessionContext context)
        {
            if (DebugSettings.Instance.EnablePlaybackDiagnostics && DebugSettings.Instance.RecordDecodedAudio)
            {
                var filename = string.Format("Dissonance_Diagnostics/Decoded_{0}_{1}_{2}", context.PlayerName, context.Id, DateTime.UtcNow.ToFileTime());
                _diagnosticOutput = new AudioFileWriter(filename, _waveFormat);
            }
        }

        public bool Read(ArraySegment<float> frame)
        {
            EncodedAudio? encoded;
            var complete = _buffer.Read(out encoded);

            int decodedCount;
            if (encoded != null)
            {
                Log.Trace("Decoding frame {0}", encoded.Value.Sequence);
                decodedCount = _decoder.Decode(encoded.Value.Data, frame);
                _recycleFrame(encoded.Value);
            }
            else
            {
                decodedCount = _decoder.Decode(null, frame);
            }

            //Sanity check that decoding got correct number of samples
            if (decodedCount != _frameSize)
                throw new InvalidOperationException(string.Format("Decoding a frame of audio got {0} samples, but should have decoded {1} samples", decodedCount, _frameSize));

            if (_diagnosticOutput != null)
                _diagnosticOutput.WriteSamples(frame);

            return complete;
        }

        public void Reset()
        {
            _buffer.Reset();
            _decoder.Reset();

            if (_diagnosticOutput != null)
            {
                _diagnosticOutput.Dispose();
                _diagnosticOutput = null;
            }
        }

        public void Push(EncodedAudio frame)
        {
            _buffer.Push(frame);
        }

        public void Stop()
        {
            _buffer.Stop();
        }
    }
}