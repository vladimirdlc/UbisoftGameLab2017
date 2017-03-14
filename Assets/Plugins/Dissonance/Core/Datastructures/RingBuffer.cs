using System;
using System.Collections;
using System.Collections.Generic;

namespace Dissonance.Datastructures
{
    /// <summary>
    /// Stores the N most recently added items
    /// </summary>
    internal class RingBuffer<T>
        : IEnumerable<T>
        where T : struct
    {
        #region fields and properties
        private readonly T[] _items;

        /// <summary>
        /// Indicates the number of items added to the collection and currently stored
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// The size of this ring buffer
        /// </summary>
        public int Capacity
        {
            get { return _items.Length; }
        }

        private int _end;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public T this[int index]
        {
            get
            {
                if (index >= 0 && index < Count)
                    throw new IndexOutOfRangeException();

                if (Count < Capacity)
                    return _items[index];
                return _items[(_end + index) % _items.Length];
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        public RingBuffer(int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException();

            _items = new T[size];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public T? Add(T item)
        {
            //Save the item we are about to remove
            T? dequeued = null;
            if (Count == Capacity)
                dequeued = _items[_end];

            //Insert the new item
            _items[_end] = item;
            _end = (_end + 1) % _items.Length;

            //Update count (unless we wrapped around)
            if (Count < _items.Length)
                Count++;

            return dequeued;
        }

        public void Add(T[] items)
        {
            Add(new ArraySegment<T>(items));
        }

        public void Add(ArraySegment<T> items)
        {
            if (items.Count > Capacity)
            {
                //this single operation will overwrite the entire buffer
                //Reset buffer to empty and copy in last <capacity> items

                _end = Capacity;
                Count = Capacity;
                Array.Copy(items.Array, items.Offset + items.Count - Capacity, _items, 0, Capacity);
            }
            else
            {
                if (_end + items.Count > Capacity)
                {
                    // going to run off the end of the buffer;
                    // copy as much as we can then put the rest at the start of the buffer
                    var remainingSpace = Capacity - _end;
                    Array.Copy(items.Array, items.Offset, _items, _end, remainingSpace);
                    Array.Copy(items.Array, items.Offset + remainingSpace, _items, 0, items.Count - remainingSpace);
                    _end = (_end + items.Count) % _items.Length;
                }
                else
                {
                    // copy the data into the buffer
                    Array.Copy(items.Array, items.Offset, _items, _end, items.Count);
                    _end += items.Count;
                }

                Count = Math.Min(Count + items.Count, Capacity);
            }
        }

        /// <summary>
        /// Copy as much data as possible into the given array segment
        /// </summary>
        /// <param name="output"></param>
        /// <returns>A subsection of the given segment, which contains the data</returns>
        public ArraySegment<T> CopyTo(ArraySegment<T> output)
        {
            var count = Math.Min(Count, output.Count);
            var start = (_end + Capacity - Count) % Capacity;
            if (start + count < Capacity)
            {
                //We can copy all the data we need in a single operation (no wrapping)
                Array.Copy(_items, start, output.Array, output.Offset, count);
            }
            else
            {
                //Copying this much data wraps around, so we need 2 copies
                var cp = Capacity - start;
                Array.Copy(_items, start, output.Array, output.Offset, cp);
                Array.Copy(_items, 0, output.Array, output.Offset + cp, count - cp);
            }

            return new ArraySegment<T>(output.Array, output.Offset, count);
        }

        public void Clear()
        {
            Count = 0;
            _end = 0;
        }

        #region ienumerable
        //Using these enumerators in high performance code would be a performance disaster!
        //They exist purely to make object initializers work properly

        /// <summary>
        /// Enumerate items in the ringbuffer (oldest to newest)
        /// </summary>
        /// <returns></returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }
        #endregion
    }
}
