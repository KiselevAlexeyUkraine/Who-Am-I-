using Components.Ui.Pages;
using Services;
using UnityEngine;
using Zenject;

public class PauseSwitcher : MonoBehaviour
{
    private InputService _input;
    private PageSwitcher _pageSwitcher;
    private PauseService _pause;

    private float _escapeCooldown = 0.2f;
    private float _lastEscapeTime;

    [Inject]
    private void Construct(InputService inputService, PageSwitcher pageSwitcher, PauseService pauseService)
    {
        _input = inputService;
        _pageSwitcher = pageSwitcher;
        _pause = pauseService;
    }

    private void Update()
    {
        if (_input.Escape && Time.unscaledTime - _lastEscapeTime >= _escapeCooldown)
        {
            _lastEscapeTime = Time.unscaledTime;

            if (_pause.IsPaused)
            {
                _pageSwitcher.Open(PageName.Stats).Forget();
            }
            else
            {
                _pageSwitcher.Open(PageName.Pause).Forget();
            }
        }
    }
}
