using System;
using System.Collections.Generic;

namespace Dissonance.Audio.Playback
{
    internal struct EncodedAudio
    {
        public class Comparer : IComparer<EncodedAudio>
        {
            public int Compare(EncodedAudio x, EncodedAudio y)
            {
                return x.Sequence.CompareTo(y.Sequence);
            }
        }

        public readonly uint Sequence;
        public readonly ArraySegment<byte> Data;

        public EncodedAudio(uint sequence, ArraySegment<byte> data)
        {
            Sequence = sequence;
            Data = data;
        }
    }
}