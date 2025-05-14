using UnityEngine;
using TMPro;
using Zenject;
using Components.Player;
using Components.Items;

namespace Components.Ui
{
    public class UiItems : MonoBehaviour
    {
        [SerializeField] private TMP_Text _medkitCountText;
        [SerializeField] private TMP_Text _batteryCountText;

        [Header("Key UI")]
        [SerializeField] private TMP_Text _redKeyText;
        [SerializeField] private TMP_Text _blueKeyText;
        [SerializeField] private TMP_Text _yellowKeyText;

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
            _inventory.OnItemsChanged += UpdateItemsUi;
            _inventory.OnKeysChanged += UpdateKeysUi;

            UpdateItemsUi();
            UpdateKeysUi();
        }

        private void OnDestroy()
        {
            if (_inventory != null)
            {
                _inventory.OnItemsChanged -= UpdateItemsUi;
                _inventory.OnKeysChanged -= UpdateKeysUi;
            }
        }

        private void UpdateItemsUi()
        {
            _medkitCountText.text = $"x{_inventory.GetMedkitCount()}";
            _batteryCountText.text = $"x{_inventory.GetBatteryCount()}";
        }

        private void UpdateKeysUi()
        {
            _redKeyText.text = $"x{_inventory.GetKeyAmount(KeyType.Red)}";
            _blueKeyText.text = $"x{_inventory.GetKeyAmount(KeyType.Blue)}";
            _yellowKeyText.text = $"x{_inventory.GetKeyAmount(KeyType.Yellow)}";
        }
    }
}
