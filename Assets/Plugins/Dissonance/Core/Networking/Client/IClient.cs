using System;
using Dissonance.Datastructures;

namespace Dissonance.Networking.Client
{
    internal interface IClient
    {
        ushort? LocalId { get; }

        string PlayerName { get; }

        ConcurrentPool<byte[]> ByteBufferPool { get; }

        void OnPlayerJoined(string playerName);

        void OnPlayerLeft(string playerName);

        void OnVoicePacketReceived(VoicePacket obj);

        void OnTextPacketReceived(TextMessage obj);

        void OnPlayerStartedSpeaking(string playerName);

        void OnPlayerStoppedSpeaking(string playerName);

        void OnDisconnected();

        void SendReliable(ArraySegment<byte> packet);

        void SendUnreliable(ArraySegment<byte> packet);
    }

    internal interface IPacketProcessor
    {
        void ReceivePlayerRoutingUpdate(ref PacketReader reader);

        void ReceiveVoiceData(ref PacketReader reader);

        void ReceiveTextData(ref PacketReader reader);

        void ReceiveHandshakeResponse(ref PacketReader reader);
    }
}
