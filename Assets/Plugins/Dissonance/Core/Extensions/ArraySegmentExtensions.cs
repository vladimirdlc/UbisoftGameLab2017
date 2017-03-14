using System;

namespace Dissonance.Extensions
{
    internal static class ArraySegmentExtensions
    {
        /// <summary>
        /// Copy from the given array segment into the given array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="segment"></param>
        /// <param name="destination"></param>
        /// <param name="destinationOffset"></param>
        /// <returns>The segment of the destination array which was written into</returns>
        internal static ArraySegment<T> CopyTo<T>(this ArraySegment<T> segment, T[] destination, int destinationOffset = 0)
            where T : struct
        {
            if (segment.Count > destination.Length - destinationOffset)
                throw new ArgumentException("Insufficient space in destination array", "destination");

            Buffer.BlockCopy(segment.Array, segment.Offset, destination, destinationOffset, segment.Count);

            return new ArraySegment<T>(destination, destinationOffset, segment.Count);
        }

        /// <summary>
        /// Copy as many samples as possible from the source array into the segment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="segment"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static int CopyFrom<T>(this ArraySegment<T> segment, T[] source)
        {
            var count = Math.Min(segment.Count, source.Length);
            Array.Copy(source, 0, segment.Array, segment.Offset, count);
            return count;
        }

        internal static void Clear<T>(this ArraySegment<T> segment)
        {
            Array.Clear(segment.Array, segment.Offset, segment.Count);
        }
    }
}
