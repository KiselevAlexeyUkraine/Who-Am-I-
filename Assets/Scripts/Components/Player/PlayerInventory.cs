using System;
using System.Collections.Generic;
using Components.Items;
using UnityEngine;

namespace Components.Player
{
    public class PlayerInventory : MonoBehaviour
    {
        public event Action OnKeysChanged;
        public event Action OnItemsChanged;
        public event Action OnNotesChanged;

        private Dictionary<KeyType, int> _keys = new();
        private List<string> _notes = new();

        [SerializeField] private int _medkitCount;
        [SerializeField] private int _batteryCount;

        public void AddKey(KeyType type)
        {
            if (!_keys.TryAdd(type, 1))
                _keys[type]++;
            OnKeysChanged?.Invoke();
        }

        public void UseKey(KeyType type)
        {
            if (_keys.TryGetValue(type, out var amount) && amount > 0)
            {
                _keys[type] = amount - 1;
                OnKeysChanged?.Invoke();
            }
        }

        public bool HasKey(KeyType type) =>
            _keys.TryGetValue(type, out var amount) && amount > 0;

        public int GetKeyAmount(KeyType type) =>
            _keys.TryGetValue(type, out var amount) ? amount : 0;

        public void AddMedkit()
        {
            _medkitCount++;
            OnItemsChanged?.Invoke();
        }

        public void UseMedkit()
        {
            if (_medkitCount > 0)
            {
                _medkitCount--;
                OnItemsChanged?.Invoke();
            }
        }

        public int GetMedkitCount() => _medkitCount;

        public void AddBattery()
        {
            _batteryCount++;
            OnItemsChanged?.Invoke();
        }

        public void UseBattery()
        {
            if (_batteryCount > 0)
            {
                _batteryCount--;
                OnItemsChanged?.Invoke();
            }
        }

        public int GetBatteryCount() => _batteryCount;

        public void AddNote(string noteId)
        {
            if (!_notes.Contains(noteId))
            {
                _notes.Add(noteId);
                OnNotesChanged?.Invoke();
            }
        }

        public IReadOnlyList<string> GetNotes() => _notes.AsReadOnly();
    }
}
