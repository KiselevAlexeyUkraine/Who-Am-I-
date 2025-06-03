using UnityEngine;

namespace Components
{
    public class RandomObjectActivator : MonoBehaviour
    {
        [SerializeField] private GameObject[] items;
        [SerializeField] private GameObject[] traps;
        [SerializeField] private GameObject[] enemies;
        [SerializeField] private GameObject[] lights;

        void Start()
        {
            int randomNumber = Random.Range(0, 2);

            ToggleObjectsByIndex(items, randomNumber);
            ToggleObjectsByIndex(traps, randomNumber);
            ToggleObjectsByIndex(enemies, randomNumber);
            ToggleObjectsByIndex(lights, randomNumber);
        }

        private void ToggleObjectsByIndex(GameObject[] array, int indexToActivate)
        {
            if (array == null || array.Length < 2) return;

            int indexToDeactivate = 1 - indexToActivate;

            if (array.Length > indexToActivate && array[indexToActivate] != null)
                array[indexToActivate].SetActive(true);

            if (array.Length > indexToDeactivate && array[indexToDeactivate] != null)
                array[indexToDeactivate].SetActive(false);
        }
    }
}