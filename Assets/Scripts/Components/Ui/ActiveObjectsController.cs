using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Components.Ui
{
    public class ActiveObjectsController : MonoBehaviour
    {
        [Range(0, 3)]
        [SerializeField]
        private int activeCount = 0;

        [Tooltip("Назначить ровно 6 GameObjects")]
        [SerializeField]
        private List<GameObject> objects = new(6);

        private void OnEnable()
        {
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;

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
                case 7:
                    SetActiveObjects(6);
                    break;
                default:
                    SetActiveObjects(Mathf.Clamp(activeCount * 2, 0, 6));
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
