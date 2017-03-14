namespace Dissonance
{
    /// <summary>
    /// Describes the tradeoff of latency and bandwidth
    /// </summary>
    public enum FrameSize
    {
        /// <summary>
        /// Best latency, but highest bandwidth overhead
        /// </summary>
        Small,

        /// <summary>
        /// Average latency, average bandwidth usage
        /// </summary>
        Medium,

        /// <summary>
        /// Worst latency, but minimal bandwidth overhead
        /// </summary>
        Large
    }
}