using UnityEngine;
using TMPro;
using Zenject;
using Components.Player;
using Components.Items;

namespace Components.Ui
{
    public class UiItems : MonoBehaviour
    {
        [Header("Item Counts")]
        [SerializeField] private TMP_Text _medkitCountText;
        [SerializeField] private TMP_Text _batteryCountText;

        [Header("Key Counts")]
        [SerializeField] private TMP_Text _redKeyText;
        [SerializeField] private TMP_Text _blueKeyText;
        [SerializeField] private TMP_Text _yellowKeyText;

        [Header("Notes")]
        [SerializeField] private TMP_Text _noteCountText;

        private int _totalNotesInLevel;
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
            _inventory.OnKeysChanged += UpdateUi;
            _inventory.OnNotesChanged += UpdateUi;

            CountNotesInLevel();
            UpdateUi();
        }

        private void OnDestroy()
        {
            if (_inventory != null)
            {
                _inventory.OnItemsChanged -= UpdateUi;
                _inventory.OnKeysChanged -= UpdateUi;
                _inventory.OnNotesChanged -= UpdateUi;
            }
        }

        private void CountNotesInLevel()
        {
            GameObject[] notes = GameObject.FindGameObjectsWithTag("Note");
            _totalNotesInLevel = notes.Length;
        }

        private void UpdateUi()
        {
            // Items
            _medkitCountText.text = $"x {_inventory.GetMedkitCount()}";
            _batteryCountText.text = $"x {_inventory.GetBatteryCount()}";

            // Keys
            _redKeyText.text = $"x {_inventory.GetKeyAmount(KeyType.Red)}";
            _blueKeyText.text = $"x {_inventory.GetKeyAmount(KeyType.Blue)}";
            _yellowKeyText.text = $"x {_inventory.GetKeyAmount(KeyType.Yellow)}";

            // Notes
            int currentNoteCount = _inventory.GetNotes().Count;
            _noteCountText.text = $"{currentNoteCount} / {_totalNotesInLevel}";
        }
    }
}
