using System;
using Dissonance.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace Dissonance.Integrations.UNet_LLAPI
{
    public class UNetCommsNetwork
        : BaseCommsNetwork<UNetServer, UNetClient, int>
    {
        private string _serverAddress;
        public string ServerAddress { get { return _serverAddress; } }

        [SerializeField]private int _maxConnections = 64;
        public int MaxConnections
        {
            get { return _maxConnections; }
        }

        private readonly int _voiceChannel;
        public int VoiceDataChannel
        {
            get { return _voiceChannel; }
        }

        private readonly int _sysChannel;
        public int SystemMessagesChannel
        {
            get { return _sysChannel; }
        }

        [SerializeField]private int _port = 5889;
        public ushort Port
        {
            get { return (ushort)_port; }
            set
            {
                if (ServerEnabled || ClientEnabled)
                {
                    throw Log.UserError(
                        "Attempting to set 'Port' when Dissonance session has already started",
                        "Modifying 'Port' property of UNetCommsNetwork from a script which runs after Dissonance has been started",
                        "https://dissonance.readthedocs.io/en/latest/Basics/Quick%20Start%20-%20UNet%20LLAPI/",
                        "7DE1905F-5951-4588-8D87-EDB689578DB9"
                    );
                }

                _port = value;
            }
        }

        private readonly HostTopology _topology;
        internal HostTopology Topology
        {
            get { return _topology; }
        }

        public UNetCommsNetwork()
        {
            var config = new ConnectionConfig();
            _voiceChannel = config.AddChannel(QosType.Unreliable);
            _sysChannel = config.AddChannel(QosType.ReliableSequenced);
            _topology = new HostTopology(config, MaxConnections);
        }

        protected override UNetServer CreateServer()
        {
            return new UNetServer(this);
        }

        protected override UNetClient CreateClient()
        {
            return new UNetClient(this);
        }

        protected override void Initialize()
        {
            NetworkTransport.Init();
        }

        protected override void Shutdown()
        {
            base.Shutdown();

            NetworkTransport.Shutdown();
        }

        public new void InitializeAsServer()
        {
            _serverAddress = "127.0.0.1";

            base.InitializeAsServer();
        }

        public void InitializeAsClient(string serverAddress)
        {
            // UNet doesn't like "localhost"
            if (serverAddress == "localhost")
                serverAddress = "127.0.0.1";
            _serverAddress = serverAddress;

            base.InitializeAsClient();
        }

        internal static bool Send(int socket, int connection, int channel, ArraySegment<byte> packet, Log log)
        {
            if (packet.Offset != 0)
                throw new ArgumentException("non-zero packet offset");

            byte error;
            var success = NetworkTransport.Send(socket, connection, channel, packet.Array, packet.Count, out error);

            if (!success)
            {
                log.Error("Error sending voice data: {0}", (NetworkError)error);

                if (FatalError((NetworkError)error, log))
                    return false;
            }

            return true;
        }

        internal static bool FatalError(NetworkError error, Log log)
        {
            switch (error)
            {
                case NetworkError.WrongHost:
                case NetworkError.WrongConnection:
                case NetworkError.WrongChannel:
                case NetworkError.NoResources:
                case NetworkError.Timeout:
                case NetworkError.VersionMismatch:
                case NetworkError.DNSFailure:
                case NetworkError.CRCMismatch:
                case NetworkError.BadMessage:
                    return true;

                case NetworkError.Ok:
                case NetworkError.MessageToLong:
                case NetworkError.WrongOperation:
                    return false;

                default:
                    log.Error("Dissonance UNet received unknown NetworkError: '{0}'", error);
                    return true;
            }
        }
    }
}
