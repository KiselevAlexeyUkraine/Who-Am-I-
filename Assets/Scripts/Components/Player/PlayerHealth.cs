using System;
using Components.Ui.Pages;
using Services;
using UnityEngine;
using Zenject;

namespace Components.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public static int DeathCount = 0;

        public event Action OnDecrease;
        public event Action OnIncrease;
        public event Action OnDied;

        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _health;
        [SerializeField] private int _healAmount = 25;

        private bool _isDead;

        public int Health => _health;
        public int MaxHealth => _maxHealth;
        public bool IsDead => _isDead;

        private InputService _input;
        private PlayerInventory _inventory;
        private DiContainer _container;

        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }

        [Inject]
        private void Construct(InputService inputService, PageSwitcher pageSwitcher)
        {
            _input = inputService;
            OnDied += () => pageSwitcher.Open(PageName.Failed).Forget();
        }

        private void Start()
        {
            _inventory = _container.Resolve<PlayerInventory>();
            _health = Mathf.Clamp(_health, 0, _maxHealth);
        }

        private void Update()
        {
            if (_input.MedicineChest && _health < _maxHealth && _inventory.GetMedkitCount() > 0)
            {
                _inventory.UseMedkit();
                IncreaseHealth(_healAmount);
                Debug.Log("Использована аптечка");
            }
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
            if (_isDead || amount <= 0) return;

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
            if (_isDead) return;

            _isDead = true;
            DeathCount++;
            OnDied?.Invoke();
        }
    }
}