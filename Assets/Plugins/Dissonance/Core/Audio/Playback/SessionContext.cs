using System;

namespace Dissonance.Audio.Playback
{
    public struct SessionContext
    {
        /// <summary>
        /// Name of the player who is speaking in this session
        /// </summary>
        public readonly string PlayerName;

        /// <summary>
        /// Unique ID for this session (IDs may be re-used after a *very* long time)
        /// </summary>
        public readonly uint Id;

        public SessionContext(string playerName, uint id)
        {
            if (playerName == null)
                throw new ArgumentNullException("playerName", "Cannot create a session context with a null player name");

            PlayerName = playerName;
            Id = id;
        }
    }
}
