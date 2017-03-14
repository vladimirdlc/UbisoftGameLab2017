namespace Dissonance.VAD
{
    /// <summary>
    /// Listens for events from the voice detector
    /// </summary>
    public interface IVoiceActivationListener
    {
        /// <summary>
        /// Indicates that voice data should be transmitted
        /// </summary>
        void VoiceActivationStart();

        /// <summary>
        /// Indicates that voice data should stop being transmitted
        /// </summary>
        void VoiceActivationStop();
    }
}