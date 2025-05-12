using System;
using System.Collections.Generic;
using UnityEngine;

namespace Components.Enemy
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private float _maxHealth = 100f;

        private float _currentHealth;
        private List<DamageablePart> _damageableParts;

        public event Action<float> OnHealthChanged;
        public event Action OnDeath;


        private void Awake()
        {
            _currentHealth = _maxHealth;
            _damageableParts = new List<DamageablePart>(GetComponentsInChildren<DamageablePart>());
            foreach (var part in _damageableParts)
            {
                part.OnTakeDamage += TakeDamage;
            }

            Debug.Log(_damageableParts.Count);
        }

        private void OnDestroy()
        {
            foreach (var part in _damageableParts)
            {
                part.OnTakeDamage -= TakeDamage;
            }
        }

        private void TakeDamage(float damage)
        {
            _currentHealth -= damage;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
            OnHealthChanged?.Invoke(_currentHealth);
            Debug.Log("Осталось здоровья у врага " + _currentHealth);
            if (_currentHealth <= 0)
            {
                OnDeath?.Invoke();
            }
        }
    }
}
