using Dissonance.Audio.Codecs;
using NAudio.Wave;

namespace Dissonance.Audio.Playback
{
    internal struct FrameFormat
    {
        public Codec Codec;
        public WaveFormat WaveFormat;
        public uint FrameSize;
    }
}