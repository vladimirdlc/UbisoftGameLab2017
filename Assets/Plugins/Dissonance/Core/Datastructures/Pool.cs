using System;
using System.Collections.Generic;

namespace Dissonance.Datastructures
{
    /// <summary>
    /// A pool of resources which will always return an instance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Pool<T>
        : IRecycler<T>
        where T : class
    {
        private readonly int _maxSize;
        private readonly Func<T> _factory;
        
        private readonly Stack<T> _items;

        public int Count
        {
            get { return _items.Count; }
        }

        public int Capacity
        {
            get { return _maxSize; }
        }

        public Pool(int maxSize, Func<T> factory)
        {
            _maxSize = maxSize;
            _factory = factory;
            _items = new Stack<T>(maxSize);
        }

        /// <summary>
        /// Get an item from this pool
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            if (_items.Count > 0)
                return _items.Pop();

            return _factory();
        }

        /// <summary>
        /// Return an item to the pool
        /// </summary>
        /// <param name="item"></param>
        public void Put(T item)
        {
            if (_items.Count < _maxSize)
                _items.Push(item);
        }

        void IRecycler<T>.Recycle(T item)
        {
            Put(item);
        }
    }

    public interface IRecycler<in T> where T : class
    {
        void Recycle(T item);
    }
}
