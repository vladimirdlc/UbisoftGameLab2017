using System;
using System.Threading;

namespace Dissonance.Networking.Client
{
    internal interface IConnectionNegotiator
    {
        ConnectionNegotiator.ConnectionState State { get; }
    }

    internal class ConnectionNegotiator
        : IConnectionNegotiator
    {

        #region helper types
        public enum ConnectionState
        {
            None,
            Negotiating,
            Connected,
            Disconnected
        }
        #endregion

        #region fields and properties
        private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(ConnectionNegotiator).Name);
        private static readonly TimeSpan HandshakeRequestInterval = TimeSpan.FromSeconds(2);

        private readonly IClient _client;

        private DateTime _lastHandshakeRequest;

        private bool _running;

        private int _connectionStateValue = (int)ConnectionState.None;
        public ConnectionState State
        {
            get { return (ConnectionState)_connectionStateValue; }
        }
        #endregion

        #region constructor
        public ConnectionNegotiator(IClient client)
        {
            _client = client;
        }
        #endregion

        public void Start()
        {
            if (State == ConnectionState.Disconnected)
                Log.PossibleBug("Attempted to restart a ConnectionNegotiator after it has been disconnected", "92F0B2EB-282A-4558-B3BD-6656F83A06E3");

            _running = true;
        }

        public void Stop()
        {
            _running = false;
            _connectionStateValue = (int)ConnectionState.Disconnected;

            _client.OnDisconnected();
        }

        public void Update()
        {
            if (!_running)
                return;

            //send a new connection handshake periodically while we're not connected
            // - these accesses of _connectionState are not protected by the _connectionStateLock. This is safe since C# guarantees no tearing on 32 bit reads (and this enum is 32 bit)
            var shouldResendHandshake = State == ConnectionState.Negotiating && DateTime.Now - _lastHandshakeRequest > HandshakeRequestInterval;

            if (State == ConnectionState.None || shouldResendHandshake)
                BeginConnectionNegotiation();
        }

        /// <summary>
        /// Begin negotiating a connection with the server by sending a handshake.
        /// </summary>
        /// <remarks>It is safe to call this several times, even once negotiation has finished</remarks>
        private void BeginConnectionNegotiation()
        {
            //Sanity check. We can't do *anything* with a disconnected client, definitely not restart negotiation!
            if (State == ConnectionState.Disconnected)
                throw Log.PossibleBug("Attempted to begin connection negotiation with a client which is disconnected", "39533F23-2DAC-4340-9A7D-960904464E23");

            _lastHandshakeRequest = DateTime.Now;

            //Send the handshake request to the server (when the server replies with a response, we know we're connected)
            _client.SendReliable(
                new PacketWriter(new ArraySegment<byte>(_client.ByteBufferPool.Get()))
                .WriteHandshakeRequest()
                .Written
            );

            //Set the state to negotiating only if the state was previously none
            Interlocked.CompareExchange(ref _connectionStateValue, (int)ConnectionState.Negotiating, (int)ConnectionState.None);
        }

        public void ReceiveHandshakeResponse(ref PacketReader reader)
        {
            var session = reader.ReadHandshakeResponse();

            //We could receive an unbounded number of handshake responses. We only want to run this event on the *first* one (when we transition from Negotiating to Connected
            //Additionally it's possible the connection is not in the negotiating state (could already be disconnected). So check that it's the right value before exchanging.
            if (Interlocked.CompareExchange(ref _connectionStateValue, (int)ConnectionState.Connected, (int)ConnectionState.Negotiating) == (int)ConnectionState.Negotiating)
                Log.Info("Received handshake response from server, joined session '{0}'", session);
        }
    }
}
