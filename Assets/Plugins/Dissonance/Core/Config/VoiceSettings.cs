using System.ComponentModel;
using JetBrains.Annotations;
using UnityEngine;

namespace Dissonance.Config
{
    public class VoiceSettings
        :
#if !NCRUNCH
        ScriptableObject,
#endif
        INotifyPropertyChanged
    {
        #region fields and properties
        private static readonly Log Log = Logs.Create(LogCategory.Recording, typeof(VadSettings).Name);

        // ReSharper disable InconsistentNaming
        private const string PersistName_Quality = "Dissonance_Audio_Quality";
        private const string PersistName_FrameSize = "Dissonance_Audio_FrameSize";

        private const string PersistName_Denoise = "Dissonance_Audio_Denoise";
        private const string PersistName_DenoiseMaxAttenuation = "Dissonance_Audio_Denoise_MaxAttenuation";

        private const string PersistName_AGC = "Dissonance_Audio_AGC";
        private const string PersistName_AGC_Target = "Dissonance_Audio_AGC_Target";
        private const string PersistName_AGC_MaxGain = "Dissonance_Audio_AGC_MaxGain";
        private const string PersistName_AGC_GainIncrement = "Dissonance_Audio_AGC_GainIncrement";
        private const string PersistName_AGC_GainDecrement = "Dissonance_Audio_AGC_GainDecrement";


        // ReSharper restore InconsistentNaming

        public static readonly string SettingsFileResourceName = "VoiceSettings";
        public static readonly string SettingsFilePath = "Assets/Plugins/Dissonance/Resources/" + SettingsFileResourceName + ".asset";

        [SerializeField]private AudioQuality _quality;
        public AudioQuality Quality
        {
            get { return _quality; }
            set
            {
                Preferences.Set(PersistName_Quality, ref _quality, value, (key, q) => PlayerPrefs.SetInt(key, (int)q), Log, setAtRuntime: false);
                OnPropertyChanged("Quality");
            }
        }

        [SerializeField]private FrameSize _frameSize;
        public FrameSize FrameSize
        {
            get { return _frameSize; }
            set
            {
                Preferences.Set(PersistName_FrameSize, ref _frameSize, value, (key, f) => PlayerPrefs.SetInt(key, (int)f), Log, setAtRuntime: false);
                OnPropertyChanged("FrameSize");
            }
        }

        [SerializeField]private bool _denoise;
        public bool Denoise
        {
            get { return _denoise; }
            set
            {
                Preferences.Set(PersistName_Denoise, ref _denoise, value, Preferences.SetBool, Log);
                _denoise = value;
                OnPropertyChanged("Denoise");
            }
        }

        [SerializeField]private int _denoiseMaxAttenuation;
        public int DenoiseMaxAttenuation
        {
            get { return _denoiseMaxAttenuation; }
            set
            {
                Preferences.Set(PersistName_DenoiseMaxAttenuation, ref _denoiseMaxAttenuation, value, PlayerPrefs.SetInt, Log);
                OnPropertyChanged("DenoiseMaxAttenuation");
            }
        }

        [SerializeField]private bool _agc;
        public bool AGC
        {
            get { return _agc; }
            set
            {
                Preferences.Set(PersistName_AGC, ref _agc, value, Preferences.SetBool, Log);
                OnPropertyChanged("AGC");
            }
        }

        [SerializeField]private float _agcTargetLevel;
        public float AgcTargetLevel
        {
            get { return _agcTargetLevel; }
            set
            {
                Preferences.Set(PersistName_AGC_Target, ref _agcTargetLevel, value, PlayerPrefs.SetFloat, Log);
                OnPropertyChanged("AgcTargetLevel");
            }
        }

        [SerializeField]private int _agcMaxGain;
        public int AgcMaxGain
        {
            get { return _agcMaxGain; }
            set
            {
                Preferences.Set(PersistName_AGC_MaxGain, ref _agcMaxGain, value, PlayerPrefs.SetInt, Log);
                OnPropertyChanged("AgcMaxGain");
            }
        }

        [SerializeField]private int _agcGainIncrement;
        public int AgcGainIncrement
        {
            get { return _agcGainIncrement; }
            set
            {
                Preferences.Set(PersistName_AGC_GainIncrement, ref _agcGainIncrement, value, PlayerPrefs.SetInt, Log);
                OnPropertyChanged("AgcGainIncrement");
            }
        }

        [SerializeField]private int _agcGainDecrement;
        public int AgcGainDecrement
        {
            get { return _agcGainDecrement; }
            set
            {
                Preferences.Set(PersistName_AGC_GainDecrement, ref _agcGainDecrement, value, PlayerPrefs.SetInt, Log);
                OnPropertyChanged("AgcGainDecrement");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private static VoiceSettings _instance;
        public static VoiceSettings Instance
        {
            get { return _instance ?? (_instance = Load()); }
        }
        #endregion

        public VoiceSettings()
        {
            _quality = AudioQuality.Medium;
            _frameSize = FrameSize.Medium;

            _denoise = true;
            _denoiseMaxAttenuation = -15;

            _agc = true;
            _agcTargetLevel = 8000;
            _agcMaxGain = 30;
            _agcGainIncrement = 12;
            _agcGainDecrement = -40;
        }

        public static void Preload()
        {
            if (_instance == null)
                _instance = Load();
        }

        private static VoiceSettings Load()
        {
#if NCRUNCH
            return new VoiceSettings();
#else
            var settings = Resources.Load<VoiceSettings>(SettingsFileResourceName) ?? CreateInstance<VoiceSettings>();

            //Get all the settings values
            Preferences.Get(PersistName_Quality, ref settings._quality, (s, q) => (AudioQuality)PlayerPrefs.GetInt(s, (int)q), Log);
            Preferences.Get(PersistName_FrameSize, ref settings._frameSize, (s, f) => (FrameSize)PlayerPrefs.GetInt(s, (int)f), Log);

            Preferences.Get(PersistName_Denoise, ref settings._denoise, Preferences.GetBool, Log);
            Preferences.Get(PersistName_DenoiseMaxAttenuation, ref settings._denoiseMaxAttenuation, PlayerPrefs.GetInt, Log);

            Preferences.Get(PersistName_AGC, ref settings._agc, Preferences.GetBool, Log);
            Preferences.Get(PersistName_AGC_Target, ref settings._agcTargetLevel, PlayerPrefs.GetFloat, Log);
            Preferences.Get(PersistName_AGC_MaxGain, ref settings._agcMaxGain, PlayerPrefs.GetInt, Log);
            Preferences.Get(PersistName_AGC_GainIncrement, ref settings._agcGainIncrement, PlayerPrefs.GetInt, Log);
            Preferences.Get(PersistName_AGC_GainDecrement, ref settings._agcGainDecrement, PlayerPrefs.GetInt, Log);

            return settings;
#endif
        }
    }
}
