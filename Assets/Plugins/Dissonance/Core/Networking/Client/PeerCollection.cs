using System;
using System.Linq;

namespace Dissonance.Networking.Client
{
    internal class PeerCollection
    {
        #region fields and properties
        private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(PeerCollection).Name);

        private readonly IClient _client;

        private readonly RoutingTable _playerIds = new RoutingTable();
        public IReadonlyRoutingTable RoutingTable
        {
            get { return _playerIds; }
        }

        public ushort? LocalPeerId { get; private set; }
        #endregion

        #region constructor
        public PeerCollection(IClient client)
        {
            _client = client;
        }
        #endregion

        /// <summary>
        /// Remove all players from the session
        /// </summary>
        public void Clear()
        {
            var players = _playerIds.Items.ToArray();
            foreach (var player in players)
                _client.OnPlayerLeft(player);
            _playerIds.Clear();
        }

        public void ReceivePlayerRoutingUpdate(ref PacketReader reader)
        {
            //This method allocates a few temporary lists. This is ok because it only happens very rarely - when a player joins or leaves the session
            Log.Debug("Received player routing table");

            //Keep a sorted list of all the players we knew about in the session before this update
            var oldPlayers = _playerIds.Items.ToArray();
            Array.Sort(oldPlayers);

            //Clear the routing table and overwrite it with the new data
            _playerIds.Clear();
            _playerIds.Deserialize(ref reader);

            //Check our own ID. If this isn't present something is wrong and bail out.
            LocalPeerId = _playerIds.GetId(_client.PlayerName);
            if (!LocalPeerId.HasValue)
            {
                Log.Warn("Received player routing update, cannot find self ID");
                return;
            }

            //Create a sorted list of all players currently in the session
            var currentPlayers = _playerIds.Items.ToArray();
            Array.Sort(currentPlayers);

            //Find which players are no longer in the session
            foreach (var player in oldPlayers)
                if (currentPlayers.Length == 0 || Array.BinarySearch(currentPlayers, player) < 0)
                    _client.OnPlayerLeft(player);

            foreach (var player in currentPlayers)
            {
                if (oldPlayers.Length == 0 || Array.BinarySearch(oldPlayers, player) < 0)
                {
                    var id = _playerIds.GetId(player);
                    if (id.HasValue)
                        _client.OnPlayerJoined(player);
                }
            }
        }
    }
}
