using UnityEngine;
using TMPro;
using Zenject;
using Components.Player;

namespace Components.Ui
{
    public class UiItems : MonoBehaviour
    {
        [SerializeField] private TMP_Text _medkitCountText;
        [SerializeField] private TMP_Text _batteryCountText;

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
            _inventory.OnItemsChanged += UpdateUi;
            UpdateUi();
        }

        private void OnDestroy()
        {
            if (_inventory != null)
            {
                _inventory.OnItemsChanged -= UpdateUi;
            }
        }

        private void UpdateUi()
        {
            _medkitCountText.text = $"x{_inventory.GetMedkitCount()}";
            _batteryCountText.text = $"x{_inventory.GetBatteryCount()}";
        }
    }
}
