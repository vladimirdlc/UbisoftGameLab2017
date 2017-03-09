using System;
using System.Diagnostics;
using Dissonance.VAD;
using UnityEngine;

namespace Dissonance
{
    /// <summary>
    ///     Opens and closes voice comm channels to a room or specific player in response to events
    ///     such as voice activation, push to talk, or local player proximity.
    /// </summary>
    public class VoiceBroadcastTrigger
        : BaseCommsTrigger, IVoiceActivationListener
    {
        #region field and properties
        private readonly Stopwatch _activeHoldTimer = new Stopwatch();
        private PlayerChannel? _playerChannel;
        private RoomChannel? _roomChannel;

        private bool _isVadSpeaking;
        private bool _scriptDeactivated;
        private CommActivationMode? _previousMode;
        private IDissonancePlayer _self;

        private bool _activated;

        [SerializeField]private bool _broadcastPosition = true;
        /// <summary>
        /// Get or set if voice sent with this broadcast trigger should use positional playback
        /// </summary>
        public bool BroadcastPosition
        {
            get { return _broadcastPosition; }
            set
            {
                if (_broadcastPosition != value)
                {
                    _broadcastPosition = value;

                    if (_playerChannel.HasValue)
                    {
                        var channel = _playerChannel.Value;
                        channel.Positional = value;
                    }

                    if (_roomChannel.HasValue)
                    {
                        var channel = _roomChannel.Value;
                        channel.Positional = value;
                    }
                }
            }
        }

        [SerializeField]private CommTriggerTarget _channelType;
        /// <summary>
        /// Get or set the target type of voice sent with this trigger
        /// </summary>
        public CommTriggerTarget ChannelType
        {
            get { return _channelType; }
            set
            {
                if (_channelType != value)
                {
                    _channelType = value;

                    //Close the channel because it's type has been changed. Next update will automatically open the channel if necessary.
                    CloseChannel();
                }
            }
        }

        [SerializeField]private string _inputName;
        /// <summary>
        /// Get or set the input axis name (only applicable if this trigger is using Push-To-Talk)
        /// </summary>
        public string InputName
        {
            get { return _inputName; }
            set { _inputName = value; }
        }

        [SerializeField]private CommActivationMode _mode = CommActivationMode.VoiceActivation;
        /// <summary>
        /// Get or set how the player indicates speaking intent to this trigger
        /// </summary>
        public CommActivationMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        [SerializeField]private string _playerId;
        /// <summary>
        /// Get or set the target player ID of this trigger (only applicable if the channel type is 'player')
        /// </summary>
        public string PlayerId
        {
            get { return _playerId; }
            set
            {
                if (_playerId != value)
                {
                    _playerId = value;

                    //Since the player ID has changed we need to close the channel. Next update will open it if necessary
                    if (_channelType == CommTriggerTarget.Player)
                        CloseChannel();
                }
            }
        }

        [SerializeField]private bool _useTrigger;
        /// <summary>
        /// Get or set if this broadcast trigger should use a unity trigger volume
        /// </summary>
        public bool UseTrigger
        {
            get { return _useTrigger; }
            set { _useTrigger = value; }
        }

        [SerializeField]private string _roomName;
        /// <summary>
        /// Get or set the target room of this trigger (only applicable if the channel type is 'room')
        /// </summary>
        public string RoomName
        {
            get { return _roomName; }
            set
            {
                if (_roomName != value)
                {
                    _roomName = value;

                    //Since the room has changed we need to close the channel. Next update will open it if necessary
                    if (_channelType == CommTriggerTarget.Room)
                        CloseChannel();
                }
            }
        }

        [SerializeField]private ChannelPriority _priority = ChannelPriority.None;
        /// <summary>
        /// Get or set the priority of voice sent with this trigger
        /// </summary>
        public ChannelPriority Priority
        {
            get { return _priority; }
            set
            {
                if (_priority != value)
                {
                    _priority = value;

                    if (_playerChannel.HasValue)
                    {
                        var channel = _playerChannel.Value;
                        channel.Priority = value;
                    }

                    if (_roomChannel.HasValue)
                    {
                        var channel = _roomChannel.Value;
                        channel.Priority = value;
                    }
                }
            }
        }

        private TimeSpan _holdTime = TimeSpan.FromMilliseconds(300);
        /// <summary>
        /// Get or set how long after the player intent ends should voice continue to be transmitted
        /// </summary>
        public TimeSpan HoldTime
        {
            get { return _holdTime; }
            set { _holdTime = value; }
        }

        /// <summary>
        /// Get if this voice broadcast trigger is currently transmitting voice
        /// </summary>
        public bool IsTransmitting
        {
            get { return _playerChannel != null || _roomChannel != null; }
        }

        protected override bool IsColliderTriggerable
        {
            get { return UseTrigger; }
        }
        #endregion

        protected override void Start()
        {
            base.Start();

            _self = GetComponent<IDissonancePlayer>();
        }

        protected override void Update()
        {
            base.Update();

            if (!CheckVoiceComm())
                return;

            if (_previousMode != Mode)
                SwitchMode();

            if (ShouldActivate())
            {
                if (!IsTransmitting)
                    OpenChannel();
            }
            else
            {
                if (IsTransmitting)
                    CloseChannel();
            }
        }

        public void OnDisable()
        {
            CloseChannel();
        }

        public void OnDestroy()
        {
            CloseChannel();
            if (Comms != null)
                Comms.UnsubscribeFromVoiceActivation(this);
        }

        #region manual activation
        /// <summary>
        /// Allow this broadcast trigger to transmit voice
        /// </summary>
        public void StartSpeaking()
        {
            _scriptDeactivated = false;
        }

        /// <summary>
        /// Prevent this broadcast trigger from speaking until StartSpeaking is called
        /// </summary>
        public void StopSpeaking()
        {
            _scriptDeactivated = true;
        }
        #endregion

        private void SwitchMode()
        {
            if (!CheckVoiceComm())
                return;

            CloseChannel();
            _scriptDeactivated = false;

            if (_previousMode == CommActivationMode.VoiceActivation && Mode != CommActivationMode.VoiceActivation)
            {
                Comms.UnsubscribeFromVoiceActivation(this);
                _isVadSpeaking = false;
            }

            if (Mode == CommActivationMode.VoiceActivation)
                Comms.SubcribeToVoiceActivation(this);

            _previousMode = Mode;
        }

        private bool ShouldActivate()
        {
            //Check some situations where activating is impossible...
            // - Cannot broadcast to yourself (by sibling component)!
            if (_channelType == CommTriggerTarget.Self && _self != null && _self.Type == NetworkPlayerType.Local)
                return false;

            // - Cannot broadcast to yourself (by name)
            if (_channelType == CommTriggerTarget.Player && Comms.LocalPlayerName == _playerId)
                return false;

            //Test the actual activation systems
            bool activate;
            switch (Mode)
            {
                case CommActivationMode.VoiceActivation:
                    activate = _isVadSpeaking;
                    break;

                case CommActivationMode.PushToTalk:
                    activate = Input.GetAxis(InputName) > 0.5f;
                    break;

                case CommActivationMode.None:
                    activate = false;
                    break;

                default:
                    Log.Error("Unknown Activation Mode '{0}'", Mode);
                    activate = false;
                    break;
            }

            //Only speak if this has been activated by scripts (defaults to true)
            activate &= !_scriptDeactivated;

            //Only activate if the local player has the correct tokens (and the set of required tokens for this trigger is not empty)
            activate &= TokenActivationState;

            //After the modes above return false we want to stay active for the holdTime
            if (!activate)
            {
                if (_activated)
                {
                    _activeHoldTimer.Start();
                    if (_activeHoldTimer.Elapsed > _holdTime)
                    {
                        _activeHoldTimer.Stop();
                        _activeHoldTimer.Reset();
                    }
                    else
                    {
                        activate = true;
                    }
                }
            }
            _activated = activate;

            return activate && (!UseTrigger || IsColliderTriggered);
        }

        #region channel management
        private void OpenChannel()
        {
            if (!CheckVoiceComm())
                return;

            if (ChannelType == CommTriggerTarget.Room)
            {
                _roomChannel = Comms.RoomChannels.Open(RoomName, _broadcastPosition, _priority);
            }
            else
            {
                string target;
                if (ChannelType == CommTriggerTarget.Player)
                    target = PlayerId;
                else
                    target = _self.PlayerId;

                _playerChannel = Comms.PlayerChannels.Open(target, _broadcastPosition, _priority);
            }
        }

        private void CloseChannel()
        {
            if (_roomChannel != null)
            {
                _roomChannel.Value.Dispose();
                _roomChannel = null;
            }

            if (_playerChannel != null)
            {
                _playerChannel.Value.Dispose();
                _playerChannel = null;
            }
        }
        #endregion

        #region IVoiceActivationListener impl
        void IVoiceActivationListener.VoiceActivationStart()
        {
            _isVadSpeaking = true;
        }

        void IVoiceActivationListener.VoiceActivationStop()
        {
            _isVadSpeaking = false;
        }
        #endregion
    }
}
