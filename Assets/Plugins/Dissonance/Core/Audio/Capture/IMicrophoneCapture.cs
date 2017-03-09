using System;
using NAudio.Wave;

namespace Dissonance.Audio.Capture
{
    /// <summary>
    /// Interface for capturing microphone samples
    /// </summary>
    public interface IMicrophoneCapture
        : IDisposable
    {
        /// <summary>
        /// Format of audio produced by the microphone
        /// </summary>
        WaveFormat Format { get; }

        /// <summary>
        /// Number of PCM samples which will be read at a time
        /// </summary>
        int FrameSize { get; }

        /// <summary>
        /// Subscribe a handler to raw PCM data from the microphone
        /// </summary>
        /// <param name="listener"></param>
        void Subscribe(IMicrophoneHandler listener);

        /// <summary>
        /// Unsubscribe a handler from receiving raw PCM data
        /// </summary>
        /// <param name="listener"></param>
        /// <returns>true; if the subscriber was unsubscribed. False if the given subscriber was not found</returns>
        bool Unsubscribe(IMicrophoneHandler listener);

        /// <summary>
        /// Capture frames of microphone input and send them on to subscribers
        /// </summary>
        /// <param name="transmit">If true, samples will be sent to subscribers. Otherwise samples will be discarded</param>
        void Update(bool transmit);
    }
}
