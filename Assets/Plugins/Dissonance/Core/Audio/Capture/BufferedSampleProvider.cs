using System;
using Dissonance.Datastructures;
using NAudio.Wave;

namespace Dissonance.Audio.Capture
{
    /// <summary>
    /// A sample provider which reads from an internal buffer of samples
    /// </summary>
    internal class BufferedSampleProvider
        : ISampleProvider
    {
        public int Count
        {
            get { return _samples.EstimatedUnreadCount; }
        }

        private readonly WaveFormat _format;
        /// <summary>
        /// Format of the samples in this provider
        /// </summary>
        public WaveFormat WaveFormat
        {
            get { return _format; }
        }

        private readonly TransferBuffer<float> _samples;

        public BufferedSampleProvider(WaveFormat format, int bufferSize)
        {
            _format = format;
            _samples = new TransferBuffer<float>(bufferSize);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (!_samples.Read(new ArraySegment<float>(buffer, offset, count)))
                return 0;
            return count;
        }

        public int Write(float[] buffer, int offset, int count)
        {
            var capacity = _samples.Capacity - _samples.EstimatedUnreadCount;

            if (capacity < count)
            {
                if (_samples.Write(new ArraySegment<float>(buffer, offset, capacity)))
                    return count;
                else
                    return 0;
            }
            else
            {
                if (_samples.Write(new ArraySegment<float>(buffer, offset, count)))
                    return count;
                else
                    return 0;
            }
        }

        public void Reset()
        {
            _samples.Clear();
        }
    }
}
