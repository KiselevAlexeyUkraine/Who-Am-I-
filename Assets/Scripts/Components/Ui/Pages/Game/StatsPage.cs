using Services;
using Zenject;

namespace Components.Ui.Pages.Game
{
    public class StatsPage : BasePage
    {
        [Inject]
        private void Construct(PauseService pauseService)
        {
            OnOpen += () => pauseService.Play();
            OnClose += () => pauseService.Pause();
        }
    }
}