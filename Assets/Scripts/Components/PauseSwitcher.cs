using Components.Ui.Pages;
using Services;
using UnityEngine;
using Zenject;

public class PauseSwitcher : MonoBehaviour
{
	private InputService _input;
	private PageSwitcher _pageSwitcher;
	private PauseService _pause;

	[Inject]
	private void Construct(InputService inputService, PageSwitcher pageSwitcher, PauseService pauseService)
	{
		_input = inputService;
		_pageSwitcher = pageSwitcher;
		_pause = pauseService;
	}

	private void Update()
	{
		if (_input.Escape)
		{
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
