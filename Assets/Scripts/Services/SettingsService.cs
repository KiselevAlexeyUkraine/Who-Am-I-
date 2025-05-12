using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Services
{
    public class SettingsService : MonoBehaviour
    {
        public event Action<bool> OnChanged;
        
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private float _defaultMasterVolume = 0.5f; 
        [SerializeField] private float _defaultSoundsVolume = 0.5f; 
        [SerializeField] private float _defaultMusicVolume = 0.5f; 
        [SerializeField] private float _defaultMouseSensitivity = 2f; 
        [SerializeField] private float _defaultFov = 80f; 
        [SerializeField] private int _defaultFpsLock = -1; 
        [SerializeField] private int _defaultFpsCounter = 0; 
        [SerializeField] private int _defaultVsync = 0; 
        
        private const string MasterVolumePrefKey = "MasterVolume"; 
        private const string SoundsVolumePrefKey = "SoundsVolume"; 
        private const string MusicVolumePrefKey = "MusicVolume";
        private const string MouseSensitivityPrefKey = "MouseSensitivity";
        private const string FovPrefKey = "Fov";
        private const string FpsCounterPrefKey = "FpsCounter";
        private const string MaxFpsLockPrefKey = "FpsLock";
        private const string VsyncPrefKey = "Vsync";
        
        public float SavedMasterVolume { get; private set; }
        public float SavedSoundsVolume { get; private set; }
        public float SavedMusicVolume { get; private set; }
        public float SavedMouseSensitivity { get; private set; }
        public float SavedFov { get; private set; }
        public bool SavedFpsCounter { get; private set; }
        public bool SavedVsync { get; private set; }
        public int SavedMaxFpsLock { get; private set; }
        
        private void Awake()
        {
            SavedMasterVolume = PlayerPrefs.GetFloat(MasterVolumePrefKey, _defaultMasterVolume);
            SavedSoundsVolume = PlayerPrefs.GetFloat(SoundsVolumePrefKey, _defaultSoundsVolume);
            SavedMusicVolume = PlayerPrefs.GetFloat(MusicVolumePrefKey, _defaultMusicVolume);
            SavedMouseSensitivity = PlayerPrefs.GetFloat(MouseSensitivityPrefKey, _defaultMouseSensitivity);
            SavedFov = PlayerPrefs.GetFloat(FovPrefKey, _defaultFov);
            SavedFpsCounter = PlayerPrefs.GetInt(FpsCounterPrefKey, _defaultFpsCounter) == 1;
            SavedMaxFpsLock = PlayerPrefs.GetInt(MaxFpsLockPrefKey, _defaultFpsLock);
            SavedVsync = PlayerPrefs.GetInt(VsyncPrefKey, _defaultVsync) == 1;
        }

        private void Start()
        {
            SetMasterVolume(SavedMasterVolume);
            SetSoundsVolume(SavedSoundsVolume);
            SetMusicVolume(SavedMusicVolume);
            SetMouseSensitivity(SavedMouseSensitivity);
            SetFov(SavedFov);
            SetFpsCounter(SavedFpsCounter);
            SetMaxFpsLock(SavedMaxFpsLock);
            SetVsync(SavedVsync);
        }

        public void SetMasterVolume(float value)
        {
            SavedMasterVolume = value;
            SetVolume(value, MasterVolumePrefKey);
        }

        public void SetSoundsVolume(float value)
        {
            SavedSoundsVolume = value;
            SetVolume(value, SoundsVolumePrefKey);
        }

        public void SetMusicVolume(float value)
        {
            SavedMusicVolume = value;
            SetVolume(value, MusicVolumePrefKey);
        }
        
        public void SetMouseSensitivity(float value)
        {
            SavedMouseSensitivity = value;
            PlayerPrefs.SetFloat(MouseSensitivityPrefKey, value);
        }

		public void SetFov(float value)
		{
			SavedFov = value;
			PlayerPrefs.SetFloat(FovPrefKey, value);
		}

		public void SetFpsCounter(bool value)
        {
            SavedFpsCounter = value;
            PlayerPrefs.SetInt(FpsCounterPrefKey, value ? 1 : 0);
            OnChanged?.Invoke(value);
        }

        public void SetMaxFpsLock(int value)
        {
            SavedMaxFpsLock = value;
            Application.targetFrameRate = value;
            PlayerPrefs.SetInt(MaxFpsLockPrefKey, value);
        }

        public void SetVsync(bool value)
        {
            SavedVsync = value;
            QualitySettings.vSyncCount = SavedVsync ? 1 : 0;
            PlayerPrefs.SetInt(VsyncPrefKey, value ? 1 : 0);
        }

        public void SaveSettings()
        {
            PlayerPrefs.Save();
        }

        private void SetVolume(float value, string name)
        {
            value = Mathf.Clamp(value, 0.0001f, 1f);
            var volume = Mathf.Log10(value) * 20f;
            
            _mixer.SetFloat(name, volume); 
            
            PlayerPrefs.SetFloat(name, value); 
        }
    }
}