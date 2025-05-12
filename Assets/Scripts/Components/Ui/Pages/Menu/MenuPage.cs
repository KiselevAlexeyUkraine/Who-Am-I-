using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Components.Ui.Pages.Menu
{
    public class MenuPage : BasePage
    {
        [SerializeField] private Button _start;
        [SerializeField] private Button _levels;
        [SerializeField] private Button _settings;
        [SerializeField] private Button _authors;
        [SerializeField] private Button _exit;

        private void Awake()
        {
            _start.onClick.AddListener(() => { PageSwitcher.Open(PageName.Start).Forget(); });
            _levels.onClick.AddListener(() => { PageSwitcher.Open(PageName.Levels).Forget(); });
            _settings.onClick.AddListener(() => { PageSwitcher.Open(PageName.Settings).Forget(); });
            _authors.onClick.AddListener(() => { PageSwitcher.Open(PageName.Authors).Forget(); });
            _exit.onClick.AddListener(() => { PageSwitcher.Open(PageName.Exit).Forget(); });
        }

        private void OnDestroy()
        {
            _start.onClick.RemoveAllListeners();
            _levels.onClick.RemoveAllListeners();
            _settings.onClick.RemoveAllListeners();
            _authors.onClick.RemoveAllListeners();
            _exit.onClick.RemoveAllListeners();
        }
    }
}