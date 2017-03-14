using System;
using UnityEngine;

namespace Dissonance.VAD
{
    internal partial class VoiceDetection
    {
        private struct StartupState
        {
            private const int StartupFrames = 50;
            private int _startupCounter;

            internal void Reset()
            {
                _startupCounter = StartupFrames;
            }

            internal VadState Handle(VoiceDetection detector, float energy)
            {
                _startupCounter--;

                //Accumulate background values for all three parameters
                if (Math.Abs(energy) > float.Epsilon)
                    detector._backgroundEnergy = Mathf.Min(energy, detector._backgroundEnergy);

                if (_startupCounter != 0)
                    return VadState.Startup;

                //Initialise deviation measure with a large overestimate of the expected true value
                detector._bgEnergyDeviation = 1;

                return _startupCounter == 0 ? VadState.Silence : VadState.Startup;
            }
        }
    }
}
