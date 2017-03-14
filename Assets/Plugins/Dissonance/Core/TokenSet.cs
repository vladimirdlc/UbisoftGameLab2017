using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dissonance
{
    [Serializable]
    public class TokenSet
        : IEnumerable<string>
    {
        private static readonly IComparer<string> _sortOrder = StringComparer.Ordinal;

        /// <remarks>
        /// This field contains the tokens which are currently in this set, sorted using the _sortOrder defined above
        /// Since they are sorted we can efficiently find items in the "set" using a binary search. This plays nice with
        /// unity serialization (which will serialize lists but not sets). It's probably also a marginal win in allocations.
        /// </remarks>
        // ReSharper disable once FieldCanBeMadeReadOnly.Local (Justification: Confuses unity serialization)
        [SerializeField] private List<string> _tokens = new List<string>();

        /// <summary>
        /// Number of tokens currently in the set
        /// </summary>
        public int Count
        {
            get { return _tokens.Count; }
        }

        public event Action<string> TokenRemoved;
        public event Action<string> TokenAdded;

        private int Find(string item)
        {
            if (item == null)
                return -1;

            var index = _tokens.BinarySearch(item, _sortOrder);
            return index < 0 ? -1 : index;
        }

        public bool ContainsToken(string token)
        {
            var index = Find(token);
            return index >= 0;
        }

        public bool AddToken(string token)
        {
            if (token == null)
                throw new ArgumentNullException("token", "Cannot add a null token");

            //Check if the collection already contains this token
            var index = _tokens.BinarySearch(token);
            if (index >= 0)
                return false;

            //since the item is *not* in the collection the return value is the complement
            //of the index of the next item in the collection (that's the contract of BinarySearch)
            _tokens.Insert(~index, token);

#if !NCRUNCH
            if (Application.isPlaying)
#endif
            {
                var act = TokenAdded;
                if (act != null)
                    act(token);
            }

            return true;
        }

        public bool RemoveToken(string token)
        {
            if (token == null)
                throw new ArgumentNullException("token", "Cannot remove a null token");

            var index = Find(token);
            if (index < 0)
                return false;

            _tokens.RemoveAt(index);

#if !NCRUNCH
            if (Application.isPlaying)
#endif
            {
                var act = TokenRemoved;
                if (act != null)
                    act(token);
            }

            return true;
        }

        public bool IntersectsWith(TokenSet other)
        {
            if (other == null)
                throw new ArgumentNullException("other", "Cannot intersect with null");

            var i = 0;
            var j = 0;
            while (i < _tokens.Count && j < other._tokens.Count)
            {
                var comparison = _sortOrder.Compare(_tokens[i], other._tokens[j]);
                if (comparison < 0)
                    i++;
                else if (comparison > 0)
                    j++;
                else
                    return true;
            }

            return false;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _tokens.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public interface IAccessTokenCollection
    {
        IEnumerable<string> Tokens { get; }

        bool ContainsToken(string token);

        bool AddToken(string token);

        bool RemoveToken(string token);
    }
}
