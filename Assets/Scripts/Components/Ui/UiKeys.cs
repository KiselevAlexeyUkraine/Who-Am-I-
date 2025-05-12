using UnityEngine;
using TMPro;
using Components.Player;
using Zenject;
using Components.Items;

namespace Components.Ui
{
    public class UiKeys : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _mainKeysText;
        [SerializeField]
        private TMP_Text _additionalKeysText;

        private DiContainer _container;
		private PlayerInventory _inventory;

        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }

		private void Awake()
        {
            _inventory = _container.Resolve<PlayerInventory>();
            _inventory.OnKeysChanged += UpdateUi;

            UpdateUi();
        }

        private void OnDestroy()
        {
            _inventory.OnKeysChanged -= UpdateUi;
        }

        private void UpdateUi()
        {
            _mainKeysText.text = $"{_inventory.GetKeyAmount(KeyType.Main)}";
            _additionalKeysText.text = $"{_inventory.GetKeyAmount(KeyType.Additional)}";
        }
    }
}
