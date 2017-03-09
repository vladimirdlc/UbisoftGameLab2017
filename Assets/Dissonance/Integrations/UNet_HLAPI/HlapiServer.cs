using System;
using Dissonance.Networking;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Dissonance.Integrations.UNet_HLAPI
{
    public class HlapiServer
        : BaseServer<HlapiServer, HlapiClient, HlapiConn>
    {
        #region fields and properties
        private readonly HlapiCommsNetwork _network;

        private readonly NetworkWriter _sendWriter;

        private readonly byte[] _receiveBuffer = new byte[1024];

        private GameObject _trackerPrefab;
        #endregion

        #region constructors
        public HlapiServer(HlapiCommsNetwork network)
        {
            _network = network;

            _sendWriter = new NetworkWriter(new byte[1024]);
            _trackerPrefab = Resources.Load<GameObject>("HlapiPlayerTracker");
        }
        #endregion

        public override void Connect()
        {
            base.Connect();
            NetworkServer.RegisterHandler(_network.TypeCode, OnMessageReceivedHandler);
        }

        private void OnMessageReceivedHandler(UnityEngine.Networking.NetworkMessage netmsg)
        {
            NetworkReceivedPacket(new HlapiConn(netmsg.conn), _network.CopyToArraySegment(netmsg.reader, new ArraySegment<byte>(_receiveBuffer)));
        }

        protected override void AddClient(HlapiConn peer, ClientInfo client)
        {
            base.AddClient(peer, client);

            //Create a game object which is controlled by the new client
            //When the client disconnects we will get a callback from this game object
            if (peer.Connection.isConnected)
            {
                var go = (GameObject)Object.Instantiate(_trackerPrefab, _network.transform);
                go.GetComponent<HlapiPlayerStateTracker>().Track(this, peer);
                go.name = string.Format("HLAPI State Tracker {0}", client.PlayerName);

                NetworkServer.SpawnWithClientAuthority(go, peer.Connection);
            }
        }

        internal void PlayerDisconnected(HlapiConn peer)
        {
            ClientDisconnected(peer);
        }

        public override void Disconnect()
        {
            base.Disconnect();
            NetworkServer.UnregisterHandler(_network.TypeCode);
        }

        protected override void ReadMessages()
        {
            //Messages are received in an event handler, so we don't need to do any work to read events
        }

        protected override void SendReliable(HlapiConn connection, ArraySegment<byte> packet)
        {
            Send(packet, connection, _network.ReliableSequencedChannel);
        }

        protected override void SendUnreliable(HlapiConn connection, ArraySegment<byte> packet)
        {
            Send(packet, connection, _network.UnreliableChannel);
        }

        private void Send(ArraySegment<byte> packet, HlapiConn connection, byte channel)
        {
            if (_network.PreprocessPacketToClient(packet, connection))
                return;

            var length = _network.CopyPacketToNetworkWriter(packet, _sendWriter);

            if (connection.Connection == null)
                Log.Error("Cannot send to a null destination");
            else if (!connection.Connection.SendBytes(_sendWriter.AsArray(), length, channel))
                Log.Error("Failed to send a message");
        }
    }
}
