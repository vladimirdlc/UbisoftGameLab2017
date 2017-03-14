using System.Collections.Generic;
using UnityEngine;

namespace Dissonance
{
    /// <summary>
    /// Base class for dissonance room triggers
    /// </summary>
    public abstract class BaseCommsTrigger
        : MonoBehaviour, IAccessTokenCollection
    {
        #region fields and properties

        protected readonly Log Log;

        protected abstract bool IsColliderTriggerable { get; }

        protected bool IsColliderTriggered
        {
            get { return IsColliderTriggerable && _entitiesInCollider.Count > 0; }
        }
        private readonly List<GameObject> _entitiesInCollider = new List<GameObject>(8);

        // ReSharper disable once FieldCanBeMadeReadOnly.Local (Justification: Confuses unity serialization)
        [SerializeField]
        private TokenSet _tokens = new TokenSet();
        private bool? _cachedTokenActivation = null;

        /// <summary>
        /// Get the set of tokens this trigger requires (trigger will only function if the local player knows at least one of the tokens)
        /// </summary>
        public IEnumerable<string> Tokens { get { return _tokens; } }

        private DissonanceComms _comms;
        protected DissonanceComms Comms
        {
            get { return _comms; }
            private set
            {
                if (_comms != null)
                {
                    _comms.TokenAdded -= TokensModified;
                    _comms.TokenRemoved -= TokensModified;
                }

                _comms = value;

                if (_comms != null)
                {
                    _comms.TokenAdded += TokensModified;
                    _comms.TokenRemoved += TokensModified;
                }
            }
        }
        #endregion

        protected BaseCommsTrigger()
        {
            Log = Logs.Create(LogCategory.Core, GetType().Name);
        }

        // ReSharper disable once UnusedMemberHierarchy.Global (Justification: Used implicitly by unity)
        protected virtual void Start()
        {
            if (Comms == null)
                Comms = FindLocalVoiceComm();

            _tokens.TokenAdded += TokensModified;
            _tokens.TokenRemoved += TokensModified;
        }

        // ReSharper disable once UnusedMemberHierarchy.Global (Justification: Used implicitly by unity)
        protected virtual void Update()
        {
            if (!CheckVoiceComm())
                return;

            //Remove items which triggered the collider trigger but died before leaving the collider
            for (var i = _entitiesInCollider.Count - 1; i >= 0; i--)
            {
                var thing = _entitiesInCollider[i];
                if (!thing.gameObject.activeInHierarchy || (_entitiesInCollider == null))
                    _entitiesInCollider.RemoveAt(i);
            }
        }

        #region tokens
        private void TokensModified(string token)
        {
            _cachedTokenActivation = null;
        }

        protected bool TokenActivationState
        {
            get
            {
                if (!_cachedTokenActivation.HasValue)
                {
                    _cachedTokenActivation = _tokens.Count == 0 || Comms.HasAnyToken(_tokens);
                    Log.Info("Recalculating token activation: {0} tokens, activated: {1}", _tokens.Count, _cachedTokenActivation.Value);
                }

                return _cachedTokenActivation.Value;
            }
        }

        /// <summary>
        /// Test if this trigger will work with the given token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool ContainsToken(string token)
        {
            return _tokens.ContainsToken(token);
        }

        /// <summary>
        /// Add a token to the set for this trigger
        /// </summary>
        /// <param name="token"></param>
        /// <returns>A value indicating if the given token was added (false if the set already contained the token)</returns>
        public bool AddToken(string token)
        {
            return _tokens.AddToken(token);
        }

        /// <summary>
        /// Remove a token from the set for this trigger
        /// </summary>
        /// <param name="token"></param>
        /// <returns>A value indicating if the given token was removed (false if the set never contained the token in the first place)</returns>
        public bool RemoveToken(string token)
        {
            return _tokens.RemoveToken(token);
        }
        #endregion

        #region collider trigger
        // ReSharper disable once UnusedMember.Local (used implicitly by unity)
        // ReSharper disable once SuggestBaseTypeForParameter (Implicitly required by unity)
        private void OnTriggerEnter(Collider other)
        {
            if (!enabled)
                return;
            if (!CheckVoiceComm())
                return;
            if (!IsColliderTriggerable)
                return;

            if (ColliderTriggerFilter(other))
            {
                Log.Debug("Collider entered ({0})", _entitiesInCollider.Count);
                _entitiesInCollider.Add(other.gameObject);
            }
        }

        // ReSharper disable once UnusedMember.Local (used implicitly by unity)
        // ReSharper disable once SuggestBaseTypeForParameter (Implicitly required by unity)
        private void OnTriggerExit(Collider other)
        {
            if (!enabled)
                return;
            if (!CheckVoiceComm())
                return;
            if (!IsColliderTriggerable)
                return;

            if (ColliderTriggerFilter(other))
            {
                Log.Debug("Collider exited ({0})", _entitiesInCollider.Count);
                _entitiesInCollider.Remove(other.gameObject);
            }
        }

        /// <summary>
        /// When something affects the trigger (enter or exit) it will only affect the trigger state of this component if this filter returns true.
        /// May be overriden to filter which entities should trigger. Default behaviour returns true if the entity is the local dissonance player, otherwise false
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if this entity should affect the trigger, otherwise false</returns>
        protected virtual bool ColliderTriggerFilter(Collider other)
        {
            var player = other.GetComponent<IDissonancePlayer>();
            return player != null && player.Type == NetworkPlayerType.Local;
        }
        #endregion

        private DissonanceComms FindLocalVoiceComm()
        {
            var comm = GetComponent<DissonanceComms>();
            if (comm == null)
                comm = FindObjectOfType<DissonanceComms>();

            return comm;
        }

        protected bool CheckVoiceComm()
        {
            //This is ugly, but correct. Comms == null is *not* correct!
            //Unity returns true for a null check if an object is merely disposed.
            //In some cases (disable and destroy) Comms may reasonably be disposed but not null!
            var missing = ReferenceEquals(Comms, null);

            //If we didn't find it, try to find it before sending a warning
            if (missing)
            {
                Comms = FindLocalVoiceComm();
                missing = ReferenceEquals(Comms, null);
            }

            if (missing)
                Log.Error("Cannot find DissonanceComms component for local player in scene! To fix this add a DissonanceComms component to your scene.");

            return !missing;
        }
    }
}
