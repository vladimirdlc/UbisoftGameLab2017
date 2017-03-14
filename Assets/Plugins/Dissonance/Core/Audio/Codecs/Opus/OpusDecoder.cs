using System;
using NAudio.Wave;

namespace Dissonance.Audio.Codecs.Opus
{
    internal class OpusDecoder
        : IVoiceDecoder
    {
        private readonly WaveFormat _format;
        public WaveFormat Format
        {
            get { return _format; }
        }

        private OpusNative.OpusDecoder _decoder;

        public OpusDecoder(WaveFormat format)
        {
            _format = format;
            _decoder = new OpusNative.OpusDecoder(format.SampleRate, format.Channels) {
                EnableForwardErrorCorrection = false
            };
        }

        public void Dispose()
        {
            if (_decoder != null)
                _decoder.Dispose();
            _decoder = null;
        }

        public void Reset()
        {
            _decoder.Reset();
        }

        public int Decode(ArraySegment<byte>? input, ArraySegment<float> output)
        {
            return _decoder.DecodeFloats(input, output);
        }
    }
}
