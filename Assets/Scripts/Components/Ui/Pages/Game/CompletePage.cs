using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Components.Ui.Pages.Game
{
	public class CompletePage : BasePage
	{
		[SerializeField] private TMP_Text _killed;
		[SerializeField] private TMP_Text _time;
		[SerializeField] private Button _nextLevel;
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
			_killed.text = $"0";
			_time.text = $"0";
			_nextLevel.onClick.AddListener(() => { PageSwitcher.Open(PageName.NextLevel).Forget(); });
			_restart.onClick.AddListener(() => { PageSwitcher.Open(PageName.GameRestart).Forget(); });
			_menu.onClick.AddListener(() => { PageSwitcher.Open(PageName.GameExit).Forget(); });
		}

		private void OnDestroy()
		{
			_nextLevel.onClick.RemoveAllListeners();
			_restart.onClick.RemoveAllListeners();
			_menu.onClick.RemoveAllListeners();
		}
	}
}