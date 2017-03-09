using System;
using NAudio.Wave;

namespace Dissonance.Audio.Codecs.Identity
{
    internal class IdentityDecoder
        : IVoiceDecoder
    {
        private readonly WaveFormat _format;
        public WaveFormat Format
        {
            get { return _format; }
        }

        public IdentityDecoder(WaveFormat format)
        {
            _format = format;
        }

        public void Reset()
        {
        }

        public int Decode(ArraySegment<byte>? input, ArraySegment<float> output)
        {
            if (!input.HasValue)
            {
                Array.Clear(output.Array, output.Offset, output.Count);
                return output.Count;
            }

            var bytes = input.Value.Count;
            if (bytes > output.Count * sizeof(float))
                throw new ArgumentException("output buffer is too small");

            Buffer.BlockCopy(input.Value.Array, input.Value.Offset, output.Array, output.Offset, bytes);
            return input.Value.Count / sizeof(float);
        }

        public void Dispose()
        {
        }
    }
}
