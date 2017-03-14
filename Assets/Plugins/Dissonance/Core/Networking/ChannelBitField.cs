namespace Dissonance.Networking
{
    internal struct ChannelBitField
    {
        private const byte TypeMask = 0x01;         //00000001
        private const byte PositionalMask = 0x02;   //00000010
        private const byte ClosureMask = 0x04;      //00000100
        private const byte PriorityMask = 0x18;     //00011000

        private readonly byte _bitfield;
        public byte Bitfield
        {
            get { return _bitfield; }
        }

        public ChannelType Type
        {
            get
            {
                if ((_bitfield & TypeMask) == TypeMask)
                    return ChannelType.Room;
                return ChannelType.Player;
            }
        }

        public bool IsClosing
        {
            get { return (_bitfield & ClosureMask) == ClosureMask; }
        }

        public bool IsPositional
        {
            get { return (_bitfield & PositionalMask) == PositionalMask; }
        }

        public ChannelPriority Priority
        {
            get
            {
                var val = (_bitfield & PriorityMask) >> 3;
                switch (val)
                {
                    default: return ChannelPriority.Default;
                    case 1: return ChannelPriority.Low;
                    case 2: return ChannelPriority.Medium;
                    case 3: return ChannelPriority.High;
                }
            }
        }

        public ChannelBitField(byte bitfield)
        {
            _bitfield = bitfield;
        }

        public ChannelBitField(ChannelType type, ChannelPriority priority, bool positional, bool closing) : this()
        {
            _bitfield = 0;

            if (type == ChannelType.Room)
                _bitfield |= TypeMask;
            if (positional)
                _bitfield |= PositionalMask;
            if (closing)
                _bitfield |= ClosureMask;

            switch (priority)
            {
                case ChannelPriority.Low:
                    _bitfield |= 1 << 3;
                    break;
                case ChannelPriority.Medium:
                    _bitfield |= 2 << 3;
                    break;
                case ChannelPriority.High:
                    _bitfield |= 3 << 3;
                    break;

                // ReSharper disable RedundantCaseLabel, RedundantEmptyDefaultSwitchBranch (justification: I like to be explicit about these things)
                case ChannelPriority.None:
                case ChannelPriority.Default:
                default:
                    break;
                // ReSharper restore RedundantCaseLabel, RedundantEmptyDefaultSwitchBranch
            }
        }
    }
}
