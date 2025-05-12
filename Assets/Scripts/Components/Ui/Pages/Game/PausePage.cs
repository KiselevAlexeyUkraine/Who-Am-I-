using Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Components.Ui.Pages.Game
{
    public class PausePage : BasePage
    {
        [SerializeField] private Button _continue;
        [SerializeField] private Button _restart;
        [SerializeField] private Button _settings;
        [SerializeField] private Button _exit;

        private void Awake()
        {
            _continue.onClick.AddListener(() => { PageSwitcher.Open(PageName.Stats).Forget(); });
            _restart.onClick.AddListener(() => { PageSwitcher.Open(PageName.GameRestart).Forget(); });
            _settings.onClick.AddListener(() => { PageSwitcher.Open(PageName.GameSettings).Forget(); });
            _exit.onClick.AddListener(() => { PageSwitcher.Open(PageName.GameExit).Forget(); });
        }

        private void OnDestroy()
        {
            _continue.onClick.RemoveAllListeners();
            _restart.onClick.RemoveAllListeners();
            _settings.onClick.RemoveAllListeners();
            _exit.onClick.RemoveAllListeners();
        }
    }
}