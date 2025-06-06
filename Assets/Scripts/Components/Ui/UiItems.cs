using UnityEngine;
using TMPro;
using Zenject;
using Components.Player;
using Components.Items;
using System.Linq;
using UnityEngine.UI;

namespace Components.Ui
{
    public class UiItems : MonoBehaviour
    {
        [Header("Количество предметов")]
        [SerializeField] private TMP_Text _medkitCountText;
        [SerializeField] private TMP_Text _batteryCountText;

        [Header("Количество ключей")]
        [SerializeField] private TMP_Text _redKeyText;
        [SerializeField] private TMP_Text _blueKeyText;
        [SerializeField] private TMP_Text _yellowKeyText;

        [Header("Записки")]
        [SerializeField] private TMP_Text _noteCountText;
        [SerializeField] private Image _notePanel;

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
            _medkitCountText.text = $"x {_inventory.GetMedkitCount()}";
            _batteryCountText.text = $"x {_inventory.GetBatteryCount()}";

            _redKeyText.text = $"x {_inventory.GetKeyAmount(KeyType.Red)}";
            _blueKeyText.text = $"x {_inventory.GetKeyAmount(KeyType.Blue)}";
            _yellowKeyText.text = $"x {_inventory.GetKeyAmount(KeyType.Yellow)}";

            int currentNoteCount = _inventory.GetNotes().Count;

            // Проверка наличия хотя бы одной не собранной записки по сохранениям
            bool anyUncollected = Note.AllNotes.Any(note => PlayerPrefs.GetInt($"Note_{note.NoteId}", 0) == 0);

            if (_totalNotesInLevel == 0 || !anyUncollected)
            {
                _notePanel.enabled = false;
                _noteCountText.enabled = false;
            }
            else
            {
                _noteCountText.text = $"{currentNoteCount} / {_totalNotesInLevel}";
                _notePanel.enabled = true;
                _noteCountText.enabled = true;
            }
        }
    }
}
