using UnityEngine;
using System;
using Services;
using Zenject;

namespace Components.Player
{
    public class FlashlightController : MonoBehaviour
    {
        [Header("Flashlight Settings")]
        [SerializeField] private Light _flashlight;
        [Tooltip("Максимальное значение батареи")]
        [SerializeField] private float _batteryCapacity = 100f;
        [Tooltip("Значение расхода батареи в секунду")]
        [SerializeField] private float _drainRate = 5f;

        public float BatteryLevel { get; private set; }
        public bool IsOn => _flashlight.enabled;

        public event Action OnBatteryDepleted;
        public event Action<float> OnBatteryLevelChanged;

        private InputService _input;
        private DiContainer _container;
        private PlayerInventory _inventory;

        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }

        [Inject]
        private void Construct(InputService inputService)
        {
            _input = inputService;
        }

        private void Start()
        {
            _inventory = _container.Resolve<PlayerInventory>();
            BatteryLevel = _batteryCapacity;
            _flashlight.enabled = false;
            NotifyBatteryChange();
        }

        private void Update()
        {
            if (_input.Light)
            {
                Toggle();
            }

            if (_input.ReloadBatarey && BatteryLevel < _batteryCapacity && _inventory.GetBatteryCount() > 0)
            {
                _inventory.UseBattery();
                Recharge(_batteryCapacity);
            }

            if (IsOn)
            {
                BatteryLevel -= _drainRate * Time.deltaTime;
                BatteryLevel = Mathf.Clamp(BatteryLevel, 0f, _batteryCapacity);

                NotifyBatteryChange();

                if (Mathf.Approximately(BatteryLevel, 0f))
                {
                    _flashlight.enabled = false;
                    OnBatteryDepleted?.Invoke();
                }
            }
        }

        public void Toggle()
        {
            if (BatteryLevel > 0f)
            {
                _flashlight.enabled = !_flashlight.enabled;
            }
        }

        public void SetBattery(float value)
        {
            BatteryLevel = Mathf.Clamp(value, 0f, _batteryCapacity);
            NotifyBatteryChange();
        }

        public void Recharge(float amount)
        {
            BatteryLevel = Mathf.Clamp(BatteryLevel + amount, 0f, _batteryCapacity);
            NotifyBatteryChange();
        }

        private void NotifyBatteryChange()
        {
            OnBatteryLevelChanged?.Invoke(BatteryLevel / _batteryCapacity);
        }
    }
}
