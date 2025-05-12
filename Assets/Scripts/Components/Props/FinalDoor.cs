using Components.Props;
using Components.Ui.Pages;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Components.Props
{
	[RequireComponent(typeof(Door))]
	public class FinalDoor : MonoBehaviour
	{
		private Door _door;
		private PageSwitcher _pageSwitcher;

		[Inject]
		private void Construct(PageSwitcher pageSwitcher)
		{
			_pageSwitcher = pageSwitcher;
		}

		private void Awake()
		{
			_door = GetComponent<Door>();
			_door.Opened += CompleteLevel;
		}

		private void OnDestroy()
		{
			_door.Opened -= CompleteLevel;
		}

		private void CompleteLevel()
		{
			_pageSwitcher.Open(PageName.Complete).Forget();
		}
	}
}