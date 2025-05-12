using Services;
using Zenject;

namespace Components.Ui.Pages.Game
{
    public class GameStart : BasePage
    {
        [Inject]
        private void Construct(PauseService pauseService)
        {
            OnOpen += () =>
            {
                pauseService.Play();
            };
            Opened += () => 
            {
                PageSwitcher.Open(PageName.Stats).Forget(); 
            };
        }
    }
}