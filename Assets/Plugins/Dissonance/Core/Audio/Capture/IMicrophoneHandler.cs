using System;
using NAudio.Wave;

namespace Dissonance.Audio.Capture
{
    public interface IMicrophoneHandler
    {
        void Handle(ArraySegment<float> buffer, WaveFormat format);
    }
}
