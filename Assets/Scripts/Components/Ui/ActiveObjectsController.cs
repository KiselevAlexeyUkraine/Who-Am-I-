using UnityEngine;
using System.Collections.Generic;
using Services;
using Zenject;

namespace Components.Ui
{
    public class ActiveObjectsController : MonoBehaviour
    {
        [Range(0, 4)]
        [SerializeField] private int activeCount = 0;

        [Tooltip("Назначить ровно 8 GameObjects")]
        [SerializeField] private List<GameObject> objects = new(7);

        private SceneService _sceneService;

        [Inject]
        private void Construct(SceneService sceneService)
        {
            _sceneService = sceneService;
        }

        private void OnEnable()
        {
            int sceneIndex = _sceneService.GetCurrentScene();

            switch (sceneIndex)
            {
                case 2:
                case 3:
                    SetActiveObjects(2);
                    break;
                case 4:
                case 5:
                    SetActiveObjects(4);
                    break;
                case 6:
                    SetActiveObjects(6);
                    break;
                case 7:
                    SetActiveObjects(7);
                    break;
                default:
                    Debug.LogError("Нет ещё уровней");
                    break;
            }
        }

        private void SetActiveObjects(int totalToActivate)
        {
            if (objects == null || objects.Count < 6)
                return;

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] != null)
                    objects[i].SetActive(i < totalToActivate);
            }
        }
    }
}
