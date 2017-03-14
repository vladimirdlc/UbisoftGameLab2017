using System;
using System.Collections.Generic;

namespace Dissonance.Datastructures
{
    /// <summary>
    /// A set of (Pow2) buffers
    /// </summary>
    internal class POTBuffer
    {
        private readonly List<float[]> _buffers;

        private uint _maxCount;
        public uint MaxCount
        {
            get { return _maxCount; }
        }

        public uint Pow2
        {
            get { return (uint)_buffers.Count; }
        }

        private uint _currentCount;
        public uint Count
        {
            get { return _currentCount; }
        }

        public POTBuffer(byte maxPow)
        {
            _buffers = new List<float[]>(maxPow);
            for (var i = 0; i < maxPow; i++)
                _buffers.Add(new float[1 << i]);

            _maxCount = (uint)(1 << maxPow) - 1;
        }

        /// <summary>
        /// Mark all buffers as unused
        /// </summary>
        public void Free()
        {
            _currentCount = 0;
        }

        /// <summary>
        /// Set the count of accessible buffers
        /// </summary>
        /// <param name="count"></param>
        public void Alloc(uint count)
        {
            if (count > MaxCount)
                throw new ArgumentOutOfRangeException("count", "count is larger than buffer capacity");

            _currentCount = count;
        }

        public bool Expand(int limit = int.MaxValue)
        {
            if (_currentCount != 0)
                throw new InvalidOperationException("Cannot expand buffer while it is in use");

            //Check if expanding the buffer would exceed the limit
            var newMax = (uint)(1 << (_buffers.Count + 1)) - 1;
            if (newMax > limit)
                return false;

            //Expand the buffer
            _buffers.Add(new float[1 << _buffers.Count]);
            _maxCount = newMax;
            return true;
        }

        public float[] GetBuffer(ref uint count, bool zeroed = false)
        {
            if (count > _currentCount)
                throw new ArgumentOutOfRangeException("count", "count must be <= the total allocated size (set with Alloc(count))");
            if (count == 0)
                throw new ArgumentOutOfRangeException("count", "count must be > 0");

            //Find the largest array which fits within the requested amount
            for (var i = _buffers.Count - 1; i >= 0; i--)
            {
                var buf = _buffers[i];

                if (buf.Length <= count)
                {
                    //Subtract off the count the amount of space we've supplied
                    checked { count = (uint)(count - buf.Length); }

                    //Zero out the array as necessary and return it
                    if (zeroed)
                        Array.Clear(buf, 0, buf.Length);
                    return buf;
                }
            }

            //This should never happen!
            throw new InvalidOperationException("Failed to find a correctly sized buffer to service request");
        }
    }
}