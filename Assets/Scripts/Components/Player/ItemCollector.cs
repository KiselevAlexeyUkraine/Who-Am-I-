using System;
using Components.Items;
using UnityEngine;

namespace Components.Player
{
    public class ItemCollector : MonoBehaviour
    {
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private PlayerInventory _playerInventory;
        [SerializeField] private LayerMask _itemLayer;

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & _itemLayer.value) == 0)
            {
                return;
            }

            if (!other.TryGetComponent(out Item item))
            {
                return;
            }
            
            switch (item.Type)
            {
                case ItemType.Health:
                    if (item.TryGetComponent(out Health health))
                    {
                        _playerHealth.IncreaseHealth(health.Hp);
                    }
                    break;
                case ItemType.Key:
                    if (item.TryGetComponent(out Key key))
                    {
                        _playerInventory.AddKey(key.Type);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
                
            Destroy(other.gameObject);
        }
    }
}
