using System;
using Dissonance.Networking;
using UnityEngine.Networking;

namespace Dissonance.Integrations.UNet_LLAPI
{
    public class UNetClient
        : BaseClient<UNetServer, UNetClient, int>
    {
        private readonly UNetCommsNetwork _network;

        private readonly byte[] _readBuffer = new byte[1024];

        private int _socket = -1;
        private int _connection = -1;

        public UNetClient(UNetCommsNetwork network)
            : base(network)
        {
            _network = network;
        }

        public override void Connect()
        {
            byte error;
            _socket = NetworkTransport.AddHost(_network.Topology);
            _connection = NetworkTransport.Connect(_socket, _network.ServerAddress, _network.Port, 0, out error);

            if (error != (int)NetworkError.Ok)
                throw new DissonanceException(string.Format("Failed to connect to Dissonance server on port {0}, Error {1}", _network.Port, (NetworkError)error));
        }

        public override void Disconnect()
        {
            base.Disconnect();
            byte error;
            NetworkTransport.Disconnect(_socket, _connection, out error);

            if (error != (int)NetworkError.Ok)
                Log.Error("Failed to cleanly disconnect from Dissonance server at {0}:{1}, Error {2}", _network.ServerAddress, _network.Port, (NetworkError)error);
        }

        protected override void ReadMessages()
        {
            NetworkEventType eventType;

            do
            {
                int senderConnectionId;
                int channelId;
                int dataSize;
                byte error;

                eventType = NetworkTransport.ReceiveFromHost(_socket, out senderConnectionId, out channelId, _readBuffer, _readBuffer.Length, out dataSize, out error);

                if (error != 0)
                {
                    Log.Error("Error reading client socket: {0}", (NetworkError)error);

                    if (UNetCommsNetwork.FatalError((NetworkError)error, Log))
                    {
                        Disconnect();
                        return;
                    }
                }
                else
                {
                    switch (eventType)
                    {
                        case NetworkEventType.DataEvent:
                            NetworkReceivedPacket(new ArraySegment<byte>(_readBuffer, 0, dataSize));
                            break;

                        case NetworkEventType.ConnectEvent:
                            Connected();
                            break;

                        case NetworkEventType.DisconnectEvent:
                            Disconnect();
                            break;

                        case NetworkEventType.Nothing:
                        case NetworkEventType.BroadcastEvent:
                            break;

                        default:
                            Log.Error("Received unknown network event '{0}'", eventType);
                            break;
                    }
                }
            } while (eventType != NetworkEventType.Nothing);
        }

        private void Send(int channel, ArraySegment<byte> packet)
        {
            if (!UNetCommsNetwork.Send(_socket, _connection, channel, packet, Log))
                Disconnect();
        }

        protected override void SendReliable(ArraySegment<byte> packet)
        {
            Send(_network.SystemMessagesChannel, packet);
        }

        protected override void SendUnreliable(ArraySegment<byte> packet)
        {
            Send(_network.VoiceDataChannel, packet);
        }
    }
}
