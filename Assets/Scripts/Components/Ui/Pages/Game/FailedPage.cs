using Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Components.Ui.Pages.Game
{
    public class FailedPage : BasePage
    {
        [SerializeField] private Button _restart;
        [SerializeField] private Button _menu;

        [Inject]
        private void Construct(PauseService pauseService, PauseSwitcher pauseSwitcher)
        {
            OnOpen += () =>
            {
                pauseService.Pause();
                pauseSwitcher.enabled = false;
            };
		}

        private void Awake()
        {
            _restart.onClick.AddListener(() => { PageSwitcher.Open(PageName.GameRestart).Forget(); });
            _menu.onClick.AddListener(() => { PageSwitcher.Open(PageName.GameExit).Forget(); });
        }

        private void OnDestroy()
        {
            _restart.onClick.RemoveAllListeners();
            _menu.onClick.RemoveAllListeners();
        }
    }
}