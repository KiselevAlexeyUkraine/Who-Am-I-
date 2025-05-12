using System;
using UnityEngine;

namespace Components.Enemy
{
    public class DamageablePart : MonoBehaviour, IDamageable
    {
        public event Action<float> OnTakeDamage;

        public void TakeDamage(int amount)
        {
            OnTakeDamage?.Invoke(amount);
        }
    }
}
