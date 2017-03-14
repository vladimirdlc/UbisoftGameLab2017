using System;
using NAudio.Wave;

namespace Dissonance.Audio.Playback
{
    /// <summary>
    ///     Represents a decoder pipeline for a single playback session.
    /// </summary>
    public struct SpeechSession
    {
        private static readonly float MinimumDelay = 0.050f;

        public readonly WaveFormat WaveFormat;

        private readonly IDecoderPipeline _pipeline;
        private readonly SessionContext _context;
        private readonly DateTime _creationTime;
        private readonly IJitterEstimator _jitter;

        public int BufferCount { get { return _pipeline.BufferCount; } }
        public SessionContext Context { get { return _context; } }
        public ChannelPriority Priority { get { return _pipeline.Priority; } }
        public bool Positional { get { return _pipeline.Positional; } }

        public DateTime ActivationTime
        {
            get { return _creationTime + Delay; }
        }

        public TimeSpan Delay
        {
            get
            {
                var delay = Math.Max(MinimumDelay, _jitter.Jitter);
                return TimeSpan.FromSeconds(delay);
            }
        }

        private SpeechSession(SessionContext context, WaveFormat waveFormat, IJitterEstimator jitter, IDecoderPipeline pipeline)
        {
            _context = context;
            _pipeline = pipeline;
            _creationTime = DateTime.Now;
            _jitter = jitter;

            WaveFormat = waveFormat;
        }

        internal static SpeechSession Create(SessionContext context, WaveFormat format, IJitterEstimator jitter, IDecoderPipeline pipeline)
        {
            return new SpeechSession(context, format, jitter, pipeline);
        }

        public void Prepare()
        {
            _pipeline.Prepare(_context);
        }

        /// <summary>
        ///     Pulls the specfied number of samples from the pipeline, decoding packets as necessary.
        /// </summary>
        /// <param name="samples"></param>
        /// <returns><c>true</c> if there are more samples available; else <c>false</c>.</returns>
        public bool Read(ArraySegment<float> samples)
        {
            return _pipeline.Read(samples);
        }
    }
}