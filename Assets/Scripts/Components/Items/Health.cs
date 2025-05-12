using UnityEngine;

namespace Components.Items
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int _hp;

        public int Hp => _hp;
    }
}