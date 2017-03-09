using System;
using NAudio.Wave;

namespace Dissonance.Audio.Playback
{
    internal interface ISampleSource
    {
        WaveFormat WaveFormat { get; }

        void Prepare(SessionContext context);
        bool Read(ArraySegment<float> samples);
        void Reset();
    }
}