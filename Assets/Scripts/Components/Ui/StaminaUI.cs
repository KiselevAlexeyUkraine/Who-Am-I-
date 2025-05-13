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

        private DiContainer _container;
        private PlayerControls _playerControls;

        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }

        private void Awake()
        {
            _playerControls = _container.Resolve<PlayerControls>();

            _playerControls.OnStaminaChanged += UpdateStaminaBar;
            UpdateStaminaBar(1f);
        }

        private void OnDestroy()
        {
            if (_playerControls != null)
            {
                _playerControls.OnStaminaChanged -= UpdateStaminaBar;
            }
        }

        private void UpdateStaminaBar(float normalizedValue)
        {
            _staminaBar.fillAmount = Mathf.Clamp01(normalizedValue);
        }
    }
}
