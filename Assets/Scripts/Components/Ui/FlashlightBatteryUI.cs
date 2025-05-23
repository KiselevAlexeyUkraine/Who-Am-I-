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

        private Gradient _batteryGradient;

        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }

        private void Awake()
        {
            _flashlight = _container.Resolve<FlashlightController>();

            InitGradient();

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

        private void InitGradient()
        {
            _batteryGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];

            colorKeys[0].color = Color.red;
            colorKeys[0].time = 0.2f; // Red at 20%
            colorKeys[1].color = Color.yellow;
            colorKeys[1].time = 0.5f; // Yellow at 50%
            colorKeys[2].color = Color.green;
            colorKeys[2].time = 1f; // Green at 100%

            alphaKeys[0].alpha = 1f;
            alphaKeys[0].time = 0f;
            alphaKeys[1].alpha = 1f;
            alphaKeys[1].time = 1f;

            _batteryGradient.SetKeys(colorKeys, alphaKeys);
        }

        private void SetBatteryLevel(float normalizedValue)
        {
            normalizedValue = Mathf.Clamp01(normalizedValue);
            _batteryBar.fillAmount = normalizedValue;
            _batteryBar.color = _batteryGradient.Evaluate(normalizedValue);
        }
    }
}
