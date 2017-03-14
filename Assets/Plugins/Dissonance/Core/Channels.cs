using System;
using System.Collections.Generic;
using Dissonance.Audio.Capture;
using Dissonance.Datastructures;

namespace Dissonance
{
    /// <summary>
    /// Base class for a collection of channels
    /// </summary>
    /// <typeparam name="T">Type of the channel</typeparam>
    /// <typeparam name="TId">Type of the unique ID which identifies this channel</typeparam>
    public abstract class Channels<T, TId>
        where T : IChannel<TId>
    {
        private readonly Log _log;

        private readonly Dictionary<ushort, T> _openChannelsBySubId;
        private readonly Pool<ChannelProperties> _propertiesPool;

        private ushort _nextId;

        public event Action<TId, ChannelProperties> OpenedChannel;
        public event Action<TId, ChannelProperties> ClosedChannel;

        internal Channels(IChannelPriorityProvider priorityProvider)
        {
            _log = Logs.Create((LogCategory.Core), GetType().Name);

            _openChannelsBySubId = new Dictionary<ushort, T>();
            _propertiesPool = new Pool<ChannelProperties>(64, () => new ChannelProperties(priorityProvider));
        }

        protected abstract T CreateChannel(ushort subscriptionId, TId channelId, ChannelProperties properties);

        /// <summary>
        /// Number of currently open channels
        /// </summary>
        public int Count
        {
            get { return _openChannelsBySubId.Count; }
        }

        /// <summary>
        /// Check if the given item is currently in this collection (i.e. is the channel open)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            return _openChannelsBySubId.ContainsKey(item.SubscriptionId);
        }

        /// <summary>
        /// Open a new channel
        /// </summary>
        /// <param name="id"></param>
        /// <param name="positional"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public T Open(TId id, bool positional = false, ChannelPriority priority = ChannelPriority.Default)
        {
            if (id == null)
                throw new ArgumentNullException("id", "Cannot open a channel with a null ID");

            //Sanity check to ensure we don't enter an infinite loop
            if (_openChannelsBySubId.Count >= ushort.MaxValue)
                throw _log.PossibleBug("Attempted to open 65535 channels", "7564ECCA-73C2-4720-B4C0-B873E63216AD");

            //Generate a new ID for this channel.
            ushort subId;
            do
            {
                subId = unchecked(_nextId++);
            } while (_openChannelsBySubId.ContainsKey(subId));

            var properties = _propertiesPool.Get();
            properties.Id = subId;
            properties.Positional = positional;
            properties.Priority = priority;

            var channel = CreateChannel(subId, id, properties);

            _openChannelsBySubId.Add(channel.SubscriptionId, channel);

            var handler = OpenedChannel;
            if (handler != null) handler(channel.TargetId, channel.Properties);

            return channel;
        }

        public bool Close(T channel)
        {
            if (channel == null)
                throw new ArgumentNullException("channel", "Cannot close a null channel");

            var removed = _openChannelsBySubId.Remove(channel.SubscriptionId);
            if (removed)
            {
                channel.Properties.Id = 0;
                _propertiesPool.Put(channel.Properties);

                var handler = ClosedChannel;
                if (handler != null) handler(channel.TargetId, channel.Properties);
            }

            return removed;
        }

        public Dictionary<ushort, T>.Enumerator GetEnumerator()
        {
            return _openChannelsBySubId.GetEnumerator();
        }
    }

    /// <summary>
    /// A collection of channels to players
    /// </summary>
    public sealed class PlayerChannels
        : Channels<PlayerChannel, string>
    {
        internal PlayerChannels(IChannelPriorityProvider priorityProvider)
            : base(priorityProvider)
        {
            
        }

        protected override PlayerChannel CreateChannel(ushort subscriptionId, string channelId, ChannelProperties properties)
        {
            return new PlayerChannel(subscriptionId, channelId, this, properties);
        }
    }

    /// <summary>
    /// A collection of channels to rooms
    /// </summary>
    public sealed class RoomChannels
        : Channels<RoomChannel, string>
    {
        internal RoomChannels(IChannelPriorityProvider priorityProvider)
            : base(priorityProvider)
        {

        }

        protected override RoomChannel CreateChannel(ushort subscriptionId, string channelId, ChannelProperties properties)
        {
            return new RoomChannel(subscriptionId, channelId, this, properties);
        }
    }

    public sealed class ChannelProperties
    {
        private readonly IChannelPriorityProvider _defaultPriority;

        public ushort Id { get; internal set; }

        public bool Positional { get; set; }

        public ChannelPriority Priority { get; set; }

        /// <summary>
        /// This calculates what priority to actually use to transmit with. If Priority is set to None then this will fall back to some other default value
        /// </summary>
        internal ChannelPriority TransmitPriority
        {
            get
            {
                if (Priority == ChannelPriority.None)
                    return _defaultPriority.DefaultChannelPriority;
                return Priority;
            }
        }

        internal ChannelProperties(IChannelPriorityProvider defaultPriority)
        {
            _defaultPriority = defaultPriority;
        }
    }
    
    public enum ChannelPriority
    {
        /// <summary>
        /// No priority assigned
        /// </summary>
        None = -2,

        /// <summary>
        /// Low priority (will be muted by voices at the default priority)
        /// </summary>
        Low = -1,

        /// <summary>
        /// Default priority (will be muted by medium and high priority)
        /// </summary>
        Default = 0,

        /// <summary>
        /// Medium priority (will be muted by high priority)
        /// </summary>
        Medium = 1,

        /// <summary>
        /// High priority (will not be muted by any other voice)
        /// </summary>
        High = 2
    }

    /// <summary>
    /// Interface for channels. Channels are implemented as structs, and should never be cast into this interface! It us used to restrict generic types.
    /// </summary>
    /// <typeparam name="T">Type which represents the target of this channel</typeparam>
    // ReSharper disable once TypeParameterCanBeVariant (Justification: Unity panics if you do this)
    public interface IChannel<T> : IDisposable
    {
        /// <summary>
        /// The target of this channel
        /// </summary>
        T TargetId { get; }
        
        /// <summary>
        /// A unique ID for this channel (may be re-used by other channels, but only after this channel has been closed)
        /// </summary>
        ushort SubscriptionId { get; }

        /// <summary>
        /// The properties of this channel
        /// </summary>
        ChannelProperties Properties { get; }
    }
    
    /// <summary>
    /// A channel sending voice data to a player. Dispose this struct to close the channel.
    /// </summary>
    public struct PlayerChannel : IChannel<string>
    {
        private readonly ushort _subscriptionId;
        private readonly string _playerId;
        private readonly ChannelProperties _properties;
        private readonly PlayerChannels _channels;

        internal PlayerChannel(ushort subscriptionId, string playerId, PlayerChannels channels, ChannelProperties properties)
        {
            _subscriptionId = subscriptionId;
            _playerId = playerId;
            _channels = channels;
            _properties = properties;
        }

        /// <summary>
        /// Unique ID of this channel (may be re-used by another channel once this one is closed)
        /// </summary>
        public ushort SubscriptionId
        {
            get { return _subscriptionId; }
        }

        /// <summary>
        /// The name of the player this channel is sending voice data to
        /// </summary>
        public string TargetId
        {
            get { return _playerId; }
        }

        /// <summary>
        /// Gets the configurable properties of this channel.
        /// </summary>
        ChannelProperties IChannel<string>.Properties
        {
            get { return _properties; }
        }

        internal ChannelProperties Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Get a value indicating if this channel has been closed
        /// </summary>
        public bool IsOpen
        {
            get { return _channels.Contains(this); }
        }

        /// <summary>
        /// Gets or sets a value indicating if this channel should be played on other clients with 3D positional audio.
        /// </summary>
        public bool Positional
        {
            get
            {
                CheckValidProperties();
                return _properties.Positional;
            }
            set
            {
                CheckValidProperties();
                _properties.Positional = value;
            }
        }

        /// <summary>
        /// Gets or sets the speaker priority for this channel.
        /// </summary>
        public ChannelPriority Priority
        {
            get
            {
                CheckValidProperties();
                return _properties.Priority;
            }
            set
            {
                CheckValidProperties();
                _properties.Priority = value;
            }
        }
        
        /// <summary>
        /// Close this channel (stop sending data)
        /// </summary>
        public void Dispose()
        {
            _channels.Close(this);
        }

        private void CheckValidProperties()
        {
            if (_properties.Id != _subscriptionId)
                throw new DissonanceException("Cannot access channel properties on a closed channel.");
        }
    }

    /// <summary>
    /// A channel sending voice data to a room. Dispose this struct to close the channel.
    /// </summary>
    public struct RoomChannel : IChannel<string>
    {
        private static readonly Log Log = Logs.Create(LogCategory.Core, typeof(RoomChannel).Name);

        private readonly ushort _subscriptionId;
        private readonly string _roomId;
        private readonly ChannelProperties _properties;
        private readonly RoomChannels _channels;

        internal RoomChannel(ushort subscriptionId, string roomId, RoomChannels channels, ChannelProperties properties)
        {
            _subscriptionId = subscriptionId;
            _roomId = roomId;
            _channels = channels;
            _properties = properties;
        }

        /// <summary>
        /// Unique ID of this channel (may be re-used by another channel once this one is closed)
        /// </summary>
        public ushort SubscriptionId
        {
            get { return _subscriptionId; }
        }

        /// <summary>
        /// The name of the room this channel is sending voice data to
        /// </summary>
        public string TargetId
        {
            get { return _roomId; }
        }

        /// <summary>
        /// Gets the configurable properties of this channel.
        /// </summary>
        ChannelProperties IChannel<string>.Properties
        {
            get { return _properties; }
        }

        internal ChannelProperties Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Get a value indicating if this channel has been closed
        /// </summary>
        public bool IsOpen
        {
            get { return _channels.Contains(this); }
        }

        /// <summary>
        /// Gets or sets a value indicating if this channel should be played on other clients with 3D positional audio.
        /// </summary>
        public bool Positional
        {
            get
            {
                CheckValidProperties();
                return _properties.Positional;
            }
            set
            {
                CheckValidProperties();
                _properties.Positional = value;
            }
        }

        /// <summary>
        /// Gets or sets the speaker priority for this channel.
        /// </summary>
        public ChannelPriority Priority
        {
            get
            {
                CheckValidProperties();
                return _properties.Priority;
            }
            set
            {
                CheckValidProperties();
                _properties.Priority = value;
            }
        }

        /// <summary>
        /// Close this channel (stop sending data)
        /// </summary>
        public void Dispose()
        {
            _channels.Close(this);
        }

        private void CheckValidProperties()
        {
            if (_properties.Id != _subscriptionId)
                throw Log.UserError("Attempted to access a disposed channel", "Attempting to get or set channel properties after calling Dispose() on a channel", "https://placeholder-software.co.uk/dissonance/docs/Tutorials/Directly-Using-Channels", "DE77DE73-8DBF-4802-A413-B9A5D77A5189");
        }
    }
}
