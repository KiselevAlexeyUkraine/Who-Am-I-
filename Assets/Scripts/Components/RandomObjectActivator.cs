using UnityEngine;

namespace Components
{
    public class RandomObjectActivator : MonoBehaviour
    {
        [SerializeField] private GameObject[] items;
        [SerializeField] private GameObject[] traps;
        [SerializeField] private GameObject[] enemies;

        void Start()
        {
            int randomNumber = Random.Range(0, 2); // 0 или 1

            ActivateObjectByIndex(items, randomNumber);
            ActivateObjectByIndex(traps, randomNumber);
            ActivateObjectByIndex(enemies, randomNumber);
        }

        private void ActivateObjectByIndex(GameObject[] array, int index)
        {
            if (array != null && array.Length > index && array[index] != null)
            {
                array[index].SetActive(true);
            }
        }
    }
}