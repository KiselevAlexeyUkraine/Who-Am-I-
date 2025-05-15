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
        [SerializeField] private Image _healthBar; // Health UI

        private DiContainer _container;
        private PlayerControls _playerControls;
        private PlayerHealth _playerHealth;

        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }

        private void Awake()
        {
            _playerControls = _container.Resolve<PlayerControls>();
            _playerHealth = _container.Resolve<PlayerHealth>();

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

        private void UpdateStaminaBar(float normalizedValue)
        {
            _staminaBar.fillAmount = Mathf.Clamp01(normalizedValue);
        }

        private void UpdateHealthBar()
        {
            float normalizedHealth = _playerHealth.Health / (float)_playerHealth.MaxHealth;
            _healthBar.fillAmount = Mathf.Clamp01(normalizedHealth);
        }
    }
}
