using System;
using Dissonance.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace Dissonance.Integrations.UNet_HLAPI
{
    public class HlapiCommsNetwork
        : BaseCommsNetwork<HlapiServer, HlapiClient, HlapiConn>
    {
        // ReSharper disable UnassignedField.Global, FieldCanBeMadeReadOnly.Global, ConvertToConstant.Global (Justification: Changed by the editor in a way code inspector can't understand)
        public byte UnreliableChannel = 1;
        public byte ReliableSequencedChannel = 0;

        public short TypeCode = 18385;
        // ReSharper restore UnassignedField.Global, FieldCanBeMadeReadOnly.Global, ConvertToConstant.Global

        private bool _connecting;

        protected override HlapiServer CreateServer()
        {
            return new HlapiServer(this);
        }

        protected override HlapiClient CreateClient()
        {
            return new HlapiClient(this);
        }

        protected override void Update()
        {
            base.Update();

            if (_connecting)
            {
                //We need to wait until network is completely done setting up before we try to setup dissonance
                if (!(NetworkManager.singleton.isNetworkActive && (NetworkServer.active || NetworkClient.active)))
                    return;

                if (NetworkServer.active)
                    InitializeAsServer();
                else
                    InitializeAsClient();
                _connecting = false;
            }
        }

        protected override void Initialize()
        {
            //Register state tracker prefab
            ClientScene.RegisterPrefab(Resources.Load<GameObject>("HlapiPlayerTracker"));

            //Sanity check the channels
            if (UnreliableChannel >= NetworkManager.singleton.channels.Count)
            {
                throw Log.UserError(
                    "configured 'unreliable' channel is out of range",
                    "set the wrong channel number in the HLAPI Comms Network component",
                    "https://dissonance.readthedocs.io/en/latest/Basics/Quick%20Start%20-%20UNet%20HLAPI/",
                    "B19B4916-8709-490B-8152-A646CCAD788E"
                );
            }

            var unreliable = NetworkManager.singleton.channels[UnreliableChannel];
            if (unreliable != QosType.Unreliable)
            {
                throw Log.UserError(
                    string.Format("configured 'unreliable' channel has QoS type '{0}'", unreliable),
                    "not creating the channel with the correct QoS type",
                    "https://dissonance.readthedocs.io/en/latest/Basics/Quick%20Start%20-%20UNet%20HLAPI/",
                    "24ee53b1-7517-4672-8a4a-64a3e3c87ef6"
                );
            }

            if (ReliableSequencedChannel >= NetworkManager.singleton.channels.Count)
            {
                throw Log.UserError(
                    "configured 'reliable' channel is out of range",
                    "set the wrong channel number in the HLAPI Comms Network component",
                    "https://dissonance.readthedocs.io/en/latest/Basics/Quick%20Start%20-%20UNet%20HLAPI/",
                    "5F5F2875-ECC8-433D-B0CB-97C151B8094D"
                );
            }

            var reliable = NetworkManager.singleton.channels[ReliableSequencedChannel];
            if (reliable != QosType.ReliableSequenced)
            {
                throw Log.UserError(
                    string.Format("configured 'reliable sequenced' channel has QoS type '{0}'", reliable),
                    "not creating the channel with the correct QoS type",
                    "https://dissonance.readthedocs.io/en/latest/Basics/Quick%20Start%20-%20UNet%20HLAPI/",
                    "035773ec-aef3-477a-8eeb-c234d416171c"
                );
            }

            _connecting = true;
        }

        internal bool PreprocessPacketToClient(ArraySegment<byte> packet, HlapiConn destination)
        {
            //I have no idea if the HLAPI handles loopback. Whether it does or does not isn't important though - it's more
            //efficient to handle the loopback special case directly instead of passing through the entire network system!

            //This should never even be called if this peer is not the host!
            if (!ServerEnabled)
                throw Log.PossibleBug("server packet preprocessing running, but this peer is not a server", "8f9dc0a0-1b48-4a7f-9bb6-f767b2542ab1");

            //Is this loopback?
            if (NetworkManager.singleton.client.connection != destination.Connection)
                return false;

            //This is loopback!

            // check that we have a valid local client (in cases of startup or in-progress shutdowns)
            if (Client != null)
            {
                //Since this is loopback destination == source (by definition)
                Client.NetworkReceivedPacket(packet);
            }

            return true;
        }

        internal bool PreprocessPacketToServer(ArraySegment<byte> packet)
        {
            //I have no idea if the HLAPI handles loopback. Whether it does or does not isn't important though - it's more
            //efficient to handle the loopback special case directly instead of passing through the entire network system!

            //This should never even be called if this peer is not a client!
            if (!ClientEnabled)
                throw Log.PossibleBug("client packet processing running, but this peer is not a client", "dd75dce4-e85c-4bb3-96ec-3a3636cc4fbe");

            //Is this loopback?
            if (!ServerEnabled)
                return false;

            //This is loopback!

            //Since this is loopback destination == source (by definition)
            Server.NetworkReceivedPacket(new HlapiConn(NetworkManager.singleton.client.connection), packet);

            return true;
        }

        internal ArraySegment<byte> CopyToArraySegment(NetworkReader msg, ArraySegment<byte> segment)
        {
            var length = (int)msg.ReadPackedUInt32();
            if (length > segment.Count)
                throw Log.PossibleBug("receive buffer is too small", "A7387195-BF3D-4796-A362-6C64BB546445");

            for (var i = 0; i < length; i++)
                segment.Array[segment.Offset + i] = msg.ReadByte();

            return new ArraySegment<byte>(segment.Array, segment.Offset, length);
        }

        internal int CopyPacketToNetworkWriter(ArraySegment<byte> packet, NetworkWriter writer)
        {
            writer.SeekZero();
            writer.StartMessage(TypeCode);
            {
                //Length prefix the packet
                writer.WritePackedUInt32((uint)packet.Count);

                //Copy out the bytes.
                //You might think we could use `Write(buffer, offset, count)` here. You would be wrong! In that method the 'offset' is the
                //offset to write to in the packet! This is probably a bug in unity!
                for (var i = 0; i < packet.Count; i++) 
                    writer.Write(packet.Array[packet.Offset + i]); //
            }
            writer.FinishMessage();

            return writer.Position;
        }
    }

    public struct HlapiConn
        : IEquatable<HlapiConn>
    {
        public readonly NetworkConnection Connection;

        public HlapiConn(NetworkConnection connection)
            : this()
        {
            Connection = connection;
        }

        public bool Equals(HlapiConn other)
        {
            if (Connection == null)
            {
                if (other.Connection == null)
                    return true;
                return false;
            }

            return Connection.Equals(other.Connection);
        }
    }
}
