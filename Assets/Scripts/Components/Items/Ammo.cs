using UnityEngine;

namespace Components.Items
{
    public class Ammo : MonoBehaviour
    {
        [SerializeField] private int _amount;

        public int Amount => _amount;
    }
}