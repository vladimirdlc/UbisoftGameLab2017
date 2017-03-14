using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Dissonance.Datastructures;
using Dissonance.Extensions;
using Dissonance.Networking.Client;

namespace Dissonance.Networking
{
    /// <summary>
    /// Helper struct for writing a packet. This struct represents a position within a packet and may be copied to keep a reference to the same position
    /// </summary>
    internal struct PacketWriter
    {
        #region fields and properties
        internal const ushort Magic = 0x8bc7;

        private readonly ArraySegment<byte> _array;
        private int _count;

        /// <summary>
        /// A segment of all the bytes which have been written to so far
        /// </summary>
        public ArraySegment<byte> Written
        {
            get { return new ArraySegment<byte>(_array.Array, _array.Offset, _count); }
        }
        #endregion

        #region constructor
        /// <summary>
        /// Construct a new packet writer to write into the given array
        /// </summary>
        /// <param name="array">Array to write into (starting at index 0)</param>
        public PacketWriter(byte[] array)
            : this(new ArraySegment<byte>(array))
        {
        }

        /// <summary>
        /// Construct a new packet writer to write into the given array segment
        /// </summary>
        /// <param name="array">Segment to write into</param>
        public PacketWriter(ArraySegment<byte> array)
        {
            _array = array;
            _count = 0;
        }
        #endregion

        #region write primitive
        /// <summary>
        /// Write a single byte into the underlying array. Mutate this writer to represent the position after the write.
        /// </summary>
        /// <param name="b">value to write</param>
        /// <returns>A copy of this writer (after the write has been applied)</returns>
        public PacketWriter Write(byte b)
        {
            _array.Array[_array.Offset + _count] = b;
            _count++;

            return this;
        }
        
        /// <summary>
        /// Write an unsigned 16 bit integer into the underlying array. Mutate this writer to represent the position after the write.
        /// </summary>
        /// <param name="u">value to write</param>
        /// <returns>A copy of this writer (after the write has been applied)</returns>
        public PacketWriter Write(ushort u)
        {
            var un = new Union16 { UInt16 = u };
            Write(un.LSB);
            Write(un.MSB);

            return this;
        }

        /// <summary>
        /// Write a string into the underlying array. Mutate this writer to represent the position after the write.
        /// </summary>
        /// <param name="s">value to write (may be null)</param>
        /// <remarks>Should be read with PacketReader.ReadString()</remarks>
        /// <returns>A copy of this writer (after the write has been applied)</returns>
        public PacketWriter Write(string s)
        {
            //Special case for null input
            if (s == null)
            {
                Write((ushort)0);
                return this;
            }

            //Sanity check
            //This is not strictly accurate (we assume every char encodes to one byte, but it's a safe underestimate of string length)
            if (s.Length > ushort.MaxValue)
                throw new ArgumentException("Cannot encode strings with more than 65535 characters");

            //Write the UTF8 string out, leaving 2 bytes at the start for the length
            var length = Encoding.UTF8.GetBytes(s, 0, s.Length, _array.Array, _array.Offset + _count + sizeof(ushort));

            //This check is completely accurate (unlike the one above");
            if (length > ushort.MaxValue)
                throw new ArgumentException("Cannot encode strings which encode to more than 65535 UTF8 bytes");

            //Write out the length header, now we're satisfied everything fits
            Write((ushort)(length + 1));
            _count += length;

            return this;
        }

        /// <summary>
        /// Write some bytes into the underlying array. Mutate this writer to represent the position after the write. </summary>
        /// <param name="data">data to write</param>
        /// <returns>A copy of this writer (after the write has been applied)</returns>
        public PacketWriter Write(ArraySegment<byte> data)
        {
            Write((ushort)data.Count);

            data.CopyTo(_array.Array, _array.Offset + _count);
            _count += data.Count;

            return this;
        }
        
        /// <summary>
        /// Write the constant magic number
        /// </summary>
        internal void WriteMagic()
        {
            Write((ushort)Magic);
        }
        #endregion

        #region write high level

        /// <summary>
        /// Write out a handshake request. Mutate this writer to represent the position after the write.
        /// </summary>
        /// <returns>A copy of this writer (after the write has been applied)</returns>
        public PacketWriter WriteHandshakeRequest()
        {
            WriteMagic();
            Write((byte) MessageTypes.HandshakeRequest);

            return this;
        }

        /// <summary>
        /// Write out a handshake response. Mutate this writer to represent the position after the write.
        /// </summary>
        /// <returns>A copy of this writer (after the write has been applied)</returns>
        public PacketWriter WriteHandshakeResponse(Guid session)
        {
            WriteMagic();
            Write((byte)MessageTypes.HandshakeResponse);
            Write(new ArraySegment<byte>(session.ToByteArray()));

            return this;
        }

        /// <summary>
        /// Write out a client state packet. Mutate this writer to represent the position after the write.
        /// </summary>
        /// <param name="playerName">Name of the local player</param>
        /// <param name="rooms">Rooms state of the local player</param>
        /// <returns>A copy of this writer (after the write has been applied)</returns>
        public PacketWriter WriteClientState(string playerName, Rooms rooms)
        {
            WriteMagic();
            Write((byte)MessageTypes.ClientState);

            //Write out the player name
            Write(playerName);

            //Write out the rooms
            Write((ushort)rooms.Count);
            for (var i = 0; i < rooms.Count; i++)
                Write(rooms[i]);

            return this;
        }

        /// <summary>
        /// Write out a voice packet. Mutate this writer to represent the position after the write.
        /// </summary>
        /// <param name="senderId">Local player ID</param>
        /// <param name="sequenceNumber">Sequence number (monotonically increases with audio frames)</param>
        /// <param name="channels">List of local open channels</param>
        /// <param name="encodedAudio">The encoded audio data</param>
        /// <returns>A copy of this writer (after the write has been applied)</returns>
        internal PacketWriter WriteVoiceData(ushort senderId, ref ushort sequenceNumber, IList<OpenChannel> channels, ArraySegment<byte> encodedAudio)
        {
            WriteMagic();
            Write((byte)MessageTypes.VoiceData);

            Write((byte)0); //A single reserved byte for a bitfield (quality, codec etc)
            Write(senderId);
            Write(unchecked(sequenceNumber++));

            //Write out a list of channels this packet is for
            Write((ushort)channels.Count);
            for (var i = 0; i < channels.Count; i++)
            {
                var channel = channels[i];

                Write(channel.Bitfield);
                Write(channel.Recipient);
            }

            //Write out the encoded audio
            Write(encodedAudio);

            WriteMagic();

            return this;
        }

        /// <summary>
        /// Write out a text message packet. Mutate this writer to represent the position after the write.
        /// </summary>
        /// <param name="senderId">Local player ID</param>
        /// <param name="recipient">Type of recipinent</param>
        /// <param name="target">ID of the recipient (deepnds room or player ID, depends upon recipinent parameter)</param>
        /// <param name="data">Message to send</param>
        /// <returns>A copy of this writer (after the write has been applied)</returns>
        internal PacketWriter WriteTextPacket(ushort senderId, ChannelType recipient, ushort target, string data)
        {
            WriteMagic();
            Write((byte)MessageTypes.TextData);

            Write((byte)recipient);
            Write((ushort)senderId);
            Write((ushort)target);
            Write(data);

            return this;
        }
        #endregion

        public PacketWriter WritePlayerRoutingUpdate(RoutingTable playerIds)
        {
            WriteMagic();
            Write((byte)MessageTypes.PlayerRoutingUpdate);

            playerIds.Serialize(ref this);

            return this;
        }
    }
}
