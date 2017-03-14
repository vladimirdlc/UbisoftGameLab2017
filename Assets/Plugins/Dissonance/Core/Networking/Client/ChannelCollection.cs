using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dissonance.Networking.Client
{
    internal class ChannelCollection
        : IDisposable
    {
        #region fields and properties
        private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(ChannelCollection).Name);

        private readonly IReadonlyRoutingTable _peers;
        private readonly PlayerChannels _playerChannels;
        private readonly RoomChannels _roomChannels;

        private readonly List<OpenChannel> _openChannels = new List<OpenChannel>();
        private readonly ReadOnlyCollection<OpenChannel> _openChannelsReadonly;
        #endregion

        #region constructor
        public ChannelCollection(IReadonlyRoutingTable peers, PlayerChannels playerChannels, RoomChannels roomChannels)
        {
            _peers = peers;
            _playerChannels = playerChannels;
            _roomChannels = roomChannels;
            _openChannelsReadonly = new ReadOnlyCollection<OpenChannel>(_openChannels);

            playerChannels.OpenedChannel += OpenPlayerChannel;
            playerChannels.ClosedChannel += ClosePlayerChannel;
            roomChannels.OpenedChannel += OpenRoomChannel;
            roomChannels.ClosedChannel += CloseRoomChannel;
            
            //There may already be some channels which were created before we created those events, run through them all now so we're up to date
            foreach (var playerChannel in playerChannels)
                OpenPlayerChannel(playerChannel.Value.TargetId, playerChannel.Value.Properties);
            foreach (var roomChannel in roomChannels)
                OpenRoomChannel(roomChannel.Value.TargetId, roomChannel.Value.Properties);
        }
        #endregion

        public void Dispose()
        {
            _openChannels.Clear();

            _playerChannels.OpenedChannel -= OpenPlayerChannel;
            _playerChannels.ClosedChannel -= ClosePlayerChannel;
            _roomChannels.OpenedChannel -= OpenRoomChannel;
            _roomChannels.ClosedChannel -= CloseRoomChannel;
        }

        public void GetChannels(IList<OpenChannel> channels)
        {
            lock (_openChannels)
                for (var i = 0; i < _openChannels.Count; i++)
                    channels.Add(_openChannels[i]);
        }

        public void CleanClosingChannels()
        {
            lock (_openChannels)
            {
                for (var i = _openChannels.Count - 1; i >= 0; i--)
                    if (_openChannels[i].IsClosing)
                        _openChannels.RemoveAt(i);
            }
        }

        private void OpenPlayerChannel(string player, ChannelProperties config)
        {
            var id = _peers.GetId(player);
            if (id == null)
            {
                Log.Warn("Unrecognized player ID '{0}'", player);
                return;
            }

            var channel = new OpenChannel(ChannelType.Player, config, false, id.Value);

            lock (_openChannels)
                _openChannels.Add(channel);
        }

        private void ClosePlayerChannel(string player, ChannelProperties config)
        {
            var id = _peers.GetId(player);
            if (id == null)
            {
                Log.Warn("Unrecognized player name '{0}'", player);
                return;
            }

            lock (_openChannels)
            {
                for (var i = _openChannels.Count - 1; i >= 0; i--)
                {
                    var channel = _openChannels[i];
                    if (!channel.IsClosing && channel.Type == ChannelType.Player && channel.Recipient == id.Value && ReferenceEquals(channel.Config, config))
                    {
                        _openChannels[i] = channel.AsClosing();
                        break;
                    }
                }
            }
        }

        private void OpenRoomChannel(string roomName, ChannelProperties config)
        {
            var id = roomName.ToRoomId();
            var channel = new OpenChannel(ChannelType.Room, config, false, id);

            lock (_openChannels)
                _openChannels.Add(channel);
        }

        private void CloseRoomChannel(string roomName, ChannelProperties config)
        {
            var roomId = roomName.ToRoomId();

            lock (_openChannels)
            {
                for (var i = _openChannels.Count - 1; i >= 0; i--)
                {
                    var channel = _openChannels[i];
                    if (!channel.IsClosing && channel.Type == ChannelType.Room && channel.Recipient == roomId && ReferenceEquals(channel.Config, config))
                    {
                        _openChannels[i] = channel.AsClosing();
                        break;
                    }
                }
            }
        }
    }
}
