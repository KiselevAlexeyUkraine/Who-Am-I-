using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Components.Ui.Pages.Game
{
    public class GameSettingsPage : BasePage
    {
		[SerializeField] private Button _back;
		[SerializeField] private Slider _masterVolumeSlider;
		[SerializeField] private TMP_Text _masterVolumeValue;
		[SerializeField] private Slider _soundsVolumeSlider;
		[SerializeField] private TMP_Text _soundsVolumeValue;
		[SerializeField] private Slider _musicVolumeSlider;
		[SerializeField] private TMP_Text _musicVolumeValue;
		[SerializeField] private Slider _mouseSensitivitySlider;
		[SerializeField] private TMP_Text _mouseSensitivityValue;
		[SerializeField] private Slider _fovSlider;
		[SerializeField] private TMP_Text _fovValue;
		[SerializeField] private Slider _fpsLockSlider;
		[SerializeField] private TMP_Text _fpsLockValue;
		[SerializeField] private Toggle _fpsCounterToggle;
		[SerializeField] private TMP_Text _fpsCounterValue;
		[SerializeField] private Toggle _vsyncToggle;
		[SerializeField] private TMP_Text _vsyncValue;

		private SettingsService _settingsService;

		[Inject]
		private void Construct(SettingsService settingsService)
		{
			_settingsService = settingsService;
			OnClose += settingsService.SaveSettings;
		}

		private void Awake()
		{
			_back.onClick.AddListener(() => { PageSwitcher.Open(PageName.Pause).Forget(); });
			_masterVolumeSlider.onValueChanged.AddListener(value =>
			{
				_settingsService.SetMasterVolume(value);
				_masterVolumeValue.text = $"{value: 0.00}";
			});
			_soundsVolumeSlider.onValueChanged.AddListener(value =>
			{
				_settingsService.SetSoundsVolume(value);
				_soundsVolumeValue.text = $"{value: 0.00}";
			});
			_musicVolumeSlider.onValueChanged.AddListener(value =>
			{
				_settingsService.SetMusicVolume(value);
				_musicVolumeValue.text = $"{value: 0.00}";
			});
			_mouseSensitivitySlider.onValueChanged.AddListener(value =>
			{
				_settingsService.SetMouseSensitivity(value);
				_mouseSensitivityValue.text = $"{value: 0.00}";
			});
			_fovSlider.onValueChanged.AddListener(value =>
			{
				_settingsService.SetFov(value);
				_fovValue.text = $"{value: 0}";
			});
			_fpsLockSlider.onValueChanged.AddListener(value =>
			{
				_settingsService.SetMaxFpsLock((int)value < (int)_fpsLockSlider.minValue + 1 ? -1 : (int)value);
				_fpsLockValue.text = (int)value < (int)_fpsLockSlider.minValue + 1 ? "Выкл." : $"{(int)value}";
			});
			_fpsCounterToggle.onValueChanged.AddListener(value =>
			{
				_settingsService.SetFpsCounter(value);
				_fpsCounterValue.text = value ? "Вкл." : "Выкл.";
			});
			_vsyncToggle.onValueChanged.AddListener(value =>
			{
				_settingsService.SetVsync(value);
				_vsyncValue.text = value ? "Вкл." : "Выкл.";
			});
		}

		private void OnEnable()
		{
			_masterVolumeSlider.value = _settingsService.SavedMasterVolume;
			_soundsVolumeSlider.value = _settingsService.SavedSoundsVolume;
			_musicVolumeSlider.value = _settingsService.SavedMusicVolume;
			_mouseSensitivitySlider.value = _settingsService.SavedMouseSensitivity;
			_fovSlider.value = _settingsService.SavedFov;
			_fpsLockSlider.value = _settingsService.SavedMaxFpsLock;
			_fpsCounterToggle.isOn = _settingsService.SavedFpsCounter;
			_vsyncToggle.isOn = _settingsService.SavedVsync;

			_fpsLockSlider.onValueChanged?.Invoke(_settingsService.SavedMaxFpsLock);
		}

		private void OnDestroy()
		{
			OnClose -= _settingsService.SaveSettings;

			_back.onClick.RemoveAllListeners();
			_masterVolumeSlider.onValueChanged.RemoveAllListeners();
			_soundsVolumeSlider.onValueChanged.RemoveAllListeners();
			_musicVolumeSlider.onValueChanged.RemoveAllListeners();
			_mouseSensitivitySlider.onValueChanged.RemoveAllListeners();
			_fovSlider.onValueChanged.RemoveAllListeners();
			_fpsLockSlider.onValueChanged.RemoveAllListeners();
			_fpsCounterToggle.onValueChanged.RemoveAllListeners();
			_vsyncToggle.onValueChanged.RemoveAllListeners();
		}
	}
}