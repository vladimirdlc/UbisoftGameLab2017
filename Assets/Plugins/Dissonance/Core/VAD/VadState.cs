namespace Dissonance.VAD
{
    internal enum VadState
    {
        /// <summary>
        /// VAD is in startup, background noises will be measured (no speech will be detected)
        /// </summary>
        Startup,

        /// <summary>
        /// VAD is currently detecting silence
        /// </summary>
        Silence,

        /// <summary>
        /// VAD has been detecting speech for a short amount of time
        /// </summary>
        ShortSpeech,

        /// <summary>
        /// VAD has been detecting speech for a long amount of time
        /// </summary>
        LongSpeech
    }
}
