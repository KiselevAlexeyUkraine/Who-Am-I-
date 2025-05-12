using Components.Items;
using Components.Player;
using System;
using UnityEngine;
using Zenject;

namespace Components.Props
{
    public class Door : MonoBehaviour
    {
        public event Action Opened;

        [SerializeField] private Collider _trigger;
        [SerializeField] private Transform _door;
        [SerializeField] private KeyType _type;
        [SerializeField] private float _openingDuration;
    
        private PlayerInventory _playerInventory;

        [Inject]
        private void Construct(PlayerInventory inventory)
        {
            _playerInventory = inventory;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && _playerInventory.HasKey(_type))
            {
                _playerInventory.UseKey(_type);
                _trigger.enabled = false;
                
            }
        }
    }
}
