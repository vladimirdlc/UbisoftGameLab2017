using System;

namespace Dissonance.Networking
{
    public struct VoicePacket
    {
        public readonly string SenderPlayerId;
        public readonly bool Positional;
        public readonly ChannelPriority Priority;
        public readonly ArraySegment<byte> EncodedAudioFrame;
        public readonly uint SequenceNumber;

        public VoicePacket(string senderPlayerId, ChannelPriority priority, bool positional, ArraySegment<byte> encodedAudioFrame, uint sequence)
        {
            SenderPlayerId = senderPlayerId;
            Priority = priority;
            Positional = positional;
            EncodedAudioFrame = encodedAudioFrame;
            SequenceNumber = sequence;
        }
    }
    
    public struct TextMessage
    {
        public readonly string Sender;
        public readonly ChannelType RecipientType;
        public readonly string Recipient;
        public readonly string Message;

        public TextMessage(string sender, ChannelType recipientType, string recipient, string message)
        {
            Sender = sender;
            RecipientType = recipientType;
            Recipient = recipient;
            Message = message;
        }
    }

    public interface ICommsNetwork
    {
        /// <summary>
        ///     Attempts a connection to the voice server.
        /// </summary>
        /// <param name="playerName">The name of the local player. Must be unique on the network.</param>
        /// <param name="rooms">The room membership collection the network should track.</param>
        /// <param name="playerChannels">The player channels collection the network should track.</param>
        /// <param name="roomChannels">The room channels collection the network should track.</param>
        /// <param name="connectionCallback">
        ///     A callback called once connection is complete.
        ///     Argument is <c>true</c> if connected successfully; else <c>false</c>.
        /// </param>
        void Initialize(string playerName, Rooms rooms, PlayerChannels playerChannels, RoomChannels roomChannels, Action<bool> connectionCallback);

        /// <summary>
        /// Event which is raised when a remote player joins the Dissonance session. Passed the unique ID of the player
        /// </summary>
        event Action<string> PlayerJoined;

        /// <summary>
        /// Event which is raised when a remote player leaves the Dissonance session. Passed the unique ID of the player
        /// </summary>
        event Action<string> PlayerLeft;

        /// <summary>
        /// Event which is raised when a voice packet is received
        /// </summary>
        event Action<VoicePacket> VoicePacketReceived;

        /// <summary>
        /// Event which is raised when a text packet is received
        /// </summary>
        event Action<TextMessage> TextPacketReceived;

        /// <summary>
        /// Event which is raised when a remote player begins speaking. Passed the unique ID of the player
        /// </summary>
        event Action<string> PlayerStartedSpeaking;

        /// <summary>
        /// Event which is raised when a remote player stops speaking. Passed the unique ID of the player
        /// </summary>
        event Action<string> PlayerStoppedSpeaking;

        /// <summary>
        /// Send the given voice data to the specified recipients.
        /// </summary>
        /// <remarks>The implementation of this method MUST NOT keep a reference to the given array beyond the scope of this method (the array is recycled for other uses)</remarks>
        /// <param name="data">The encoded audio data to send.</param>
        void SendVoice(ArraySegment<byte> data);

        /// <summary>
        /// Send a text message to a destination
        /// </summary>
        /// <param name="data">The message to send</param>
        /// <param name="recipientType">Type of recipinent for this message (either to a room or to a player)</param>
        /// <param name="recipientId">ID of the recipient (either a room ID or a player ID depending upon the recipinent type parameter)</param>
        void SendText(string data, ChannelType recipientType, string recipientId);
    }
}
