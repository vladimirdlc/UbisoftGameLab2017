using System;
using System.Threading;

namespace Dissonance.Networking.Client
{
    internal class RoomMembershipManager
        : IDisposable
    {
        #region fields and properties
        private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(RoomMembershipManager).Name);

        private readonly IClient _client;
        private readonly IConnectionNegotiator _connection;
        private readonly Rooms _rooms;

        private bool _started;
        private int _stateDirty;
        #endregion

        #region constructor
        public RoomMembershipManager(IClient client, IConnectionNegotiator connection, Rooms rooms)
        {
            _client = client;
            _connection = connection;
            _rooms = rooms;

            _rooms.JoinedRoom += RoomMembershipChanged;
            _rooms.LeftRoom += RoomMembershipChanged;
        }
        #endregion

        private void RoomMembershipChanged(string id)
        {
            Interlocked.Increment(ref _stateDirty);
        }

        public void Start()
        {
            _started = true;
            Interlocked.Increment(ref _stateDirty);
        }

        public void Update()
        {
            if (!_started)
                return;

            SendState();
        }

        public void Dispose()
        {
            _started = false;

            _rooms.JoinedRoom -= RoomMembershipChanged;
            _rooms.LeftRoom -= RoomMembershipChanged;
        }

        private void SendState()
        {
            //Not connected, can't send state
            if (_connection.State != ConnectionNegotiator.ConnectionState.Connected)
                return;

            // not ready to send state yet
            if (_client.PlayerName == null)
                return;

            var count = Interlocked.Exchange(ref _stateDirty, 0);
            if (count > 0)
            {
                Log.Trace("Sending state ('{0}', {1} rooms)", _client.PlayerName, _rooms.Count);

                var packet = new PacketWriter(new ArraySegment<byte>(_client.ByteBufferPool.Get())).WriteClientState(_client.PlayerName, _rooms).Written;
                _client.SendReliable(packet);
            }
        }
    }
}
