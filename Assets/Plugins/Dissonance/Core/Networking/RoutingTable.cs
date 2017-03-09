using System;
using System.Collections.Generic;
using System.Linq;

namespace Dissonance.Networking
{
    internal sealed class RoutingTable
        : IReadonlyRoutingTable
    {
        private readonly List<string> _items;
        private readonly Stack<ushort> _freeIds;
        private readonly IEnumerable<string> _alive;

        public IEnumerable<string> Items
        {
            get { return _alive; }
        }

        internal int Count
        {
            get { return _items.Count; }
        }

        internal string this[ushort id]
        {
            get { return _items[id]; }
        }

        public RoutingTable()
        {
            _items = new List<string>();
            _freeIds = new Stack<ushort>();
            _alive = _items.Where(x => x != null);
        }

        public string GetName(ushort id)
        {
            if (id >= _items.Count)
                return null;

            return _items[id];
        }

        public ushort? GetId(string name)
        {
            for (ushort i = 0; i < _items.Count; i++)
            {
                if (_items[i] == name)
                    return i;
            }

            return null;
        }

        public ushort Register(string name)
        {
            var found = _items.IndexOf(name);
            if (found != -1)
                throw new InvalidOperationException(string.Format("Name is already in table with ID '{0}'", found));

            if (_freeIds.Count > 0)
            {
                var index = _freeIds.Pop();
                _items[index] = name;
                return index;
            }
            else
            {
                _items.Add(name);
                return (ushort)(_items.Count - 1);
            }
        }

        public bool Unregister(string name)
        {
            for (ushort i = 0; i < _items.Count; i++)
            {
                if (_items[i] == name)
                {
                    _items[i] = null;
                    _freeIds.Push(i);
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            _items.Clear();
            _freeIds.Clear();
        }

        public void Serialize(ref PacketWriter writer)
        {
            writer.Write((ushort)_items.Count);

            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];

                writer.Write(item);
            }
        }

        public void Deserialize(ref PacketReader reader)
        {
            Clear();

            var count = reader.ReadUInt16();

            for (ushort i = 0; i < count; i++)
            {
                var item = reader.ReadString();
                _items.Add(item);

                if (item == null)
                    _freeIds.Push(i);
            }
        }
    }

    internal interface IReadonlyRoutingTable
    {
        ushort? GetId(string player);

        string GetName(ushort id);
    }
}
