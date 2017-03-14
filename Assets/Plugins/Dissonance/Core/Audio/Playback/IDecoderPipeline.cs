using System;

namespace Dissonance.Audio.Playback
{
    internal interface IDecoderPipeline
    {
        int BufferCount { get; }

        ChannelPriority Priority { get; }
        bool Positional { get; }

        void Prepare(SessionContext context);
        bool Read(ArraySegment<float> samples);
    }
}