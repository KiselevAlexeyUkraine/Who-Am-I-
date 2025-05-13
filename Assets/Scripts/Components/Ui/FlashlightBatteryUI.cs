using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Components.Player;

namespace Components.Ui
{
    public class FlashlightBatteryUI : MonoBehaviour
    {
        [Header("UI Reference")]
        [SerializeField] private Image _batteryBar;

        private DiContainer _container;
        private FlashlightController _flashlight;

        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }

        private void Awake()
        {
            _flashlight = _container.Resolve<FlashlightController>();

            _flashlight.OnBatteryLevelChanged += SetBatteryLevel;
            SetBatteryLevel(_flashlight.BatteryLevel / 100f);
        }

        private void OnDestroy()
        {
            if (_flashlight != null)
            {
                _flashlight.OnBatteryLevelChanged -= SetBatteryLevel;
            }
        }

        private void SetBatteryLevel(float normalizedValue)
        {
            _batteryBar.fillAmount = Mathf.Clamp01(normalizedValue);
        }
    }
}
