namespace Dissonance.Networking
{
    internal enum MessageTypes
        : byte
    {
        ClientState = 1,
        PlayerRoutingUpdate = 2,
        VoiceData = 3,
        TextData = 4,
        HandshakeRequest = 5,
        HandshakeResponse = 6
    }
}
