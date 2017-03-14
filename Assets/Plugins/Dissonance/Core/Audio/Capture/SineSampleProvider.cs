using System;
using NAudio.Wave;

namespace Dissonance.Audio.Capture
{
    internal class SineSampleProvider
        : ISampleProvider
    {
        private readonly WaveFormat _format;
        private readonly double _frequency;

        private const double TwoPi = Math.PI * 2;

        public WaveFormat WaveFormat
        {
            get { return _format; }
        }

        private double _index;

        public SineSampleProvider(WaveFormat format, float frequency)
        {
            _format = format;
            _frequency = frequency;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                //Slightly reduce amplitude to prevent minor clipping
                buffer[offset + i] = (float)Math.Sin(_index) * 0.95f;

                //Stay within the 0 -> 2Pi range to prevent "_index" running out of precision
                _index = (_index + (_frequency / _format.SampleRate)) % TwoPi;
            }

            return count;
        }
    }
}
