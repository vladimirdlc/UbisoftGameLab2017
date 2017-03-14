namespace Dissonance.Datastructures
{
    internal abstract class BaseWindowCalculator
    {
        #region fields and properties
        private readonly RingBuffer<float> _buffer;

        protected int Count
        {
            get { return _buffer.Count; }
        }

        protected int Capacity
        {
            get { return _buffer.Capacity; }
        }
        #endregion

        #region constructor
        protected BaseWindowCalculator(int size)
        {
            _buffer = new RingBuffer<float>(size);
        }
        #endregion

        public void Update(float added)
        {
            //Add a value and get back the value we removed (or null)
            var removed = _buffer.Add(added);

            //Keep a running total of what we have in the window
            UpdateSums(removed, added);
        }

        /// <summary>
        /// Update the sum and sum of squares variables to keep them up to date with the window
        /// </summary>
        /// <param name="removed">Value removed from the window</param>
        /// <param name="added">Value to add to the window</param>
        protected abstract void UpdateSums(float? removed, float added);

        public virtual void Clear()
        {
            _buffer.Clear();
        }
    }
}
