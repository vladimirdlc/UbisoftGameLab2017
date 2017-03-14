using System;
using System.Collections.Generic;
using System.Threading;
using Dissonance.Config;

namespace Dissonance.Networking.Client
{
    /// <summary>
    /// Receives packet and sends them to the correct place for processing.
    /// </summary>
    /// <remarks>Primary functionality is artifically delaying packets when DebugSettings.Instance.EnableNetworkSimulation is true</remarks>
    internal class PacketRouter
    {
        #region helper types
        private struct DelayedPacket
        {
            public ArraySegment<byte> Data;
            public DateTime SimulatedReceiptTime;
        }
        #endregion

        #region fields and properties
        private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(PacketRouter).Name);

        private readonly Random _rnd = new Random();
        private int _delayedCount;
        private readonly List<DelayedPacket> _processingPackets = new List<DelayedPacket>();
        private readonly List<DelayedPacket> _delayedUnorderedPackets = new List<DelayedPacket>();
        private readonly Queue<DelayedPacket> _delayedOrderedPackets = new Queue<DelayedPacket>();

        private readonly IPacketProcessor _processor;
        #endregion

        #region constructor
        public PacketRouter(IPacketProcessor processor)
        {
            _processor = processor;
        }
        #endregion

        public void NetworkReceivedPacket(ArraySegment<byte> data)
        {
            if (data.Array == null)
                throw new ArgumentNullException("data");

            if (DebugSettings.Instance.EnableNetworkSimulation)
            {
                var reader = new PacketReader(data);
                reader.ReadUInt16(); // skip magic number (we don't want to error check it here, we'll let the error get caught in the normal way)
                var header = (MessageTypes)reader.ReadByte();

                //Check if we should discard this packet to fake packet loss
                if (!IsReliable(header))
                {
                    var lossRoll = _rnd.NextDouble();
                    if (lossRoll < DebugSettings.Instance.PacketLoss)
                        return;
                }

                var delay = _rnd.Next(DebugSettings.Instance.MinimumLatency, DebugSettings.Instance.MaximumLatency);
                var simulatedArrivalTime = DateTime.Now + TimeSpan.FromMilliseconds(delay);

                var dataCopy = new byte[data.Count];
                Array.Copy(data.Array, data.Offset, dataCopy, 0, data.Count);

                var packet = new DelayedPacket {
                    SimulatedReceiptTime = simulatedArrivalTime,
                    Data = new ArraySegment<byte>(dataCopy)
                };

                Interlocked.Increment(ref _delayedCount);
                if (IsOrdered(header))
                {
                    lock (_delayedOrderedPackets)
                        _delayedOrderedPackets.Enqueue(packet);
                }
                else
                {
                    lock (_delayedUnorderedPackets)
                        _delayedUnorderedPackets.Add(packet);
                }
            }
            else
                ProcessReceivedPacket(data);
        }

        private static bool IsOrdered(MessageTypes header)
        {
            return header != MessageTypes.VoiceData;
        }

        private static bool IsReliable(MessageTypes header)
        {
            return header != MessageTypes.VoiceData;
        }

        private void ProcessReceivedPacket(ArraySegment<byte> data)
        {
            var reader = new PacketReader(data);

            var magic = reader.ReadUInt16();
            if (magic != PacketWriter.Magic)
            {
                Log.Warn("Received packet with incorrect magic number. Expected {0}, got {1}", PacketWriter.Magic, magic);
                return;
            }

            var header = (MessageTypes)reader.ReadByte();
            switch (header)
            {
                case MessageTypes.ClientState:
                    Log.Error("Received a client state update (this should only ever be received by the server)");
                    break;

                case MessageTypes.PlayerRoutingUpdate:
                    _processor.ReceivePlayerRoutingUpdate(ref reader);
                    break;

                case MessageTypes.VoiceData:
                    _processor.ReceiveVoiceData(ref reader);
                    break;

                case MessageTypes.TextData:
                    _processor.ReceiveTextData(ref reader);
                    break;

                case MessageTypes.HandshakeResponse:
                    _processor.ReceiveHandshakeResponse(ref reader);
                    break;

                case MessageTypes.HandshakeRequest:
                    Log.Error("Received a handshake request (this should only ever be received by the server)");
                    break;

                default:
                    Log.Error("Ignoring a packet with an unknown header: '{0}'", header);
                    break;
            }
        }

        public void Update()
        {
            ProcessDiagnosticDelayedMessages();
        }

        private void ProcessDiagnosticDelayedMessages()
        {
            //Early exit to ensure we do no work at all when delayed packet processing has not got anything in its's buffers
            //This means in the common case where delayed packet processing is simply not active we only do this single comparison
            if (_delayedCount == 0)
                return;

            _processingPackets.Clear();
            var t = DateTime.Now;

            //Pull out all of the unordered packets which have been sufficiently delayed
            lock (_delayedUnorderedPackets)
            {
                for (var i = _delayedUnorderedPackets.Count - 1; i >= 0; i--)
                {
                    var p = _delayedUnorderedPackets[i];
                    if (p.SimulatedReceiptTime <= t)
                    {
                        _processingPackets.Add(p);
                        _delayedUnorderedPackets.RemoveAt(0);
                    }
                }
            }

            //Sort the unordered packets by their receipt time
            _processingPackets.Sort((a, b) => a.SimulatedReceiptTime.CompareTo(b.SimulatedReceiptTime));

            //Keep dequeuing ordered packets which they have been sufficiently delayed (ensuring that we preserve order)
            lock (_delayedOrderedPackets)
            {
                while (_delayedOrderedPackets.Count > 0 && _delayedOrderedPackets.Peek().SimulatedReceiptTime <= t)
                    _processingPackets.Add(_delayedOrderedPackets.Dequeue());
            }

            //Ensure the counter is kept up to date
            Interlocked.Add(ref _delayedCount, -_processingPackets.Count);

            //Now process all of the packets
            for (var i = 0; i < _processingPackets.Count; i++)
                ProcessReceivedPacket(_processingPackets[i].Data);
            _processingPackets.Clear();
        }
    }
}
