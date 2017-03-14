using Dissonance.Audio.Playback;

namespace Dissonance
{
    /// <summary>
    /// The state of a player in a Dissonance session
    /// </summary>
    public class VoicePlayerState
    {
        private readonly string _name;
        private readonly VoicePlayback _player;

        internal VoicePlayerState(VoicePlayback player)
        {
            _player = player;
            _name = player.PlayerName;
        }

        /// <summary>
        /// Get the name of the player this object represents
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Get a value indicating if this player is connected to the game
        /// </summary>
        public bool IsConnected
        {
            get { return _player != null && _player.isActiveAndEnabled && _player.PlayerName == _name; }
        }

        /// <summary>
        /// Get a value indicating if this player is speaking
        /// </summary>
        public bool IsSpeaking
        {
            get { return IsConnected && _player.IsSpeaking; }
        }

        /// <summary>
        /// The current amplitude of the voice signal from this player
        /// </summary>
        public float Amplitude
        {
            get { return IsConnected ? _player.Amplitude : 0; }
        }
    }
}
