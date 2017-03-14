using System;
using System.Threading;

namespace Dissonance.Datastructures
{
    internal class TransferBuffer<T>
    {
        private readonly T[] _buffer;
        private volatile int _readHead;
        private volatile int _unread;
        private volatile int _writeHead;

        private readonly T[] _singleReadItem = new T[1];
        private readonly T[] _singleWriteItem = new T[1];

        /// <summary>
        /// Get an estimate of the amount of data in the buffer. This is only an estimate because of data races with other threads
        /// </summary>
        public int EstimatedUnreadCount
        {
            get { return _unread; }
        }

        public int Capacity
        {
            get { return _buffer.Length; }
        }

        public TransferBuffer(int capacity = 4096)
        {
            _buffer = new T[capacity];
        }

        #region write
        public bool Write(T item)
        {
            _singleWriteItem[0] = item;
            var success = Write(_singleWriteItem);
            _singleWriteItem[0] = default(T);
            return success;
        }

        public bool Write(T[] data)
        {
            return Write(new ArraySegment<T>(data, 0, data.Length));
        }

        public bool Write(ArraySegment<T> data)
        {
            // check if we have enough space in the buffer
            if (_unread + data.Count > _buffer.Length)
                return false;

            if (_writeHead + data.Count > _buffer.Length)
            {
                // going to run off the end of the buffer;
                // copy as much as we can then put the rest at the start of the buffer
                var remainingSpace = _buffer.Length - _writeHead;
                Array.Copy(data.Array, data.Offset, _buffer, _writeHead, remainingSpace);
                Array.Copy(data.Array, data.Offset + remainingSpace, _buffer, 0, data.Count - remainingSpace);
                _writeHead = (_writeHead + data.Count) % _buffer.Length;
            }
            else
            {
                // copy the data into the buffer
                Array.Copy(data.Array, data.Offset, _buffer, _writeHead, data.Count);
                _writeHead += data.Count;
            }

#pragma warning disable 420
            // Justification: It's Interlocked, so volatile isn't important (See: http://stackoverflow.com/a/425150/108234 )
            Interlocked.Add(ref _unread, data.Count);
#pragma warning restore 420

            return true;
        }
        #endregion

        #region read
        public bool Read(out T item)
        {
            var success = Read(_singleReadItem);
            item = success ? _singleReadItem[0] : default(T);
            _singleReadItem[0] = default(T);
            return success;
        }

        public bool Read(T[] data)
        {
            return Read(new ArraySegment<T>(data, 0, data.Length));
        }

        public bool Read(T[] data, int readCount)
        {
            if (readCount > data.Length)
                throw new ArgumentException("Requested read amount is > size of supplied output buffer", "readCount");

            return Read(new ArraySegment<T>(data, 0, readCount));
        }

        public bool Read(ArraySegment<T> data)
        {
            if (_unread < data.Count)
                return false;

            if (_readHead + data.Count > _buffer.Length)
            {
                // going to run off the end of the buffer;
                // copy as much as we can then start reading the rest from the start of the buffer
                var remainingSpace = _buffer.Length - _readHead;
                Array.Copy(_buffer, _readHead, data.Array, data.Offset, remainingSpace);
                Array.Copy(_buffer, 0, data.Array, data.Offset + remainingSpace, data.Count - remainingSpace);
                _readHead = (_readHead + data.Count) % _buffer.Length;
            }
            else
            {
                // copy the data out of the buffer
                Array.Copy(_buffer, _readHead, data.Array, data.Offset, data.Count);
                _readHead += data.Count;
            }

#pragma warning disable 420
            // Justification: It's Interlocked, so volatile isn't important (See: http://stackoverflow.com/a/425150/108234 )
            Interlocked.Add(ref _unread, -data.Count);
#pragma warning restore 420

            return true;
        }
        #endregion

        public void Clear()
        {
            _readHead = 0;
            _writeHead = 0;
            _unread = 0;
        }
    }
}