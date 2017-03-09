using System;
using System.Collections.Generic;
using Dissonance.Extensions;

namespace Dissonance.Networking.Client
{
    /// <summary>
    /// Receives communications from other players and passes them onwards to the right place
    /// </summary>
    internal class CommsProcessor
    {
        #region fields and properties
        private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(CommsProcessor).Name);
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(0.6);

        private readonly IClient _client;
        private readonly IReadonlyRoutingTable _peers;
        private readonly Rooms _rooms;
        private readonly ChannelCollection _channels;

        private readonly List<ReceivingChannelStats> _receiving = new List<ReceivingChannelStats>();

        private readonly List<OpenChannel> _openChannelsBuffer = new List<OpenChannel>();
        private ushort _sequenceNumber;
        #endregion

        #region constructor
        public CommsProcessor(IClient client, IReadonlyRoutingTable peers, Rooms rooms, ChannelCollection channels)
        {
            _client = client;
            _peers = peers;
            _rooms = rooms;
            _channels = channels;
        }
        #endregion

        public void Stop()
        {
            //Stop all current voice channels we're receiving
            foreach (var stats in _receiving)
            {
                var name = _peers.GetName(stats.PlayerId);
                if (name != null)
                    _client.OnPlayerStoppedSpeaking(name);
            }

            _receiving.Clear();
        }

        public void ResetPlayerStats(ushort id)
        {
            if (id < _receiving.Count)
                _receiving[FindStatsIndex(id)] = new ReceivingChannelStats();
        }

        public void Update()
        {
            CheckTimeouts();
        }

        private void CheckTimeouts()
        {
            var now = DateTime.Now.Ticks;
            for (var i = _receiving.Count - 1; i >= 0; i--)
            {
                var stats = _receiving[i];
                if (stats.Open && now - stats.LastReceiptTicks > Timeout.Ticks)
                {
                    stats.Open = false;
                    _receiving[i] = stats;

                    _client.OnPlayerStoppedSpeaking(_peers.GetName(stats.PlayerId));
                }
            }
        }

        #region helpers
        private int FindStatsIndex(ushort senderId)
        {
            while (_receiving.Count <= senderId)
                _receiving.Add(new ReceivingChannelStats());

            return senderId;
        }

        private string FindRoomName(ushort roomId)
        {
            var name = _rooms.Name(roomId);
            if (name != null)
                return name;

            Log.Warn("Unknown room ID: {0}", roomId);
            return null;
        }

        private bool ChannelAddressesUs(ChannelBitField channel, ushort recipient)
        {
            if (channel.Type == ChannelType.Player)
                return recipient == _client.LocalId;

            return _rooms.Contains(recipient);
        }
        #endregion

        #region receive
        public void ReceiveVoiceData(ref PacketReader reader)
        {
            //Read header from voice packet
            byte options;
            ushort senderId, sequenceNumber, numChannels;
            reader.ReadVoicePacketHeader(out options, out senderId, out sequenceNumber, out numChannels);

            var playerName = _peers.GetName(senderId);

            //Read channel states
            var positional = true;
            var allClosing = true;
            var priority = ChannelPriority.None;
            for (var i = 0; i < numChannels; i++)
            {
                byte channelBitfield;
                ushort channelRecipient;
                reader.ReadVoicePacketChannel(out channelBitfield, out channelRecipient);

                var channel = new ChannelBitField(channelBitfield);
                if (ChannelAddressesUs(channel, channelRecipient))
                {
                    if (!channel.IsPositional)
                        positional = false;

                    if (!channel.IsClosing)
                        allClosing = false;

                    if (channel.Priority > priority)
                        priority = channel.Priority;
                }
            }

            //Read encoded voice data and copy it into another buffer (the packet will be recycled immediately, so we can't keep this frame around)
            var frameSegment = reader.ReadByteSegment();
            var frame = frameSegment.CopyTo(_client.ByteBufferPool.Get());

            //Read the magic number again
            var magic = reader.ReadUInt16();
            if (magic != PacketWriter.Magic)
            {
                Log.Warn("Corrupt audio packet, incorrect magic number sentinel. Expected {0}, got {1}", PacketWriter.Magic, magic);
                return;
            }

            //Update the statistics for the channel this data is coming in over
            var statsIndex = FindStatsIndex(senderId);
            var stats = _receiving[statsIndex];

            //If this channel is not currently open, open it (and reset sequence numbers etc)
            if (!_receiving[statsIndex].Open)
            {
                // check for old sequence numbers and discard
                if (stats.BaseSequenceNumber.WrappedDelta(sequenceNumber) < 0)
                    return;

                stats = new ReceivingChannelStats
                {
                    PlayerId = senderId,
                    BaseSequenceNumber = sequenceNumber,
                    LocalSequenceNumber = 0,
                    LastReceiptTicks = DateTime.Now.Ticks,
                    Open = true
                };

                _client.OnPlayerStartedSpeaking(playerName);
            }

            var sequenceDelta = stats.BaseSequenceNumber.WrappedDelta(sequenceNumber);
            if (stats.LocalSequenceNumber + sequenceDelta < 0)
            {
                // we must have received our first packet out of order
                // this "old" packet will give us a negative local sequence number, which will wrap when cast into the uint
                // discard the packet
                return;
            }

            stats.LastReceiptTicks = DateTime.Now.Ticks;
            stats.LocalSequenceNumber = (uint)(stats.LocalSequenceNumber + sequenceDelta);
            stats.BaseSequenceNumber = sequenceNumber;

            _client.OnVoicePacketReceived(new VoicePacket(playerName, priority, positional, frame, stats.LocalSequenceNumber));

            if (allClosing)
            {
                stats.Open = false;
                _client.OnPlayerStoppedSpeaking(playerName);
            }

            _receiving[statsIndex] = stats;
        }

        public void ReceiveTextData(ref PacketReader reader)
        {
            var packet = reader.ReadTextPacket(true);
            var recipientName = packet.RecipientType == ChannelType.Player ? _peers.GetName(packet.Recipient) : FindRoomName(packet.Recipient);

            if (recipientName != null)
            {
                _client.OnTextPacketReceived(new TextMessage(
                    _peers.GetName(packet.Sender),
                    packet.RecipientType,
                    recipientName,
                    packet.Text
                ));
            }
            else
            {
                Log.Error("Received a text message for an unknown {0} id={1}, Message: {2}", packet.RecipientType == ChannelType.Player ? "Player" : "Room", packet.Recipient, packet.Text);
            }
        }
        #endregion

        #region send
        public void SendVoiceData(ArraySegment<byte> encodedAudio)
        {
            if (!_client.LocalId.HasValue)
            {
                Log.Warn("Not received ID from Dissonance server; skipping voice packet transmission");
                return;
            }

            //Get a copy of the currently open channels
            _openChannelsBuffer.Clear();
            _channels.GetChannels(_openChannelsBuffer);

            //Write the voice and channel data into a network packet
            var packet = new PacketWriter(new ArraySegment<byte>(_client.ByteBufferPool.Get()))
                .WriteVoiceData(_client.LocalId.Value, ref _sequenceNumber, _openChannelsBuffer, encodedAudio)
                .Written;

            //Buffer up this packet to send ASAP
            _client.SendUnreliable(packet);

            //Clear up any channels which have been marked as "closing" (now that we know their status has been written into a packet)
            _channels.CleanClosingChannels();

        }

        public void SendTextData(string data, ChannelType type, string recipient)
        {
            if (!_client.LocalId.HasValue)
            {
                Log.Warn("Not received ID from Dissonance server; skipping text packet transmission");
                return;
            }

            var targetId = type == ChannelType.Player ? _peers.GetId(recipient) : recipient.ToRoomId();
            if (!targetId.HasValue)
            {
                Log.Warn("Unrecognised player name: '{0}'; skipping text packet transmission", recipient);
                return;
            }

            //Write the voice data into a network packet
            var packet = new PacketWriter(new ArraySegment<byte>(_client.ByteBufferPool.Get())).WriteTextPacket(_client.LocalId.Value, type, targetId.Value, data).Written;

            //Buffer up this packet to send ASAP
            _client.SendReliable(packet);
        }
        #endregion
    }
}
