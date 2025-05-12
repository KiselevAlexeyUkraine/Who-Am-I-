using System;
using Components.Ui.Pages;
using UnityEngine;
using Zenject;

namespace Components.Player
{

    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public event Action OnDecrease;
        public event Action OnIncrease;
        public event Action OnDied; 

        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _health;

        private bool _isDead;

        public int Health => _health;
        public int MaxHealth => _maxHealth;
        public bool IsDead => _isDead;

        [Inject]
        private void Construct(PageSwitcher pageSwitcher)
        {
            OnDied += () => pageSwitcher.Open(PageName.Failed).Forget();
        }

        private void Awake()
        {
            _health = Mathf.Clamp(_health, 0, _maxHealth);
        }

        public void IncreaseHealth(int amount = 1)
        {
            if (!_isDead && amount > 0)
            {
                _health = Mathf.Clamp(_health + amount, 0, _maxHealth);
                OnIncrease?.Invoke();
            }
        }

        public void DecreaseHealth(int amount = 1)
        {
            if (_isDead || amount <= 0)
            {
                return;
            }
            
            _health = Mathf.Clamp(_health - amount, 0, _maxHealth);
            OnDecrease?.Invoke();

            if (_health <= 0)
            {
                Die();
            }
        }

        public void TakeDamage(int amount)
        {
            DecreaseHealth(amount);
        }

        private void Die()
        {
            if (_isDead)
            {
                return;
            }

            _isDead = true;
            OnDied?.Invoke();
        }
    }
}
