using System;
using Dissonance.Config;
using Dissonance.Datastructures;
using NAudio.Wave;

namespace Dissonance.VAD
{
    /// <summary>
    /// Detect voice activity on a microphone signal
    /// </summary>
    /// <remarks>Input signal should be preprocessed with SpeexDsp preprocessor *before* passing into this</remarks>
    /// <remarks>
    /// (Very) loosely based on the algorithm detailed in this paper: http://www.eurasip.org/Proceedings/Eusipco/Eusipco2009/contents/papers/1569192958.pdf
    /// </remarks>
    internal partial class VoiceDetection
    {
        #region fields and properties
        private static readonly Log Log = Logs.Create(LogCategory.Recording, typeof(VoiceDetection).Name);

        private readonly IVadConfig _config;

        public bool IsSpeaking { get; private set; }

        private int _silentFrames;
        internal int SilentFrames
        {
            get { return _silentFrames; }
        }

        private int _speakingFrames;
        internal int SpeakingFrames
        {
            get { return _speakingFrames; }
        }

        private VadState _state;
        internal VadState State
        {
            get { return _state; }
        }

        private StartupState _startup = new StartupState();
        
        private readonly WindowDeviationCalculator _bgEnergyDevCalc = new WindowDeviationCalculator(128);
        private float _bgEnergyDeviation;
        public float BackgroundEnergyDeviation
        {
            get { return _bgEnergyDeviation; }
        }

        private float _backgroundEnergy;
        public float BackgroundEnergy { get { return _backgroundEnergy; } }

        private readonly WindowDeviationCalculator _deltaEnergyDevCalc = new WindowDeviationCalculator(128);
        private float _deltaEnergyMean = 0.00002f;
        public float DeltaEnergyMean
        {
            get { return _deltaEnergyMean; }
        }

        public float CurrentEnergy { get; private set; }

        public bool UseEnergyHeuristic { get; set; }
        #endregion

        #region constructor
        public VoiceDetection(IVadConfig config = null)
        {
            UseEnergyHeuristic = true;

            _config = config ?? VadSettings.Instance;

            Reset();
        }
        #endregion

        /// <summary>
        /// Clear internal state of VAD
        /// </summary>
        public void Reset()
        {
            IsSpeaking = false;
            _silentFrames = 0;
            _speakingFrames = 0;

            _state = VadState.Startup;
            _startup.Reset();

            _backgroundEnergy = float.PositiveInfinity;
        }

        public void Handle(ArraySegment<float> buffer, WaveFormat format)
        {
            //Sanity check
            if (format.Channels != 1)
                throw new InvalidOperationException("Voice detector requires mono signal");

            //Calculate frame statistics
            if (UseEnergyHeuristic)
                CurrentEnergy = CalculateEnergy(buffer);

            var previousState = _state;

            if (_state == VadState.Startup)
            {
                _state = _startup.Handle(this, CurrentEnergy);
            }
            else
            {
                //Apply all the decision factors to determine if this is a frame of speech or not
                var speech = (UseEnergyHeuristic && EnergyDecision(CurrentEnergy));

                switch (_state)
                {
                    case VadState.Silence:
                        _state = HandleSilentState(CurrentEnergy, speech);
                        break;

                    case VadState.ShortSpeech:
                        _state = HandleShortSpeechState(speech);
                        break;

                    case VadState.LongSpeech:
                        _state = HandleLongSpeechState(speech, CurrentEnergy);
                        break;

                    default:
                        throw new InvalidOperationException(string.Format("Unknown VAD state '{0}'", _state));
                }
            }

            if (_state != previousState)
                Log.Trace("VAD State changed to: {0}", _state);
        }

        private VadState HandleSilentState(float energy, bool speech)
        {
            //As soon as speech is detected immediatly transition to short speech
            IsSpeaking = speech;

            //Update counters
            _silentFrames++;
            _speakingFrames = 0;

            //Update silence characteristics
            if (!IsSpeaking && _silentFrames > 10)
            {
                _bgEnergyDevCalc.Update(energy);
                if (_bgEnergyDevCalc.Confidence > 0.5f)
                {
                    _bgEnergyDeviation = _bgEnergyDevCalc.StdDev;
                    _backgroundEnergy = _bgEnergyDevCalc.Mean;
                }
            }

            return speech ? VadState.ShortSpeech : VadState.Silence;
        }

        private VadState HandleShortSpeechState(bool speech)
        {
            //This is only a short talk spurt, so if we stop detecting speech stop classifying as speech instantly
            if (!speech)
            {
                IsSpeaking = false;
                return VadState.Silence;
            }

            //Update counters
            _silentFrames = 0;
            _speakingFrames++;

            //If we detect speech for enough frames in a row, transition to long speech state
            return _speakingFrames > _config.TransientDuration ? VadState.LongSpeech : VadState.ShortSpeech;
        }

        private VadState HandleLongSpeechState(bool speech, float energy)
        {
            //Update silence counter so we can detect a run of silent frames
            if (speech)
                _silentFrames = 0;
            else
                _silentFrames++;

            //Update speech state (transition to silence after a long duration of silence)
            IsSpeaking = _silentFrames <= _config.DeactivationHoldTime;

            //Keep a running measure of the delta between speech energy and BG energy
            var deltaEnergy = energy - _backgroundEnergy;
            _deltaEnergyDevCalc.Update(deltaEnergy);
            _deltaEnergyMean = Math.Max(0, _deltaEnergyDevCalc.Mean);

            //Transition to silence state as necessary
            return IsSpeaking ? VadState.LongSpeech : VadState.Silence;
        }

        private bool EnergyDecision(float energy)
        {
            //Multiple independent measurements of voice
            var bgDev = _bgEnergyDeviation * _config.EnergyDeviationWeight;
            var deltaMean = _deltaEnergyMean * _config.DeltaEnergyMeanWeight;

            //Which is the easiest and hardest tests to pass? Calculate bias to mostly be the hardest test to pass
            var min = Math.Min(bgDev, deltaMean);
            var max = Math.Max(bgDev, deltaMean);
            var bias = 0.9f * max + 0.1f * min;

            //var bias = _bgEnergyDeviation * conf.EnergyDeviationWeight + _deltaEnergyMean * conf.DeltaEnergyMeanWeight;

            //Energy must exceed BG noise by the bias factor
            return energy - bias >= _backgroundEnergy;
        }

        #region Energy
        private static float CalculateEnergy(ArraySegment<float> buffer)
        {
            var sumSqrs = 0f;
            for (var i = 0; i < buffer.Count; i++)
            {
                var v = buffer.Array[i + buffer.Offset];
                sumSqrs += v * v;
            }

            return sumSqrs / buffer.Count;
        }
        #endregion
    }

    public interface IVadConfig
    {
        int TransientDuration { get; }

        float EnergyDeviationWeight { get; }

        float DeltaEnergyMeanWeight { get; }

        int DeactivationHoldTime { get; }
    }
}
