using Services;
using UnityEngine;
using Zenject;

namespace Components.Helpers
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField]
        private string _sceneName;
        
        private SceneService _sceneService;

        [Inject]
        private void Construct(SceneService sceneService)
        {
            _sceneService = sceneService;
        }
        
        private void Start()
        {
            _sceneService.Load(_sceneName);
        }
    }
}
