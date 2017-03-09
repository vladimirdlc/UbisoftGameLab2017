using System;
using Dissonance.Networking;
using UnityEngine.Networking;

namespace Dissonance.Integrations.UNet_HLAPI
{
    public class HlapiClient
        : BaseClient<HlapiServer, HlapiClient, HlapiConn>
    {
        #region fields and properties
        private readonly HlapiCommsNetwork _network;

        private readonly NetworkWriter _sendWriter;

        private readonly byte[] _receiveBuffer = new byte[1024];
        #endregion

        #region constructors
        public HlapiClient(HlapiCommsNetwork network)
            : base(network)
        {
            _network = network;

            _sendWriter = new NetworkWriter(new byte[1024]);
        }
        #endregion

        public override void Connect()
        {
            //we handle loopback explicitly, so if the server is locally hosted we don't need to register the network handler
            //This is important because otherwise we'd overwrite the server message handler!
            if (!_network.ServerEnabled)
                NetworkManager.singleton.client.RegisterHandler(_network.TypeCode,OnMessageReceivedHandler);

            Connected();
        }


        private void OnMessageReceivedHandler(UnityEngine.Networking.NetworkMessage netMsg)
        {
            NetworkReceivedPacket(_network.CopyToArraySegment(netMsg.reader, new ArraySegment<byte>(_receiveBuffer)));
        }

        protected override void ReadMessages()
        {
            //Messages are received in an event handler, so we don't need to do any work to read events
        }

        protected override void SendReliable(ArraySegment<byte> packet)
        {
            Send(packet, _network.ReliableSequencedChannel);
        }

        protected override void SendUnreliable(ArraySegment<byte> packet)
        {
            Send(packet, _network.UnreliableChannel);
        }

        private void Send(ArraySegment<byte> packet, byte channel)
        {
            if (_network.PreprocessPacketToServer(packet))
                return;

            var length = _network.CopyPacketToNetworkWriter(packet, _sendWriter);

            if (!NetworkManager.singleton.client.connection.SendBytes(_sendWriter.AsArray(), length, channel))
                Log.Error("Failed to send a message");
        }
    }
}
