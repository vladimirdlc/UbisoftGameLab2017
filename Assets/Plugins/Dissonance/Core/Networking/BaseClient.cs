using System;
using System.Collections.Generic;
using System.Threading;
using Dissonance.Datastructures;
using Dissonance.Networking.Client;

namespace Dissonance.Networking
{
    public abstract class BaseClient<TServer, TClient, TPeer>
        : IClient, IPacketProcessor
        where TPeer : IEquatable<TPeer>
        where TServer : BaseServer<TServer, TClient, TPeer>
        where TClient : BaseClient<TServer, TClient, TPeer>
    {
        #region helper types
        private enum EventType
        {
            PlayerJoined,
            PlayerLeft,
            PlayerStartedSpeaking,
            PlayerStoppedSpeaking,
            VoiceData,
            TextMessage
        }

        private struct NetworkEvent
        {
            public EventType Type;
            public string PlayerName;
            public VoicePacket VoicePacket;
            public TextMessage TextMessage;
        }
        #endregion

        #region fields and properties
        protected readonly Log Log;

        private readonly string _playerName;

        private readonly ChannelCollection _channels;
        private readonly CommsProcessor _commsProcessor;
        private readonly PacketRouter _router;
        private readonly ConnectionNegotiator _connectionNegotiator;
        private readonly RoomMembershipManager _roomsManager;
        private readonly PeerCollection _peers;

        private readonly List<NetworkEvent> _queuedEvents;
        private int _disconnectedEvents;

        public event Action<string> PlayerJoined;
        public event Action<string> PlayerLeft;
        public event Action<VoicePacket> VoicePacketReceived;
        public event Action<TextMessage> TextMessageReceived;
        public event Action<string> PlayerStartedSpeaking;
        public event Action<string> PlayerStoppedSpeaking;

        #pragma warning disable 0067 //warning 0067 - Disconnected is never used. This is incorrect we assign it to another field and then use that field i.e. var d = Disconnected; d()
        public event Action Disconnected;
        #pragma warning restore 0067

        private readonly ConcurrentPool<byte[]> _byteBufferPool;
        ConcurrentPool<byte[]> IClient.ByteBufferPool { get { return _byteBufferPool; } }

        private readonly TransferBuffer<ArraySegment<byte>> _queuedUnreliableTransmissions;
        private readonly TransferBuffer<ArraySegment<byte>> _queuedReliableTransmissions;
        
        public ushort? LocalId
        {
            get { return _peers.LocalPeerId; }
        }

        internal bool IsConnected
        {
            get { return _connectionNegotiator.State == ConnectionNegotiator.ConnectionState.Connected; }
        }
        #endregion

        #region constructor
        protected BaseClient(BaseCommsNetwork<TServer, TClient, TPeer> network)
            : this(network.PlayerName, network.Rooms, network.PlayerChannels, network.RoomChannels)
        {
        }

        private BaseClient(string playerName, Rooms rooms, PlayerChannels playerChannels, RoomChannels roomChannels)
        {
            if (playerName == null)
                throw new ArgumentNullException("playerName");
            if (rooms == null)
                throw new ArgumentNullException("rooms");
            if (playerChannels == null)
                throw new ArgumentNullException("playerChannels");
            if (roomChannels == null)
                throw new ArgumentNullException("roomChannels");

            Log = Logs.Create(LogCategory.Network, GetType().Name);

            _playerName = playerName;

            _byteBufferPool = new ConcurrentPool<byte[]>(10, () => new byte[1024]);
            _queuedUnreliableTransmissions = new TransferBuffer<ArraySegment<byte>>(16);
            _queuedReliableTransmissions = new TransferBuffer<ArraySegment<byte>>(16);
            _queuedEvents = new List<NetworkEvent>();

            _peers = new PeerCollection(this);
            _channels = new ChannelCollection(_peers.RoutingTable, playerChannels, roomChannels);
            _router = new PacketRouter(this);
            _commsProcessor = new CommsProcessor(this, _peers.RoutingTable, rooms, _channels);
            _connectionNegotiator = new ConnectionNegotiator(this);
            _roomsManager = new RoomMembershipManager(this, _connectionNegotiator, rooms);
        }
        #endregion

        #region connect/disconnect
        /// <summary>
        /// Override this to perform any work necessary to join a voice session
        /// </summary>
        public abstract void Connect();

        /// <summary>
        /// Call this once work has been done as we are now in a voice session
        /// </summary>
        protected void Connected()
        {
            _roomsManager.Start();
            _connectionNegotiator.Start();
        }

        /// <summary>
        /// Override this to perform any work necessary to leave a voice session
        /// </summary>
        public virtual void Disconnect()
        {
            _roomsManager.Dispose();
            _channels.Dispose();

            _peers.Clear();

            _commsProcessor.Stop();
            _connectionNegotiator.Stop();

            Log.Info("Disconnected");
        }

        #endregion

        #region update
        public virtual void Update()
        {
            _connectionNegotiator.Update();

            ReadMessages();
            SendUnreliableDataPackets();
            SendReliableDataPackets();

            _router.Update();
            _commsProcessor.Update();
            _roomsManager.Update();

            //Invoke all buffered events (now that we're on the main thread)
            DispatchEvents();
        }

        private void SendUnreliableDataPackets()
        {
            ArraySegment<byte> buffer;
            while (_queuedUnreliableTransmissions.Read(out buffer))
            {
                SendUnreliable(buffer);

                _byteBufferPool.Put(buffer.Array);
            }
        }

        private void SendReliableDataPackets()
        {
            ArraySegment<byte> buffer;
            while (_queuedReliableTransmissions.Read(out buffer))
            {
                SendReliable(buffer);

                _byteBufferPool.Put(buffer.Array);
            }
        }
        #endregion

        /// <summary>
        /// Send a packet of voice data from this client
        /// </summary>
        /// <param name="encodedAudio"></param>
        public void SendVoiceData(ArraySegment<byte> encodedAudio)
        {
            _commsProcessor.SendVoiceData(encodedAudio);
        }

        public void SendTextData(string data, ChannelType type, string recipient)
        {
            _commsProcessor.SendTextData(data, type, recipient);
        }

        #region abstract
        /// <summary>
        /// Read messages (call NetworkReceivedPacket with all messages)
        /// </summary>
        protected abstract void ReadMessages();

        /// <summary>
        /// Send a control packet (reliable, in-order) to the server
        /// </summary>
        /// <param name="packet">Packet to send</param>
        protected abstract void SendReliable(ArraySegment<byte> packet);

        /// <summary>
        /// Send an unreliable packet (unreliable, unordered) to the server
        /// </summary>
        /// <param name="packet">Packet to send</param>
        protected abstract void SendUnreliable(ArraySegment<byte> packet);
        #endregion

        #region packet processing
        public void NetworkReceivedPacket(ArraySegment<byte> data)
        {
            _router.NetworkReceivedPacket(data);
        }

        void IPacketProcessor.ReceiveHandshakeResponse(ref PacketReader reader)
        {
            _connectionNegotiator.ReceiveHandshakeResponse(ref reader);
        }

        void IPacketProcessor.ReceivePlayerRoutingUpdate(ref PacketReader reader)
        {
            _peers.ReceivePlayerRoutingUpdate(ref reader);
        }

        void IPacketProcessor.ReceiveTextData(ref PacketReader reader)
        {
            _commsProcessor.ReceiveTextData(ref reader);
        }

        void IPacketProcessor.ReceiveVoiceData(ref PacketReader reader)
        {
            _commsProcessor.ReceiveVoiceData(ref reader);
        }
        #endregion

        #region events handling
        private void DispatchEvents()
        {
            lock (_queuedEvents)
            {
                for (var i = 0; i < _queuedEvents.Count; i++)
                {
                    var e = _queuedEvents[i];

                    switch (e.Type)
                    {
                        case EventType.PlayerJoined:
                            InvokeEvent(ref e.PlayerName, PlayerJoined);
                            break;
                        case EventType.PlayerLeft:
                            InvokeEvent(ref e.PlayerName, PlayerLeft);
                            break;
                        case EventType.PlayerStartedSpeaking:
                            InvokeEvent(ref e.PlayerName, PlayerStartedSpeaking);
                            break;
                        case EventType.PlayerStoppedSpeaking:
                            InvokeEvent(ref e.PlayerName, PlayerStoppedSpeaking);
                            break;
                        case EventType.VoiceData:
                            InvokeEvent(ref e.VoicePacket, VoicePacketReceived);
                            _byteBufferPool.Put(e.VoicePacket.EncodedAudioFrame.Array);
                            break;
                        case EventType.TextMessage:
                            InvokeEvent(ref e.TextMessage, TextMessageReceived);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                _queuedEvents.Clear();
            }

            var dc = Disconnected;
            var count = Interlocked.Exchange(ref _disconnectedEvents, 0);
            if (dc != null)
            {
                for (var i = 0; i < count; i++)
                    dc();
            }
        }

        private void InvokeEvent<T>(ref T arg, Action<T> handler)
        {
            try
            {
                if (handler != null)
                    handler(arg);
            }
            catch (Exception e)
            {
                Log.Error("Exception invoking event handler: {0}", e);
            }
        }
        #endregion

        #region IClient implementation
        string IClient.PlayerName
        {
            get { return _playerName; }
        }

        void IClient.SendReliable(ArraySegment<byte> packet)
        {
            _queuedReliableTransmissions.Write(packet);
        }

        void IClient.SendUnreliable(ArraySegment<byte> packet)
        {
            _queuedUnreliableTransmissions.Write(packet);
        }

        void IClient.OnPlayerJoined(string playerName)
        {
            var id = _peers.RoutingTable.GetId(playerName);
            if (id.HasValue)
                _commsProcessor.ResetPlayerStats(id.Value);
            else
                Log.Warn("New player '{0}' joined session but does not have an ID");

            lock (_queuedEvents)
                _queuedEvents.Add(new NetworkEvent { Type = EventType.PlayerJoined, PlayerName = playerName });
        }

        void IClient.OnPlayerLeft(string playerName)
        {
            lock (_queuedEvents)
                _queuedEvents.Add(new NetworkEvent { Type = EventType.PlayerLeft, PlayerName = playerName });
        }

        void IClient.OnVoicePacketReceived(VoicePacket obj)
        {
            lock (_queuedEvents)
                _queuedEvents.Add(new NetworkEvent { Type = EventType.VoiceData, VoicePacket = obj });
        }

        void IClient.OnTextPacketReceived(TextMessage obj)
        {
            lock (_queuedEvents)
                _queuedEvents.Add(new NetworkEvent { Type = EventType.TextMessage, TextMessage = obj });
        }

        void IClient.OnPlayerStartedSpeaking(string playerName)
        {
            lock (_queuedEvents)
                _queuedEvents.Add(new NetworkEvent { Type = EventType.PlayerStartedSpeaking, PlayerName = playerName });

            Log.Debug("Remote player '{0}' began speaking.", playerName);
        }

        void IClient.OnPlayerStoppedSpeaking(string playerName)
        {
            lock (_queuedEvents)
                _queuedEvents.Add(new NetworkEvent { Type = EventType.PlayerStoppedSpeaking, PlayerName = playerName });

            Log.Debug("Remote player '{0}' stopped speaking.", playerName);
        }

        void IClient.OnDisconnected()
        {
            Interlocked.Increment(ref _disconnectedEvents);
        }
        #endregion
    }
}
