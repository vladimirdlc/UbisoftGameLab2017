// Justification: copied from NAudio, and we want to make the minimal changes possible
// ReSharper disable All

using System;

namespace NAudio.Wave
{
    public sealed class WaveFormat
    {
        private readonly int _channels;
        /// <summary>
        /// Returns the number of channels (1=mono,2=stereo etc)
        /// </summary>
        public int Channels
        {
            get
            {
                return _channels;
            }
        }

        private readonly int _sampleRate;
        /// <summary>
        /// Returns the sample rate (samples per second)
        /// </summary>
        public int SampleRate
        {
            get
            {
                return _sampleRate;
            }
        }

        public WaveFormat(int channels, int sampleRate)
        {
            if (channels > 64)
                throw new ArgumentOutOfRangeException("channels", "More than 64 channels");

            _channels = channels;
            _sampleRate = sampleRate;
        }

        public bool Equals(WaveFormat other)
        {
            return other.Channels == Channels
                && other.SampleRate == SampleRate;
        }

        public override string ToString()
        {
            return string.Format("(Channels:{0}, Rate:{1})", Channels, SampleRate);
        }
    }
}
