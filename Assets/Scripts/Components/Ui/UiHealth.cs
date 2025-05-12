using Components.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Components.Ui
{
    public class UiHealth : MonoBehaviour
    {
        [SerializeField] private Image _healthImage;
        [SerializeField] private TMP_Text _healthText;

        private DiContainer _container;
		private PlayerHealth _playerHealth;

        [Inject]
		private void Construct(DiContainer container)
        {
            _container = container;
        }

		private void Awake()
        {
			_playerHealth = _container.Resolve<PlayerHealth>();

			_playerHealth.OnIncrease += UpdateUi;
			_playerHealth.OnDecrease += UpdateUi;

			UpdateUi();
		}

		private void OnDestroy()
		{
			_playerHealth.OnIncrease -= UpdateUi;
			_playerHealth.OnDecrease -= UpdateUi;
		}

        private void UpdateUi()
        {
            _healthImage.color = Color.HSVToRGB(0f, 0f, _playerHealth.Health / (float)_playerHealth.MaxHealth);
            _healthText.text = $"{_playerHealth.Health}";
        }
    }
}