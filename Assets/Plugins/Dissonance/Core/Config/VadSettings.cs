using System;
using Dissonance.VAD;
using UnityEngine;

namespace Dissonance.Config
{
    public class VadSettings
        :
#if !NCRUNCH
        ScriptableObject,
#endif
        IVadConfig
    {
        #region fields and properties
        private static readonly Log Log = Logs.Create(LogCategory.Recording, typeof(VadSettings).Name);

        // ReSharper disable InconsistentNaming
        private const string PersistName_DeactivationHoldTime = "Dissonance_VAD_Advanced_DeactivationHoldTime";
        private const string PersistName_TransientDuration = "Dissonance_VAD_Advanced_TransientDuration";
        private const string PersistName_EnergyDeviationWeight = "Dissonance_VAD_Advanced_EnergyDeviationWeight";
        private const string PersistName_DeltaEnergyMeanWeight = "Dissonance_VAD_Advanced_DeltaEnergyMeanWeight";
        private const string PersistName_Sensitivity = "Dissonance_VAD_Basic_Sensitivity";
        private const string PersistName_IsAdvancedMode = "Dissonance_VAD_IsAdvancedMode";
        // ReSharper restore InconsistentNaming

        public static readonly string SettingsFileResourceName = "VadSettings";
        public static readonly string SettingsFilePath = "Assets/Plugins/Dissonance/Resources/" + SettingsFileResourceName + ".asset";

        [SerializeField]private bool _advancedConfig;
        public bool AdvancedConfig
        {
            get { return _advancedConfig; }
            set
            {
                Preferences.Set(PersistName_IsAdvancedMode, ref _advancedConfig, value, (a, b) => PlayerPrefs.SetInt(a, b ? 1 : 0), Log);
            }
        }

        [SerializeField]private float _sensitivity;
        public float Sensitivity
        {
            get { return _sensitivity; }
            set
            {
                if (value > 1 || value < 0)
                    throw new ArgumentOutOfRangeException("value", "Sensitivity must be between 0 and 1");

                Preferences.Set(PersistName_Sensitivity, ref _sensitivity, value, PlayerPrefs.SetFloat, Log);
                CalculateSettings();
            }
        }

        [SerializeField] private float _deltaEnergyMeanWeight;
        public float DeltaEnergyMeanWeight
        {
            get { return _deltaEnergyMeanWeight; }
            set
            {
                if (!AdvancedConfig)
                    throw new InvalidOperationException("Cannot directly set DeltaEnergyMeanWeight unless in AdvancedConfig mode");

                Preferences.Set(PersistName_DeltaEnergyMeanWeight, ref _deltaEnergyMeanWeight, value, PlayerPrefs.SetFloat, Log);
            }
        }

        [SerializeField]private float _energyDeviationWeight;
        public float EnergyDeviationWeight
        {
            get { return _energyDeviationWeight; }
            set
            {
                if (!AdvancedConfig)
                    throw new InvalidOperationException("Cannot directly set EnergyDeviationWeight unless in AdvancedConfig mode");

                Preferences.Set(PersistName_EnergyDeviationWeight, ref _energyDeviationWeight, value, PlayerPrefs.SetFloat, Log);
            }
        }

        [SerializeField]private int _deactivationHoldTimeInFrames;
        public int DeactivationHoldTime
        {
            get { return _deactivationHoldTimeInFrames; }
            set
            {
                if (!AdvancedConfig)
                    throw new InvalidOperationException("Cannot directly set DeactivationHoldTime unless in AdvancedConfig mode");

                Preferences.Set(PersistName_DeactivationHoldTime, ref _deactivationHoldTimeInFrames, value, PlayerPrefs.SetInt, Log);
            }
        }

        [SerializeField]private int _transientDurationInFrames;
        public int TransientDuration
        {
            get { return _transientDurationInFrames; }
            set
            {
                if (!AdvancedConfig)
                    throw new InvalidOperationException("Cannot directly set TransientDuration unless in AdvancedConfig mode");

                Preferences.Set(PersistName_TransientDuration, ref _transientDurationInFrames, value, PlayerPrefs.SetInt, Log);
            }
        }

        private static VadSettings _instance;
        public static VadSettings Instance
        {
            get { return _instance ?? (_instance = Load()); }
        }
        #endregion

        public VadSettings()
        {
            AdvancedConfig = false;
            _sensitivity = 0.5f;
            CalculateSettings();
        }

        private void CalculateSettings()
        {
            if (!AdvancedConfig)
            {
                _deactivationHoldTimeInFrames = Convert.ToInt32(Mathf.Lerp(24, 11, Sensitivity)); //Decrease hold time as sensitivity increases
                _transientDurationInFrames = Convert.ToInt32(Mathf.Lerp(1, 5, Sensitivity)); //Increase transient duration with sensitivity
                _energyDeviationWeight = Mathf.Lerp(4.1f, 1.1f, Sensitivity); //Decrease energy dev weight with sensitivity
                _deltaEnergyMeanWeight = Mathf.Lerp(0.56f, 0.16f, Sensitivity); //Decrease delta weight with sensitivity
            }
        }

        public static void Preload()
        {
            if (_instance == null)
                _instance = Load();
        }

        private static VadSettings Load()
        {
#if NCRUNCH
            return new VadSettings();
#else
            var settings = Resources.Load<VadSettings>(SettingsFileResourceName) ?? CreateInstance<VadSettings>();

            //Get all the plain settings values
            Preferences.Get(PersistName_DeactivationHoldTime, ref settings._deactivationHoldTimeInFrames, PlayerPrefs.GetInt, Log);
            Preferences.Get(PersistName_TransientDuration, ref settings._transientDurationInFrames, PlayerPrefs.GetInt, Log);
            Preferences.Get(PersistName_EnergyDeviationWeight, ref settings._energyDeviationWeight, PlayerPrefs.GetFloat, Log);
            Preferences.Get(PersistName_DeltaEnergyMeanWeight, ref settings._deltaEnergyMeanWeight, PlayerPrefs.GetFloat, Log);
            Preferences.Get(PersistName_Sensitivity, ref settings._sensitivity, PlayerPrefs.GetFloat, Log);

            //If we're in basic mode, overwrite the advanced settings by calculating them from the sensitivity setting
            Preferences.Get(PersistName_IsAdvancedMode, ref settings._advancedConfig, (a, b) => PlayerPrefs.GetInt(a, 0) == 1, Log);
            if (settings._advancedConfig)
                settings.CalculateSettings();

            return settings;
#endif
        }
    }
}
