using System.Collections.Generic;
using UnityEngine;

namespace Dissonance
{
    /// <summary>
    ///     Enters and exits voice comm rooms in response to entity activation or
    ///     local player proximity.
    /// </summary>
    public class VoiceReceiptTrigger : BaseCommsTrigger
    {
        #region fields and properties
        private RoomMembership? _membership;

        [SerializeField]private string _roomName;
        /// <summary>
        /// Get or set the name of the room this receipt trigger is receives from
        /// </summary>
        public string RoomName
        {
            get { return _roomName; }
            set { _roomName = value; }
        }

        private bool _scriptDeactivated;

        [SerializeField]private bool _useTrigger;
        /// <summary>
        /// Get or set if this receipt trigger should use a unity trigger volume
        /// </summary>
        public bool UseTrigger
        {
            get { return _useTrigger; }
            set { _useTrigger = value; }
        }

        protected override bool IsColliderTriggerable
        {
            get { return _useTrigger; }
        }
        #endregion

        #region manual activation
        /// <summary>
        /// Allow this receipt trigger to receive voice
        /// </summary>
        public void StartListening()
        {
            _scriptDeactivated = false;
        }

        /// <summary>
        /// Prevent this receipt trigger from receiving any voice until StartListening is called
        /// </summary>
        public void StopListening()
        {
            _scriptDeactivated = true;
        }
        #endregion

        protected override void Update()
        {
            base.Update();

            if (!CheckVoiceComm())
                return;

            var shouldActivate =
                _roomName != null                           //Only activate if it's possible to activate
                && !_scriptDeactivated                      //Only activate if not explicitly deactivated
                && (!_useTrigger || IsColliderTriggered)    //Only activate if trigger is activated (and we're using trigger activation)
                && TokenActivationState;                    //Only activate if tokens say so

            if (shouldActivate)
            {
                if (!_membership.HasValue)
                    _membership = Comms.Rooms.Join(RoomName);
            }
            else
            {
                if (_membership.HasValue)
                {
                    Comms.Rooms.Leave(_membership.Value);
                    _membership = null;
                }
            }
        }

        // ReSharper disable once UnusedMember.Local (Justification: Used implicitly by unity)
        private void OnDisable()
        {
            if (Comms != null && _membership != null)
            {
                Comms.Rooms.Leave(_membership.Value);
                _membership = null;
            }
        }
    }
}