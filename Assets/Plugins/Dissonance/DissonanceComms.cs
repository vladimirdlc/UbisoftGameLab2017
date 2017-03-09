using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dissonance.Audio.Capture;
using Dissonance.Audio.Codecs;
using Dissonance.Audio.Codecs.Opus;
using Dissonance.Audio.Playback;
using Dissonance.Config;
using Dissonance.Datastructures;
using Dissonance.Networking;
using Dissonance.VAD;
using NAudio.Wave;
using UnityEngine;

namespace Dissonance
{
    /// <summary>
    ///     The central Dissonance Voice Comms component.
    ///     Place one of these on a voice comm entity near the root of your scene.
    /// </summary>
    /// <remarks>
    ///     Handles recording the local player's microphone and sending the data to the network.
    ///     Handles managing the playback entities for the other users on the network.
    ///     Provides the API for opening and closing channels.
    /// </remarks>
    public class DissonanceComms
        : MonoBehaviour, IPriorityManager, IAccessTokenCollection, IChannelPriorityProvider
    {
        #region fields
        private static readonly Log Log = Logs.Create(LogCategory.Core, "Dissonance Comms Component");

        private bool _started;

        private readonly List<IDissonancePlayer> _trackedPlayers = new List<IDissonancePlayer>();
        private readonly Dictionary<string, VoicePlayback> _playback = new Dictionary<string, VoicePlayback>();
        private readonly Pool<VoicePlayback> _playbackPool;

        private readonly List<IVoiceActivationListener> _acitvationListeners = new List<IVoiceActivationListener>();
        private readonly Rooms _rooms;
        private readonly PlayerChannels _playerChannels;
        private readonly RoomChannels _roomChannels;
        private readonly TextChat _text;
        private readonly List<VoicePlayerState> _players;
        private readonly ReadOnlyCollection<VoicePlayerState> _playersReadOnly;

        private ICommsNetwork _net;
        private IVoiceEncoder _encoder;
        private MicrophoneCapture _mic;
        private EncoderPipeline _transmissionPipeline;
        private string _localPlayerName;

        internal MicrophoneCapture MicCapture
        {
            get { return _mic; }
        }

        [SerializeField]private bool _isMuted;
        /// <summary>
        /// Get or set if the local player is muted (prevented from sending any voice transmissions)
        /// </summary>
        public bool IsMuted
        {
            get { return _isMuted; }
            set { _isMuted = value; }
        }

        [SerializeField]private VoicePlayback _playbackPrefab;
        /// <summary>
        /// Get or set the prefab to use for voice playback (may only be set before this component Starts)
        /// </summary>
        public VoicePlayback PlaybackPrefab
        {
            get { return _playbackPrefab; }
            set
            {
                if (_started)
                    throw Log.UserError("Cannot set playback prefab when the component has been started", "directly setting the 'PlaybackPrefab' property too late", "https://placeholder-software.co.uk/dissonance/docs/Reference/Components/Dissonance-Comms.md", "A0796DA8-A0BC-49E4-A1B3-F0AA0F51BAA0");

                _playbackPrefab = value;
            }
        }

        [SerializeField]private ChannelPriority _playerPriority = ChannelPriority.Default;
        /// <summary>
        /// The default priority to use for this player if a broadcast trigger does not specify a priority
        /// </summary>
        public ChannelPriority PlayerPriority
        {
            get { return _playerPriority; }
            set { _playerPriority = value; }
        }

        ChannelPriority IChannelPriorityProvider.DefaultChannelPriority
        {
            get { return PlayerPriority; }
            set { PlayerPriority = value; }
        }

        public event Action<string> LocalPlayerNameChanged;

        private ChannelPriority _topPrioritySpeaker = ChannelPriority.None;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local (Justification: Confuses unity serialization)
        [SerializeField]private TokenSet _tokens = new TokenSet();
        #endregion

        public DissonanceComms()
        {
            _playbackPool = new Pool<VoicePlayback>(6, CreatePlayback);
            _rooms = new Rooms();
            _playerChannels = new PlayerChannels(this);
            _roomChannels = new RoomChannels(this);
            _text = new TextChat(() => _net);
            _players = new List<VoicePlayerState>();
            _playersReadOnly = new ReadOnlyCollection<VoicePlayerState>(_players);

            _rooms.JoinedRoom += name => Log.Debug("Joined chat room '{0}'", name);
            _rooms.LeftRoom += name => Log.Debug("Left chat room '{0}'", name);

            _playerChannels.OpenedChannel += (id, _) => {
                Log.Debug("Opened channel to player '{0}'", id);
            };

            _playerChannels.ClosedChannel += (id, _) => {
                Log.Debug("Closed channel to player '{0}'", id);
            };

            _roomChannels.OpenedChannel += (id, _) => {
                Log.Debug("Opened channel to room '{0}'", id);
            };

            _roomChannels.ClosedChannel += (id, _) => {
                Log.Debug("Closed channel to room '{0}'", id);
            };
        }

        #region properties
        /// <summary>
        /// Get or set the local player name (may only be set before this component starts)
        /// </summary>
        public string LocalPlayerName
        {
            get { return _localPlayerName; }
            set
            {
                if (_localPlayerName == value)
                    return;

                if (_started)
                    throw Log.UserError("Cannot set player name when the component has been started", "directly setting the 'LocalPlayerName' property too late", "https://placeholder-software.co.uk/dissonance/docs/Reference/Components/Dissonance-Comms.md", "58973EDF-42B5-4FF1-BE01-FFF28300A97E");

                _localPlayerName = value;
                OnLocalPlayerNameChanged(value);
            }
        }

        /// <summary>
        /// Get a value indicating if Dissonance has successfully connected to a voice network yet
        /// </summary>
        public bool IsNetworkInitialized
        {
            get { return _net != null; }
        }
        
        /// <summary>
        /// Get an object to control which rooms the local player is listening to
        /// </summary>
        public Rooms Rooms
        {
            get { return _rooms; }
        }

        /// <summary>
        /// Get an object to control channels to other players
        /// </summary>
        public PlayerChannels PlayerChannels
        {
            get { return _playerChannels; }
        }

        /// <summary>
        /// Get an object to control channels to rooms (transmitting)
        /// </summary>
        public RoomChannels RoomChannels
        {
            get { return _roomChannels; }
        }

        /// <summary>
        /// Get an object to send and receive text messages
        /// </summary>
        public TextChat Text
        {
            get { return _text; }
        }

        /// <summary>
        /// Get a list of states of all players in the Dissonance voice session
        /// </summary>
        public ReadOnlyCollection<VoicePlayerState> Players
        {
            get { return _playersReadOnly; }
        }

        /// <summary>
        /// Get the priority of the current highest priority speaker
        /// </summary>
        public ChannelPriority TopPrioritySpeaker
        {
            get { return _topPrioritySpeaker; }
        }

        ChannelPriority IPriorityManager.TopPriority
        {
            get { return _topPrioritySpeaker; }
        }

        /// <summary>
        /// Get the set of tokens the local player has knowledge of
        /// </summary>
        public IEnumerable<string> Tokens
        {
            get { return _tokens; }
        }

        /// <summary>
        /// Event invoked whenever a new token is added to the local set
        /// </summary>
        public event Action<string> TokenAdded
        {
            add { _tokens.TokenAdded += value; }
            remove { _tokens.TokenAdded += value; }
        }

        /// <summary>
        /// Event invoked whenever a new token is removed from the local set
        /// </summary>
        public event Action<string> TokenRemoved
        {
            add { _tokens.TokenRemoved += value; }
            remove { _tokens.TokenRemoved += value; }
        }
        #endregion

        private VoicePlayback CreatePlayback()
        {
            //The game object must be inactive when it's added to the scene (so it can be edited before it activates)
            PlaybackPrefab.gameObject.SetActive(false);

            //Create an instance (currently inactive)
            var entity = Instantiate(PlaybackPrefab.gameObject);

            //Configure (and add, if necessary) audio source
            var audioSource = entity.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = entity.AddComponent<AudioSource>();
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.bypassReverbZones = true;
            }
            audioSource.loop = true;
            audioSource.pitch = 1;
            audioSource.clip = null;
            audioSource.playOnAwake = false;
            audioSource.ignoreListenerPause = true;
            audioSource.Stop();

            //Configure (and add, if necessary) sample player
            //Because the audio source has no clip, this filter will be "played" instead
            var player = entity.GetComponent<SamplePlaybackComponent>();
            if (player == null)
                entity.AddComponent<SamplePlaybackComponent>();

            //Configure VoicePlayback component
            var playback = entity.GetComponent<VoicePlayback>();
            playback.SetFormat(new WaveFormat(1, _encoder.SampleRate), (uint)_encoder.FrameSize);
            playback.PriorityManager = this;

            return playback;
        }

        // ReSharper disable once UnusedMember.Local (Justification: Part of Unity)
        private void Start()
        {
            //Ensure that all settings are loaded before we access them (potentially from other threads)
            VadSettings.Preload();
            DebugSettings.Preload();
            VoiceSettings.Preload();

            //Write multithreaded logs ASAP so the logging system knows which is the main thread
            Logs.WriteMultithreadedLogs();

            var net = FindNetwork();
            if (net == null)
                throw new Exception("Cannot find a voice network component. Please attach a voice network component appropriate to your network system to the DissonanceVoiceComms' entity.");

            if (PlaybackPrefab == null)
            {
                Log.Info("Loading default playback prefab");
                PlaybackPrefab = Resources.Load<GameObject>("PlaybackPrefab").GetComponent<VoicePlayback>();
            }

            net.PlayerJoined += Net_PlayerJoined;
            net.PlayerLeft += Net_PlayerLeft;
            net.VoicePacketReceived += Net_VoicePacketReceived;
            net.PlayerStartedSpeaking += Net_PlayerStartedSpeaking;
            net.PlayerStoppedSpeaking += Net_PlayerStoppedSpeaking;
            net.TextPacketReceived += _text.OnMessageReceived;
            
            if (string.IsNullOrEmpty(LocalPlayerName))
            {
                var guid = Guid.NewGuid().ToString();
                LocalPlayerName = guid;
            }

            //mark this component as started, locking the LocalPlayerName and PlaybackPrefab properties from changing
            _started = true;

            net.Initialize(LocalPlayerName, Rooms, PlayerChannels, RoomChannels, success =>
            {
                if (success)
                    Log.Info("Connected to voice comm network");
                else
                    Log.Error("Failed to connect to voice comm network!");

                if (success)
                {
                    _net = net;
                    _encoder = new OpusEncoder(VoiceSettings.Instance.Quality, VoiceSettings.Instance.FrameSize);
                    _mic = MicrophoneCapture.Start(_encoder.SampleRate, _encoder.FrameSize);

                    if (_mic != null)
                    {
                        _transmissionPipeline = new EncoderPipeline(_mic, _encoder, _net, () => _playerChannels.Count + _roomChannels.Count);

                        for (var i = 0; i < _acitvationListeners.Count; i++)
                            _mic.Subscribe(_acitvationListeners[i]);
                    }
                    else
                        Log.Warn("No microphone detected; local voice transmission will be disabled.");
                }
            });
        }

        private void Net_PlayerStoppedSpeaking(string player)
        {
            VoicePlayback speaker;
            if (_playback.TryGetValue(player, out speaker))
                speaker.StopPlayback();
        }

        private void Net_PlayerStartedSpeaking(string player)
        {
            VoicePlayback speaker;
            if (_playback.TryGetValue(player, out speaker))
                speaker.StartPlayback();
        }

        private void Net_VoicePacketReceived(VoicePacket packet)
        {
            VoicePlayback speaker;
            if (_playback.TryGetValue(packet.SenderPlayerId, out speaker))
                speaker.ReceiveAudioPacket(packet);
        }

        private void Net_PlayerLeft(string playerId)
        {
            VoicePlayback playback;
            if (_playback.TryGetValue(playerId, out playback))
            {
                _playback.Remove(playerId);

                playback.gameObject.SetActive(false);
                playback.PlayerName = null;
                _playbackPool.Put(playback);

                _players.RemoveAll(p => p.Name == playerId);
            }
        }

        private void Net_PlayerJoined(string playerId)
        {
            if (playerId == LocalPlayerName)
                return;

            var playback = _playbackPool.Get();
            playback.transform.parent = transform;
            playback.gameObject.name = "Player " + playerId + " voice comms";
            playback.PlayerName = playerId;
            playback.PositionTrackingAvailable = IsTrackingPlayerPosition(playerId);

            _playback[playerId] = playback;
            _players.Add(new VoicePlayerState(playback));

            playback.gameObject.SetActive(true);
        }

        private bool IsTrackingPlayerPosition(string playerName)
        {
            for (int i = 0; i < _trackedPlayers.Count; i++)
            {
                if (_trackedPlayers[i].PlayerId == playerName)
                    return true;
            }

            return false;
        }

        // ReSharper disable once UnusedMember.Local (Justification: Part of Unity)
        private void Update()
        {
            Logs.WriteMultithreadedLogs();

            SyncSpeakerPositions();
            SyncPlaybackPriority();

            if (_mic != null)
                _mic.Update(!IsMuted);

            if (_transmissionPipeline != null)
                _transmissionPipeline.Update();
        }

        private void SyncPlaybackPriority()
        {
            var topPriority = ChannelPriority.None;
            string topSpeaker = null;

            //The enumerator is a struct so no allocations there, yay.
            //However unity foreach loops allocate even when the enumerator is a struct, boo :(
            //Manually do the looping to avoid allocating :(
            var playbackEnumerator = _playback.Values.GetEnumerator();
            try
            {
                while (playbackEnumerator.MoveNext())
                {
                    var item = playbackEnumerator.Current;
                    if (item == null || !item.IsSpeaking)
                        continue;

                    //Accumulate a collection of all currently playing components, as well as what the top priority currently is
                    if (item.Priority > topPriority)
                    {
                        topPriority = item.Priority;
                        topSpeaker = item.PlayerName;
                    }
                }
            }
            finally
            {
                playbackEnumerator.Dispose();
            }

            if (_topPrioritySpeaker != topPriority)
            {
                _topPrioritySpeaker = topPriority;
                Log.Trace("Highest speaker priority is: {0} ({1})", topPriority, topSpeaker);
            }
        }

        // ReSharper disable once UnusedMember.Local (Justification: Part of Unity)
        private void OnDestroy()
        {
            if (_mic != null)
            {
                _mic.Dispose();
                _mic = null;
            }

            if (_encoder != null)
            {
                _encoder.Dispose();
                _encoder = null;
            }
        }

        private ICommsNetwork FindNetwork()
        {
            return gameObject.GetComponent<ICommsNetwork>();
        }

        private void SyncSpeakerPositions()
        {
            foreach (var player in _trackedPlayers)
            {
                if (player.PlayerId == null)
                    continue;

                VoicePlayback playback;
                if (_playback.TryGetValue(player.PlayerId, out playback))
                {
                    playback.transform.position = player.Position;
                    playback.transform.rotation = player.Rotation;
                }
            }
        }
        
        /// <summary>
        ///     Subscribes to automatic voice detection.
        /// </summary>
        /// <param name="listener">
        ///     The listener which is to receive notification when the player starts and stops speaking via
        ///     automatic voice detection.
        /// </param>
        public void SubcribeToVoiceActivation(IVoiceActivationListener listener)
        {
            if (listener == null)
                throw new ArgumentNullException("listener", "Cannot subscribe with a null listener");

            _acitvationListeners.Add(listener);

            if (_mic != null)
                _mic.Subscribe(listener);
        }

        /// <summary>
        ///     Unsubsribes from automatic voice detection.
        /// </summary>
        /// <param name="listener"></param>
        public void UnsubscribeFromVoiceActivation(IVoiceActivationListener listener)
        {
            if (listener == null)
                throw new ArgumentNullException("listener", "Cannot unsubscribe with a null listener");

            _acitvationListeners.Remove(listener);

            if (_mic != null)
                _mic.Unsubscribe(listener);
        }

        /// <summary>
        /// Enable position tracking for the player represented by the given object
        /// </summary>
        /// <param name="player"></param>
        public void TrackPlayerPosition(IDissonancePlayer player)
        {
            if (player == null)
                throw new ArgumentNullException("player", "Cannot track a null player");

            _trackedPlayers.Add(player);

            VoicePlayback playback;
            if (_playback.TryGetValue(player.PlayerId, out playback))
                playback.PositionTrackingAvailable = true;
        }

        /// <summary>
        /// Stop position tracking for the player represented by the given object
        /// </summary>
        /// <param name="player"></param>
        public void StopTracking(IDissonancePlayer player)
        {
            if (player == null)
                throw new ArgumentNullException("player", "Cannot stop tracking a null player");

            if (_trackedPlayers.Remove(player))
            {
                VoicePlayback playback;
                if (_playback.TryGetValue(player.PlayerId, out playback))
                {
                    playback.PositionTrackingAvailable = false;
                    playback.transform.position = transform.position;
                    playback.transform.rotation = transform.rotation;
                }
            }
        }

        protected virtual void OnLocalPlayerNameChanged(string obj)
        {
            var handler = LocalPlayerNameChanged;
            if (handler != null) handler(obj);
        }

        #region tokens
        /// <summary>
        /// Add the given token to the local player
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool AddToken(string token)
        {
            return _tokens.AddToken(token);
        }

        /// <summary>
        /// Removed the given token from the local player
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool RemoveToken(string token)
        {
            return _tokens.RemoveToken(token);
        }

        /// <summary>
        /// Test if the local player knows the given token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool ContainsToken(string token)
        {
            return _tokens.ContainsToken(token);
        }

        /// <summary>
        /// Tests if the local player knows has knowledge of *any* of the tokens in the given set
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public bool HasAnyToken(TokenSet tokens)
        {
            return _tokens.IntersectsWith(tokens);
        }
        #endregion
    }
}
