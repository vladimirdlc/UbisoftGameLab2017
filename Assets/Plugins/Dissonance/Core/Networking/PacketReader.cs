using System;
using System.Text;
using Dissonance.Datastructures;
using Dissonance.Extensions;

namespace Dissonance.Networking
{
    internal struct PacketReader
    {
        private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(PacketReader).Name);

        #region fields and properties
        private readonly ArraySegment<byte> _array;
        private int _count;

        public ArraySegment<byte> Read
        {
            get { return new ArraySegment<byte>(_array.Array, _array.Offset, _count); }
        }

        public ArraySegment<byte> Unread
        {
            get { return new ArraySegment<byte>(_array.Array, _array.Offset + _count, _array.Count - _count); }
        }

        public ArraySegment<byte> All
        {
            get { return _array; }
        }
        #endregion

        #region constructor
        public PacketReader(ArraySegment<byte> array)
        {
            _array = array;
            _count = 0;
        }

        public PacketReader(byte[] array)
            : this(new ArraySegment<byte>(array))
        {
        }
        #endregion

        #region read primitive
        public PacketReader Skip(int countBytes)
        {
            Check(countBytes, "skipped bytes");
            _count += countBytes;

            return this;
        }

        private void Check(int count, string type)
        {
            if (_array.Count - count - _count < 0)
                throw Log.PossibleBug(string.Format("Insufficient space in packet reader to read {0}", type), "4AFBC61A-77D4-43B8-878F-796F0D921184");
        }

        private byte FastReadByte()
        {
            _count++;
            return _array.Array[_array.Offset + _count - 1];
        }

        public byte ReadByte()
        {
            Check(sizeof(byte), "byte");

            return FastReadByte();
        }

        public ushort ReadUInt16()
        {
            Check(sizeof(ushort), "ushort");

            var un = new Union16 {
                LSB = FastReadByte(),
                MSB = FastReadByte()
            };

            return un.UInt16;
        }

        public ArraySegment<byte> ReadByteSegment()
        {
            //Read length prefix
            var length = ReadUInt16();

            //Now check that the rest of the data is available
            Check(length, "byte[]");

            //Get the segment from the middle of the buffer
            var segment = new ArraySegment<byte>(_array.Array, Unread.Offset, length);
            _count += length;

            return segment;
        }

        public string ReadString()
        {
            //Read the length prefix
            var length = ReadUInt16();

            //Special case for null
            if (length == 0)
                return null;
            else
                length--;

            //Now check that the rest of the string is available
            Check(length, "string");

            //Read the string
            var unread = Unread;
            var str = Encoding.UTF8.GetString(unread.Array, unread.Offset, length);

            //Apply the offset over the string length
            _count += length;

            return str;
        }

        public void SkipString()
        {
            var length = ReadUInt16();
            if (length != 0)
                length--;

            Check(length, "string");
            _count += length;
        }
        #endregion

        #region peek
        public PacketReader Peek(int offset = 0)
        {
            return new PacketReader(
                new ArraySegment<byte>(_array.Array, _array.Offset + _count + offset, _array.Count - _count - offset)
                );
        }
        #endregion

        #region read high level
        public Guid ReadHandshakeResponse()
        {
            var guidBytes = new byte[16];
            ReadByteSegment().CopyTo(guidBytes);

            return new Guid(guidBytes);
        }

        public void ReadVoicePacketHeader(out byte options, out ushort senderId, out ushort sequenceNumber, out ushort numChannels)
        {
            options = ReadByte();
            senderId = ReadUInt16();
            sequenceNumber = ReadUInt16();
            numChannels = ReadUInt16();
        }

        public void ReadVoicePacketChannel(out byte bitfield, out ushort recipient)
        {
            bitfield = ReadByte();
            recipient = ReadUInt16();
        }

        public TextPacket ReadTextPacket(bool readText)
        {
            byte options = ReadByte();
            ushort senderId = ReadUInt16();
            ushort target = ReadUInt16();

            string txt = null;
            if (readText)
                txt = ReadString();
            else
                SkipString();

            return new TextPacket(senderId, (ChannelType)options, target, txt);
        }

        public void ReadPong(out ushort timestamp)
        {
            timestamp = ReadUInt16();
        }
        #endregion
    }
}
