using System;
using Dissonance.Audio.Codecs;
using Dissonance.Networking;
using NAudio.Wave;
using UnityEngine;

namespace Dissonance.Audio.Playback
{
    /// <summary>
    ///     Handles decoding and playing audio for a specific remote player.
    ///     Entities with this behaviour are created automatically by the DissonanceVoiceComms component.
    /// </summary>
    public class VoicePlayback
        : MonoBehaviour, IPriorityManager
    {
        #region fields and properties
        private static readonly Log Log = Logs.Create(LogCategory.Playback, "Voice Playback Component");

        private readonly SpeechSessionStream _sessions;

        public AudioSource AudioSource { get; private set; }
        public bool PositionTrackingAvailable { get; internal set; }

        private SamplePlaybackComponent _player;
        private WaveFormat _inputFormat;
        private uint _inputFrameSize;
        private float? _savedSpatialBlend;

        public string PlayerName
        {
            get { return _sessions.PlayerName; }
            internal set { _sessions.PlayerName = value; }
        }

        public bool IsSpeaking
        {
            get { return _player != null && _player.HasActiveSession; }
        }

        public float Amplitude
        {
            get { return _player == null ? 0 : _player.ARV; }
        }

        public ChannelPriority Priority
        {
            get
            {
                return _player.Session.HasValue ? _player.Session.Value.Priority : ChannelPriority.None;
            }
        }

        internal IPriorityManager PriorityManager { get; set; }
        #endregion

        public VoicePlayback()
        {
            _sessions = new SpeechSessionStream(this);
        }

        public void Awake()
        {
            AudioSource = GetComponent<AudioSource>();
            _player = GetComponent<SamplePlaybackComponent>();
        }

        public void OnEnable()
        {
            AudioSource.Stop();
            AudioSource.Play();
        }

        public void OnDisable()
        {
            _sessions.StopSession();
        }

        public void Update()
        {
            _sessions.Update();

            if (!_player.HasActiveSession && _sessions.Sessions.Count > 0)
                _player.Play(_sessions.Sessions.Dequeue());

            UpdatePositionalPlayback();
        }

        private void UpdatePositionalPlayback()
        {
            var session = _player.Session;
            if (session.HasValue)
            {
                if (PositionTrackingAvailable && session.Value.Positional)
                {
                    if (_savedSpatialBlend.HasValue)
                    {
                        Log.Debug("Changing to positional playback for {0}", PlayerName);
                        AudioSource.spatialBlend = _savedSpatialBlend.Value;
                        _savedSpatialBlend = null;
                    }
                }
                else
                {
                    if (!_savedSpatialBlend.HasValue)
                    {
                        Log.Debug("Changing to non-positional playback for {0}", PlayerName);
                        _savedSpatialBlend = AudioSource.spatialBlend;
                        AudioSource.spatialBlend = 0;
                    }
                }
            }
        }

        internal void SetFormat(WaveFormat inputFormat, uint inputFrameSize)
        {
            if (inputFormat == null)
                throw new ArgumentNullException("inputFormat", "Cannot set input wave format to be null");

            _inputFormat = inputFormat;
            _inputFrameSize = inputFrameSize;
        }

        internal void StartPlayback()
        {
            _sessions.StartSession(new FrameFormat {
                Codec = Codec.Opus,
                FrameSize = _inputFrameSize,
                WaveFormat = _inputFormat
            });
        }

        internal void StopPlayback()
        {
            _sessions.StopSession();
        }

        internal void ReceiveAudioPacket(VoicePacket packet)
        {
            _sessions.ReceiveFrame(packet);
        }

        ChannelPriority IPriorityManager.TopPriority
        {
            get { return PriorityManager == null ? ChannelPriority.Default : PriorityManager.TopPriority; }
        }
    }
}