using System;
using System.Collections.Generic;
using Components.Items;
using UnityEngine;

namespace Components.Player
{
    public class PlayerInventory : MonoBehaviour
    {
        public event Action OnKeysChanged;
        public event Action OnWeaponAdd;
        public event Action OnAmmoChanged;

        private Dictionary<KeyType, int> _keys = new();

        public void AddKey(KeyType type)
        {
            if (!_keys.TryAdd(type, 1))
            {
                _keys[type]++;
            }

			OnKeysChanged?.Invoke();
        }

        public void UseKey(KeyType type)
        {
            if (_keys.TryGetValue(type, out var amount))
            {
                if (amount > 0)
                {
                    amount--;
                    _keys[type] = amount;
                    OnKeysChanged?.Invoke();
                }
            }
        }
        
        public bool HasKey(KeyType type)
        {
            if (_keys.TryGetValue(type, out var amount))
            {
                return amount > 0;
            }

            return false;
        }

        public int GetKeyAmount(KeyType type)
        {
			if (_keys.TryGetValue(type, out var amount))
			{
				return amount;
			}

            return 0;
		}
    }
}
