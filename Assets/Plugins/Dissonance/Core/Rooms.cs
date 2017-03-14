using System;
using System.Collections.Generic;

namespace Dissonance
{
    /// <summary>
    /// Collection of rooms the local client is listening to
    /// </summary>
    public sealed class Rooms
    {
        private static readonly IComparer<RoomMembership> _comparer = new RoomMembershipComparer();
        private readonly List<RoomMembership> _rooms;

        public event Action<string> JoinedRoom;
        public event Action<string> LeftRoom;

        internal Rooms()
        {
            _rooms = new List<RoomMembership>();
        }

        /// <summary>
        /// Number of rooms currently being listening to
        /// </summary>
        public int Count
        {
            get { return _rooms.Count; }
        }

        internal ushort this[int i]
        {
            get { return _rooms[i].RoomId; }
        }

        /// <summary>
        /// Checks if the collection contains the given room
        /// </summary>
        /// <param name="roomName"></param>
        /// <returns></returns>
        public bool Contains(string roomName)
        {
            var index = _rooms.BinarySearch(new RoomMembership(roomName, 0), _comparer);
            return index >= 0 && _rooms.Count > 0;
        }

        internal bool Contains(ushort roomId)
        {
            var index = _rooms.BinarySearch(new RoomMembership(roomId), _comparer);
            return index >= 0 && _rooms.Count > 0;
        }

        /// <summary>
        ///     Registers the local client as interested in broadcasts directed at the specified room.
        /// </summary>
        /// <param name="roomName">The room name.</param>
        public RoomMembership Join(string roomName)
        {
            if (roomName == null)
                throw new ArgumentNullException("roomName", "Cannot join a null room");

            var membership = new RoomMembership(roomName, 1);

            //Check to see if we already have this membership in the list
            var index = _rooms.BinarySearch(membership, _comparer);
            if (_rooms.Count == 0 || index < 0)
            {
                //Insert membership into list (making sure to keep it in order)
                var i = ~index;
                if (i == _rooms.Count)
                    _rooms.Add(membership);
                else
                    _rooms.Insert(i, membership);

                //newly joined, so invoke the event
                OnJoinedRoom(roomName);
            }
            else
            {
                //We're already in this room, increment the subscriber count
                var m = _rooms[index];
                m.Count++;
                _rooms[index] = m;
            }

            return membership;
        }

        /// <summary>
        ///     Unregisters the local client from broadcasts directed at the specified room.
        /// </summary>
        /// <param name="membership">The room membership.</param>
        public bool Leave(RoomMembership membership)
        {
            var index = _rooms.BinarySearch(membership, _comparer);

            if (index >= 0)
            {
                var m = _rooms[index];
                m.Count--;
                _rooms[index] = m;

                if (m.Count <= 0)
                {
                    _rooms.RemoveAt(index);
                    OnLeftRoom(membership.RoomName);

                    return true;
                }
            }

            return false;
        }

        private void OnJoinedRoom(string obj)
        {
            var handler = JoinedRoom;
            if (handler != null) handler(obj);
        }

        private void OnLeftRoom(string obj)
        {
            var handler = LeftRoom;
            if (handler != null) handler(obj);
        }

        internal string Name(ushort roomId)
        {
            if (_rooms.Count == 0)
                return null;

            var index = _rooms.BinarySearch(new RoomMembership(roomId), _comparer);

            if (index < 0)
                return null;

            return _rooms[index].RoomName;
        }
    }

    public struct RoomMembership
    {
        private readonly string _name;
        private readonly ushort _roomId;

        internal int Count;

        internal RoomMembership(string name, int count)
        {
            _name = name;
            _roomId = name.ToRoomId();
            Count = count;
        }

        internal RoomMembership(ushort id)
        {
            _name = null;
            _roomId = id;
            Count = 0;
        }

        public string RoomName
        {
            get { return _name; }
        }

        public ushort RoomId
        {
            get { return _roomId; }
        }
    }

    internal class RoomMembershipComparer
        : IComparer<RoomMembership>
    {
        public int Compare(RoomMembership x, RoomMembership y)
        {
            return x.RoomId.CompareTo(y.RoomId);
        }
    }
}
