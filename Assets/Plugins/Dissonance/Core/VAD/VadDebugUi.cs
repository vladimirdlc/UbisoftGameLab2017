using System;
using Dissonance.Audio.Capture;
using Dissonance.Config;
using UnityEngine;

namespace Dissonance.VAD
{
    public class VadDebugUi
        : MonoBehaviour, IVoiceActivationListener
    {
        private MicrophoneCapture _subscribed;

        public void OnGUI()
        {
            using (new GUILayout.AreaScope(new Rect(250, 0, 500, 500)))
            {

                var comms = FindObjectOfType<DissonanceComms>();
                if (comms == null)
                {
                    GUILayout.Label("VadDebugUi: DissonanceComms is null!");
                    return;
                }

                if (comms.MicCapture == null)
                {
                    GUILayout.Label("VadDebugUi: Mic is null!");
                    return;
                }

                var vad = comms.MicCapture.VAD;
                if (vad == null)
                {
                    GUILayout.Label("VadDebugUi: VAD is null!");
                    return;
                }

                //Subscribe to the VAD (must implement IVoiceActivationListener). This ensures that the VAD is active and analysing the microphone signal.
                if (_subscribed == null)
                {
                    _subscribed = comms.MicCapture;
                    _subscribed.Subscribe(this);
                }

                switch (vad.State)
                {
                    case VadState.Startup:
                        GUILayout.Label(new GUIContent(vad.State.ToString(), "Measuring background noise"));
                        break;
                    case VadState.Silence:
                        GUILayout.Label(new GUIContent(vad.State.ToString(), "No one is speaking"));
                        break;
                    case VadState.ShortSpeech:
                        GUILayout.Label(new GUIContent(vad.State.ToString(), "Someone spoke (or made a noise)"));
                        break;
                    case VadState.LongSpeech:
                        GUILayout.Label(new GUIContent(vad.State + " " + (VadSettings.Instance.DeactivationHoldTime - vad.SilentFrames), "Someone spoke"));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                VadSettings.Instance.AdvancedConfig = GUILayout.Toggle(VadSettings.Instance.AdvancedConfig, "Advanced Configuration");
                if (VadSettings.Instance.AdvancedConfig)
                {
                    GUILayout.Label(new GUIContent(
                                        "Δ Mean Weight: " + VadSettings.Instance.DeltaEnergyMeanWeight.ToString("0.0000000"),
                                        "Weight of 'Δ Mean' in bias calculation. Lower is more sensitive, higher adapts better to changing noise environment"
                                    ));
                    VadSettings.Instance.DeltaEnergyMeanWeight = GUILayout.HorizontalScrollbar(VadSettings.Instance.DeltaEnergyMeanWeight, 0.01f, 0, 2f, GUILayout.MinWidth(150));

                    GUILayout.Label(new GUIContent(
                                        "Background σ Weight: " + VadSettings.Instance.EnergyDeviationWeight.ToString("0.0000000"),
                                        "Weight of 'Background σ' in bias calculation. Lower is more sensitive, higher is less likely to misclassify noise as speech."
                                    ));
                    VadSettings.Instance.EnergyDeviationWeight = GUILayout.HorizontalScrollbar(VadSettings.Instance.EnergyDeviationWeight, 0.01f, 0.5f, 3.5f, GUILayout.MinWidth(150));

                    GUILayout.Label(new GUIContent(
                                        "Transient Length: " + VadSettings.Instance.TransientDuration,
                                        "If a short sound is below this length (in frames) the hold time will not be applied"
                                    ));
                    VadSettings.Instance.TransientDuration = (int)GUILayout.HorizontalScrollbar(VadSettings.Instance.TransientDuration, 1, 0, 10);

                    GUILayout.Label(new GUIContent(
                                        "Hold Time: " + VadSettings.Instance.DeactivationHoldTime,
                                        "How many frames should be considered speech after signal becomes quiet"
                                    ));
                    VadSettings.Instance.DeactivationHoldTime = (int)GUILayout.HorizontalScrollbar(VadSettings.Instance.DeactivationHoldTime, 1, 0, 50);
                }
                else
                {
                    GUILayout.Label(new GUIContent(
                                        "Sensitivity: " + VadSettings.Instance.Sensitivity,
                                        "How sensitive should the voice detector be"
                                    ));
                    VadSettings.Instance.Sensitivity = GUILayout.HorizontalScrollbar(VadSettings.Instance.Sensitivity, 0.01f, 0, 1);
                }

                GUILayout.Label("Energy:");
                GUILayout.Label(new GUIContent(
                    " - Current: " + vad.CurrentEnergy.ToString("0.0000000"),
                    "Current volume"
                ));
                GUILayout.Label(new GUIContent(
                    " - Δ Mean: " + vad.DeltaEnergyMean.ToString("0.0000000"),
                    "Average difference between speech and background noise"
                ));

                GUILayout.Label(new GUIContent(
                    " - BG Mean: " + vad.BackgroundEnergy.ToString("0.0000000"),
                    "Average background noise"
                ));
                GUILayout.Label(new GUIContent(
                    " - BG σ: " + vad.BackgroundEnergyDeviation.ToString("0.0000000"),
                    "Standard Deviation of background noise"
                ));

                GUILayout.Label(new GUIContent(
                    "Bias: " + (vad.BackgroundEnergyDeviation * VadSettings.Instance.EnergyDeviationWeight + vad.DeltaEnergyMean * VadSettings.Instance.DeltaEnergyMeanWeight).ToString("0.0000000"),
                    "IsSpeech = Energy - Bias > Background. Derived from (Δ Mean) and (Background σ)"
                ));

                GUILayout.Label(GUI.tooltip, new GUIStyle() {
                    fontSize = 22,
                    wordWrap = true
                });
            }
        }

        public void OnDisable()
        {
            if (_subscribed != null)
            {
                _subscribed.Unsubscribe(this);
                _subscribed = null;
            }
        }

        //We don't actually care about the start and stop events, so these methods are empty.
        //We subscribe to the VAD just to make sure it is active and all the *other* stats are relevant
        #region IVoiceActivationListener implementation
        void IVoiceActivationListener.VoiceActivationStart()
        {
        }

        void IVoiceActivationListener.VoiceActivationStop()
        {
        }
        #endregion
    }
}
