using System.Collections.Generic;

namespace Dissonance.Networking.Server
{
    internal class ClientCollection<TPeer>
    {
        #region fields and properties
        private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(PacketRouter<TPeer>).Name);

        private readonly IServer<TPeer> _server;

        private readonly RoutingTable _playerIds;
        private readonly Dictionary<TPeer, ClientInfo> _clientsByConnection;
        private readonly Dictionary<ushort, List<TPeer>> _connectionsByRoom;
        private readonly Dictionary<ushort, TPeer> _connectionsByPlayerId;

        private readonly byte[] _routingTableBuffer = new byte[1024];
        #endregion

        #region constructors
        public ClientCollection(IServer<TPeer> server)
        {
            _server = server;

            _playerIds = new RoutingTable();
            _clientsByConnection = new Dictionary<TPeer, ClientInfo>();
            _connectionsByRoom = new Dictionary<ushort, List<TPeer>>();
            _connectionsByPlayerId = new Dictionary<ushort, TPeer>();
        }
        #endregion

        public void ProcessClientState(TPeer source, ref PacketReader reader)
        {
            var newClient = !_clientsByConnection.ContainsKey(source);
            ReadClientState(source, ref reader);
            RemoveDuplicateClients(source);

            if (newClient)
                SendPlayerRoutingTable();
        }

        private void ReadClientState(TPeer source, ref PacketReader reader)
        {
            // If this connection is new create a client info object for it now
            ClientInfo client;
            if (!_clientsByConnection.TryGetValue(source, out client))
            {
                var playerName = reader.ReadString();

                var id = _playerIds.Register(playerName);

                client = new ClientInfo(playerName, id);

                _clientsByConnection.Add(source, client);
                _connectionsByPlayerId[id] = source;

                _server.AddClient(source, client);
            }
            else
            {
                //This connection is not new, which means we can avoid reading player name (which would cost an allocation)
                reader.SkipString();
            }

            //Remove this client from all rooms
            for (var i = 0; i < client.Rooms.Count; i++)
                _connectionsByRoom[client.Rooms[i]].Remove(source);
            client.Rooms.Clear();

            //Add this client to the appropriate rooms
            var roomCount = reader.ReadUInt16();
            for (ushort i = 0; i < roomCount; i++)
            {
                var id = reader.ReadUInt16();
                client.Rooms.Add(id);

                List<TPeer> connectionsInRoom;
                if (!_connectionsByRoom.TryGetValue(id, out connectionsInRoom))
                {
                    connectionsInRoom = new List<TPeer>();
                    _connectionsByRoom.Add(id, connectionsInRoom);
                }

                connectionsInRoom.Add(source);
            }

            Log.Debug("Updated client state ('{0}', {1} rooms)", client.PlayerName, client.Rooms.Count);
        }

        private void RemoveDuplicateClients(TPeer sender)
        {
            var playerName = _clientsByConnection[sender].PlayerName;

            using (var connEnum = _clientsByConnection.Keys.GetEnumerator())
            {
                while (connEnum.MoveNext())
                {
                    var connection = connEnum.Current;

                    // ReSharper disable once PossibleNullReferenceException (Justification: dictionary key cannot be null, no neither can this)
                    if (!connection.Equals(sender))
                    {
                        if (_clientsByConnection[connection].PlayerName == playerName)
                        {
                            RemoveClient(connection);

                            //We've changed the collection so start again with a new enumerator
                            RemoveDuplicateClients(sender);
                            break;
                        }
                    }
                }
            }
        }

        public void RemoveClient(TPeer connection)
        {
            ClientInfo client;
            if (_clientsByConnection.TryGetValue(connection, out client))
            {
                //Remove this client from all rooms they are in
                for (var i = 0; i < client.Rooms.Count; i++)
                    _connectionsByRoom[client.Rooms[i]].Remove(connection);

                //Remove from dictionaries holding state
                _clientsByConnection.Remove(connection);
                _connectionsByPlayerId.Remove(client.PlayerId);

                //Remove from routing table (freeing up this ID)
                if (!_playerIds.Unregister(client.PlayerName))
                    Log.Error("Client disconnected ({0}), but could not find a client with the given name in the routing table", client.PlayerName);

                //Send out the new routing table to all peers remaining in the session
                SendPlayerRoutingTable();

                Log.Info("Client disconnected ({0}: {1})", connection, client.PlayerName);
            }
        }

        private void SendPlayerRoutingTable()
        {
            lock (_routingTableBuffer)
            {
                var writer = new PacketWriter(_routingTableBuffer)
                    .WritePlayerRoutingUpdate(_playerIds);

                //Broadcast routing update to all players
                using (var connEnum = _clientsByConnection.Keys.GetEnumerator())
                {
                    while (connEnum.MoveNext())
                    {
                        var connection = connEnum.Current;
                        _server.SendReliable(connection, writer.Written);
                    }
                }
            }
        }

        #region queries
        public int GetConnectionsInRoom(ushort room, List<TPeer> recipientsBuffer)
        {
            var counter = 0;
            List<TPeer> connectionsInRoom;
            if (_connectionsByRoom.TryGetValue(room, out connectionsInRoom))
            {
                foreach (var connection in connectionsInRoom)
                {
                    if (!recipientsBuffer.Contains(connection))
                    {
                        recipientsBuffer.Add(connection);
                        counter++;
                    }
                }
            }

            return counter;
        }

        public bool GetConnectionToPlayer(ushort player, out TPeer connection)
        {
            return _connectionsByPlayerId.TryGetValue(player, out connection);
        }
        #endregion
    }
}
