namespace Dissonance.Networking.Client
{
    internal struct OpenChannel
    {
        private readonly ChannelProperties _config;

        private readonly ChannelType _type;
        private readonly ushort _recipient;
        private readonly bool _isClosing;

        public ChannelProperties Config
        {
            get { return _config; }
        }

        public byte Bitfield
        {
            get
            {
                return new ChannelBitField(
                    _type,
                    Priority,
                    IsPositional,
                    _isClosing
                ).Bitfield;
            }
        }

        public ushort Recipient
        {
            get { return _recipient; }
        }


        public ChannelType Type
        {
            get { return _type; }
        }

        public bool IsClosing
        {
            get { return _isClosing; }
        }

        public bool IsPositional
        {
            get { return _config.Positional; }
        }

        public ChannelPriority Priority
        {
            get { return _config.TransmitPriority; }
        }

        public OpenChannel(ChannelType type, ChannelProperties config, bool closing, ushort recipient)
        {
            _type = type;
            _config = config;
            _isClosing = closing;
            _recipient = recipient;
        }

        public OpenChannel AsClosing()
        {
            return new OpenChannel(_type, _config, true, _recipient);
        }
    }
}
