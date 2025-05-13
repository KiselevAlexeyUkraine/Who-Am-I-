using Components.Player;
using Components.Ui.Pages;
using Services;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private PageSwitcher _gameCanvas;
        [SerializeField] private PauseService _pauseService;
        [SerializeField] private PlayerComponent _player;
        [SerializeField] private PlayerSpawn _playerSpawn;
        [SerializeField] private PauseSwitcher _pauseSwitcher;
        
        public override void InstallBindings()
        {
            Container.Bind<InputService>().FromNew().AsSingle().NonLazy();
            Container.Bind<PauseService>().FromComponentInNewPrefab(_pauseService).AsSingle().NonLazy();
			Container.Bind<PageSwitcher>().FromComponentInNewPrefab(_gameCanvas).AsSingle().NonLazy();
            Container.Bind<PauseSwitcher>().FromComponentInNewPrefab(_pauseSwitcher).AsSingle().NonLazy();
            BindPlayer();	
        }

        private void BindPlayer()
        {
			var player = Container.InstantiatePrefab(
				_player, _playerSpawn.transform.position, _playerSpawn.transform.rotation, null
			);
			Container.Bind<PlayerInventory>().FromComponentOn(player).AsSingle().NonLazy();
			Container.Bind<PlayerHealth>().FromComponentOn(player).AsSingle().NonLazy();
            Container.Bind<FlashlightController>().FromComponentOn(player).AsSingle().NonLazy();
            Container.Bind<PlayerControls>().FromComponentOn(player).AsSingle().NonLazy();
			Container.Bind<PlayerComponent>().FromComponentOn(player).AsSingle().NonLazy();
		}
    }
}