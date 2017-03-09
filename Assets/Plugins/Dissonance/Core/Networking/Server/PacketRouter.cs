using System.Collections.Generic;

namespace Dissonance.Networking.Server
{
    /// <summary>
    /// Receives packets and routes them forward to the appropriate clients
    /// </summary>
    internal class PacketRouter<TPeer>
    {
        #region fields and properties
        private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(PacketRouter<TPeer>).Name);

        private readonly IServer<TPeer> _server;

        private readonly List<TPeer> _recipientsBuffer = new List<TPeer>();
        #endregion

        #region constructor
        public PacketRouter(IServer<TPeer> server)
        {
            _server = server;
        }
        #endregion

        public void ProcessVoiceData(TPeer source, ref PacketReader reader)
        {
            //Read the fixed size header
            byte options;
            ushort senderId, sequenceNumber, numChannels;
            reader.ReadVoicePacketHeader(out options, out senderId, out sequenceNumber, out numChannels);

            //Read out the list of recipients for this voice packet
            _recipientsBuffer.Clear();
            for (var i = 0; i < numChannels; i++)
            {
                var channelBitfield = reader.ReadByte();
                var channelRecipient = reader.ReadUInt16();

                var channel = new ChannelBitField(channelBitfield);

                //Populate the _recipientsBuffer with peers listening to the channel
                GetChannelPeers(channel.Type, channelRecipient);
            }

            //Remove source peer from recipients buffer (we don't want to send voice back to where it came from)
            _recipientsBuffer.Remove(source);

            for (var i = 0; i < _recipientsBuffer.Count; i++)
                _server.SendUnreliable(_recipientsBuffer[i], reader.All);
        }

        public void ProcessTextData(ref PacketReader reader)
        {
            var txt = reader.ReadTextPacket(false);

            _recipientsBuffer.Clear();
            GetChannelPeers(txt.RecipientType, txt.Recipient);

            //Send the original packet out to everyone (including yourself if you are listening to the room)
            for (var i = 0; i < _recipientsBuffer.Count; i++)
                _server.SendReliable(_recipientsBuffer[i], reader.All);
        }

        private void GetChannelPeers(ChannelType channelType, ushort channelRecipient)
        {
            if (channelType == ChannelType.Room)
            {
                //We need to send this on to all players in the given room
                _server.GetConnectionsInRoom(channelRecipient, _recipientsBuffer);
            }
            else if (channelType == ChannelType.Player)
            {
                //We just need to send this to the single player who is being whispered to
                TPeer target;
                if (!_server.GetConnectionToPlayer(channelRecipient, out target))
                    Log.Warn("Cannot find network connection for ID '{0}'", channelRecipient);
                else
                    _recipientsBuffer.Add(target);
            }
            else
                Log.Error("Unknown message recipient type '{0}'", channelType);
        }
    }
}
