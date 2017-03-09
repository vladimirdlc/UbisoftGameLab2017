namespace Dissonance.Networking
{
    internal struct ReceivingChannelStats
    {
        public ushort PlayerId;
        public ushort BaseSequenceNumber;
        public long LastReceiptTicks;
        public uint LocalSequenceNumber;
        public bool Open;
    }
}
