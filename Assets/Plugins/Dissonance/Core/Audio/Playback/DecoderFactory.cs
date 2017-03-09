using System;
using Dissonance.Audio.Codecs;
using Dissonance.Audio.Codecs.Identity;
using Dissonance.Audio.Codecs.Opus;

namespace Dissonance.Audio.Playback
{
    internal class DecoderFactory
    {
        public static IVoiceDecoder Create(FrameFormat format)
        {
            switch (format.Codec)
            {
                case Codec.Identity:
                    return new IdentityDecoder(format.WaveFormat);
                case Codec.Opus:
                    return new OpusDecoder(format.WaveFormat);
                default:
                    throw new ArgumentOutOfRangeException("format", "Codec not supported");
            }
        }
    }
}