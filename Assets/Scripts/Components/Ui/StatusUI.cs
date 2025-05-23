using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Components.Player;

namespace Components.Ui
{
    public class StaminaUI : MonoBehaviour
    {
        [Header("UI Reference")]
        [SerializeField] private Image _staminaBar;
        [SerializeField] private Image _healthBar;

        private DiContainer _container;
        private PlayerControls _playerControls;
        private PlayerHealth _playerHealth;

        private Gradient _staminaGradient;
        private Gradient _healthGradient;

        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }

        private void Awake()
        {
            _playerControls = _container.Resolve<PlayerControls>();
            _playerHealth = _container.Resolve<PlayerHealth>();

            InitGradients();

            _playerControls.OnStaminaChanged += UpdateStaminaBar;

            _playerHealth.OnIncrease += UpdateHealthBar;
            _playerHealth.OnDecrease += UpdateHealthBar;

            UpdateStaminaBar(1f);
            UpdateHealthBar();
        }

        private void OnDestroy()
        {
            if (_playerControls != null)
            {
                _playerControls.OnStaminaChanged -= UpdateStaminaBar;
            }

            if (_playerHealth != null)
            {
                _playerHealth.OnIncrease -= UpdateHealthBar;
                _playerHealth.OnDecrease -= UpdateHealthBar;
            }
        }

        private void InitGradients()
        {
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

            _staminaGradient = new Gradient();
            _staminaGradient.SetKeys(colorKeys, alphaKeys);

            _healthGradient = new Gradient();
            _healthGradient.SetKeys(colorKeys, alphaKeys);
        }

        private void UpdateStaminaBar(float normalizedValue)
        {
            normalizedValue = Mathf.Clamp01(normalizedValue);
            _staminaBar.fillAmount = normalizedValue;
            _staminaBar.color = _staminaGradient.Evaluate(normalizedValue);
        }

        private void UpdateHealthBar()
        {
            float normalizedHealth = _playerHealth.Health / (float)_playerHealth.MaxHealth;
            normalizedHealth = Mathf.Clamp01(normalizedHealth);
            _healthBar.fillAmount = normalizedHealth;
            _healthBar.color = _healthGradient.Evaluate(normalizedHealth);
        }
    }
}
