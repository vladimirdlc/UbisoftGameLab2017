namespace Dissonance.Audio.Playback
{
    public interface IPriorityManager
    {
        /// <summary>
        /// Get the highest priority of all current speakers
        /// </summary>
        ChannelPriority TopPriority { get; }
    }
}
