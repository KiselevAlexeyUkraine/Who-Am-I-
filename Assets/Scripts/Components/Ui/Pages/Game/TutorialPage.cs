using Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Components.Ui.Pages.Game
{
    public class TutorialPage : BasePage
    {
        [SerializeField] private Button _continue;

        [Inject]
        private void Construct(PauseService pauseService, PauseSwitcher pauseSwitcher)
        {
            OnOpen += () =>
            {
                pauseService.Pause();
            };
        }

        private void Awake()
        {
            _continue.onClick.AddListener(() => { PageSwitcher.Open(PageName.Stats).Forget(); });
        }

        private void OnDestroy()
        {
            _continue.onClick.RemoveAllListeners();
        }
    }
}
