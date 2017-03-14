using System;
using System.Collections.Generic;
using Dissonance.Datastructures;
using Dissonance.Networking;

namespace Dissonance.Audio.Playback
{
    internal interface IJitterEstimator
    {
        float Jitter { get; }
    }

    /// <summary>
    ///     Converts the sequence of stream start/stop and packet delivery events from the network into a sequence of
    ///     <see cref="SpeechSession" />.
    /// </summary>
    internal class SpeechSessionStream : IJitterEstimator
    {   
        private static readonly Log Log = Logs.Create(LogCategory.Playback, typeof (SpeechSessionStream).Name);
        private static readonly TimeSpan InitialBufferDelay = TimeSpan.FromMilliseconds(100);

        // shared pool of decoder pipelines
        private static readonly Dictionary<FrameFormat, Stack<DecoderPipeline>> FreePipelines = new Dictionary<FrameFormat, Stack<DecoderPipeline>>();
        
        private readonly Queue<SpeechSession> _sessions;
        private readonly Queue<SpeechSession> _awaitingActivation;
        private readonly IPriorityManager _priorityManager;
        private readonly WindowDeviationCalculator _jitter;

        private DecoderPipeline _active;
        private SpeechSession _activeSession;
        private string _playerName;
        private uint _currentId;

        private TimeSpan _frameDuration;
        private DateTime? _firstFrameArrival;
        private uint _firstFrameSeq;

        /// <summary>
        ///     Gets a queue of <see cref="SpeechSession" /> which are awaiting playback.
        /// </summary>
        public Queue<SpeechSession> Sessions
        {
            get { return _sessions; }
        }

        public string PlayerName
        {
            get { return _playerName; }
            set { _playerName = value; }
        }

        public float Jitter
        {
            get { return (_jitter.StdDev * 2.5f) * _jitter.Confidence + (float) InitialBufferDelay.TotalSeconds * (1 - _jitter.Confidence); }
        }

        public SpeechSessionStream(IPriorityManager priorityManager)
        {
            _priorityManager = priorityManager;
            _sessions = new Queue<SpeechSession>();
            _awaitingActivation = new Queue<SpeechSession>();
            _jitter = new WindowDeviationCalculator(32);
        }

        /// <summary>
        ///     Starts a new speech session.
        /// </summary>
        /// <param name="format">The frame format.</param>
        public void StartSession(FrameFormat format)
        {
            Log.Info("Creating new speech session with buffer time of {0}ms", Jitter);

            var pipeline = CreateDecoderPipeline(format);

            _active = pipeline;
            _activeSession = SpeechSession.Create(new SessionContext(_playerName, unchecked(_currentId++)), format.WaveFormat, this, pipeline);
            _awaitingActivation.Enqueue(_activeSession);
            _frameDuration = TimeSpan.FromSeconds((double)format.FrameSize / format.WaveFormat.SampleRate);
            _firstFrameArrival = null;
            _firstFrameSeq = 0;
        }

        /// <summary>
        ///     Queues an encoded audio frame for playback in the current session.
        /// </summary>
        /// <param name="packet"></param>
        public void ReceiveFrame(VoicePacket packet)
        {
            _active.Push(packet);

            // calculate how late the packet is
            if (!_firstFrameArrival.HasValue)
            {
                _firstFrameArrival = DateTime.Now;
                _firstFrameSeq = packet.SequenceNumber;
            }
            else
            {
                var expectedTime = _firstFrameArrival.Value + TimeSpan.FromTicks(_frameDuration.Ticks * (packet.SequenceNumber - _firstFrameSeq));
                var delay = DateTime.Now - expectedTime;
                _jitter.Update((float)delay.TotalSeconds);
            }
        }

        /// <summary>
        ///     Stops the current session.
        /// </summary>
        public void StopSession()
        {
            if (_active != null)
                _active.Stop();
        }

        /// <summary>
        ///     Publishes new sessions which are due playback.
        /// </summary>
        public void Update()
        {
            while (_awaitingActivation.Count > 0 && _awaitingActivation.Peek().ActivationTime < DateTime.Now)
            {
                var session = _awaitingActivation.Dequeue();
                _sessions.Enqueue(session);

                Log.Info("Publishing audio session ({0} items in buffer)", session.BufferCount);
            }
        }

        private DecoderPipeline CreateDecoderPipeline(FrameFormat format)
        {
            Stack<DecoderPipeline> pool;
            if (!FreePipelines.TryGetValue(format, out pool))
            {
                pool = new Stack<DecoderPipeline>();
                FreePipelines[format] = pool;
            }

            if (pool.Count > 0)
                return pool.Pop();

            var decoder = DecoderFactory.Create(format);
            return new DecoderPipeline(_priorityManager, decoder, format.FrameSize, p =>
            {
                p.Reset();
                pool.Push(p);
            });
        }
    }
}