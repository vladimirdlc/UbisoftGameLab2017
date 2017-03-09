using System;
using UnityEngine;

namespace Dissonance.Networking
{
    internal struct TextPacket
    {
        public readonly ushort Sender;
        public readonly ChannelType RecipientType;
        public readonly ushort Recipient;
        public readonly string Text;

        public TextPacket(ushort sender, ChannelType recipientType, ushort recipient, string text)
        {
            Sender = sender;
            RecipientType = recipientType;
            Recipient = recipient;
            Text = text;
        }
    }

    [RequireComponent(typeof(DissonanceComms))]
    public abstract class BaseCommsNetwork<TServer, TClient, TPeer>
        : MonoBehaviour, ICommsNetwork
        where TPeer : IEquatable<TPeer>
        where TServer : BaseServer<TServer, TClient, TPeer>
        where TClient : BaseClient<TServer, TClient, TPeer>
    {
        #region fields and properties
        protected readonly Log Log;

        private TServer _server;
        protected TServer Server
        {
            get { return _server; }
        }

        private TClient _client;
        protected TClient Client
        {
            get { return _client; }
        }

        private string _playerName;
        public string PlayerName { get { return _playerName; } }

        private bool _serverEnabled;
        public bool ServerEnabled { get { return _serverEnabled; } }

        private bool _clientEnabled;
        public bool ClientEnabled { get { return _clientEnabled; } }

        private DateTime _lastTriedClientConnect;

        private DissonanceComms _comms;
        public DissonanceComms Comms
        {
            get { return _comms; }
        }

        private Rooms _rooms;
        public Rooms Rooms { get { return _rooms; } }

        private PlayerChannels _playerChannels;
        public PlayerChannels PlayerChannels { get { return _playerChannels; } }

        private RoomChannels _roomChannels;
        public RoomChannels RoomChannels { get { return _roomChannels; } }
        #endregion

        protected BaseCommsNetwork()
        {
            Log = Logs.Create(LogCategory.Network, GetType().Name);
        }

        public virtual void Awake()
        {
            _comms = GetComponent<DissonanceComms>();
        }

        protected virtual void InitializeAsServer()
        {
            _serverEnabled = true;
            _clientEnabled = true;

            Disconnect();
        }

        protected virtual void InitializeAsClient()
        {
            _serverEnabled = false;
            _clientEnabled = true;

            Disconnect();
        }

        private Action<bool> _connectedCallback;

        private void Disconnect()
        {
            if (_server != null)
            {
                _server.Disconnect();
                _server = null;
            }

            if (_client != null)
            {
                _client.Disconnect();
                _client = null;
            }
        }

        // ReSharper disable once UnusedMemberHierarchy.Global (Justification: Used implicitly by unity)
        protected virtual void Update()
        {
            // start the server and client up if we have all required info and they are not started
            if (_playerName != null && _clientEnabled)
            {
                if (_server == null && _serverEnabled)
                    StartServer();

                if (_client == null)
                    StartClient();
            }

            //Call the "connected" callback if we've successfuly created a client
            if (_connectedCallback != null && _client != null)
            {
                if (_client.IsConnected)
                {
                    _connectedCallback(true);
                    _connectedCallback = null;
                }
            }

            // update server
            if (_server != null)
                _server.Update();

            // update client
            if (_client != null)
                _client.Update();
        }

        protected abstract TServer CreateServer();

        private void StartServer()
        {
            _server = CreateServer();
            _server.Connect();
        }

        protected abstract TClient CreateClient();

        private void StartClient()
        {
            if (DateTime.Now - _lastTriedClientConnect <= TimeSpan.FromSeconds(1))
                return;

            _lastTriedClientConnect = DateTime.Now;

            _client = CreateClient();
            _client.Connect();

            if (_client != null)
            {
                Log.Trace("Subscribing to network events");

                _client.PlayerJoined += OnPlayerJoined;
                _client.PlayerLeft += OnPlayerLeft;
                _client.VoicePacketReceived += OnVoicePacketReceived;
                _client.TextMessageReceived += OnTextPacketReceived;
                _client.PlayerStartedSpeaking += OnPlayerStartedSpeaking;
                _client.PlayerStoppedSpeaking += OnPlayerStoppedSpeaking;

                _client.Disconnected += Client_Disconnected;
            }
        }

        private void Client_Disconnected()
        {
            _client.PlayerJoined -= OnPlayerJoined;
            _client.PlayerLeft -= OnPlayerLeft;
            _client.VoicePacketReceived -= VoicePacketReceived;
            _client.TextMessageReceived -= TextPacketReceived;
            _client.PlayerStartedSpeaking -= OnPlayerStartedSpeaking;
            _client.PlayerStoppedSpeaking -= OnPlayerStoppedSpeaking;
            _client.Disconnected -= Client_Disconnected;

            _client = null;
        }

        public virtual void StopClient()
        {
            _clientEnabled = false;

            if (_client != null)
            {
                _client.Disconnect();
                _client = null;
            }
        }

        public virtual void StopServer()
        {
            _serverEnabled = false;

            if (_server != null)
            {
                StopClient();

                _server.Disconnect();
                _server = null;
            }
        }

        protected abstract void Initialize();

        protected virtual void Shutdown()
        {
            Disconnect();
        }

        // ReSharper disable once UnusedMember.Local (Justification: Used implicitly by unity)
        private void OnDisable()
        {
            Shutdown();
        }

        #region IVoiceNetwork
        public void Initialize(string playerName, Rooms rooms, PlayerChannels playerChannels, RoomChannels roomChannels, Action<bool> connectionCallback)
        {
            if (playerName == null)
                throw new ArgumentNullException("playerName");
            if (rooms == null)
                throw new ArgumentNullException("rooms");
            if (playerChannels == null)
                throw new ArgumentNullException("playerChannels");
            if (roomChannels == null)
                throw new ArgumentNullException("roomChannels");
            if (connectionCallback == null)
                throw new ArgumentNullException("connectionCallback");

            // disconnect if any details have changed
            if (_playerName != playerName || _rooms != rooms || _playerChannels != playerChannels || _roomChannels != roomChannels)
            {
                _playerName = playerName;
                _rooms = rooms;
                _playerChannels = playerChannels;
                _roomChannels = roomChannels;

                Disconnect();
            }

            Initialize();

            _connectedCallback = connectionCallback;
        }

        public event Action<string> PlayerJoined;
        public event Action<string> PlayerLeft;
        public event Action<VoicePacket> VoicePacketReceived;
        public event Action<TextMessage> TextPacketReceived;
        public event Action<string> PlayerStartedSpeaking;
        public event Action<string> PlayerStoppedSpeaking;

        public void SendVoice(ArraySegment<byte> data)
        {
            if (_client != null)
                _client.SendVoiceData(data);
        }

        public void SendText(string data, ChannelType recipientType, string recipientId)
        {
            if (_client != null)
                _client.SendTextData(data, recipientType, recipientId);
        }
        #endregion

        #region event invokers
        private void OnPlayerJoined(string obj)
        {
            var handler = PlayerJoined;
            if (handler != null) handler(obj);
        }

        private void OnPlayerLeft(string obj)
        {
            var handler = PlayerLeft;
            if (handler != null) handler(obj);
        }

        private void OnVoicePacketReceived(VoicePacket obj)
        {
            var handler = VoicePacketReceived;
            if (handler != null) handler(obj);
        }

        private void OnTextPacketReceived(TextMessage obj)
        {
            var handler = TextPacketReceived;
            if (handler != null) handler(obj);
        }

        private void OnPlayerStartedSpeaking(string obj)
        {
            var handler = PlayerStartedSpeaking;
            if (handler != null) handler(obj);
        }

        private void OnPlayerStoppedSpeaking(string obj)
        {
            var handler = PlayerStoppedSpeaking;
            if (handler != null) handler(obj);
        }
        #endregion
    }
}
