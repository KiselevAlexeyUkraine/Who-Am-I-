using Services;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private EventSystem _eventSystem;
        [SerializeField] private SettingsService _settingsService;
        [SerializeField] private MusicService _musicService;
        [SerializeField] private AudioService _audioService;
        
        public override void InstallBindings()
        {
            Container.Bind<SceneService>().FromNew().AsSingle().NonLazy();
            Container.Bind<SettingsService>().FromComponentInNewPrefab(_settingsService).AsSingle().NonLazy();
            Container.Bind<MusicService>().FromComponentInNewPrefab(_musicService).AsSingle().NonLazy();
            Container.Bind<AudioService>().FromComponentInNewPrefab(_audioService).AsSingle().NonLazy();
            Container.Bind<EventSystem>().FromComponentInNewPrefab(_eventSystem).AsSingle().NonLazy();
        }
    }
}