using UnityEngine;
using Components.Interaction;
using Components.Items;
using Components.Player;
using Zenject;
using System;
using Components.Ui.Pages;
using DG.Tweening;
using Services;

namespace Components.Props
{
    public class Door : MonoBehaviour, IInteractable
    {
        [Header("Key Settings")]
        [SerializeField] private bool _useKey = true;
        [SerializeField] private KeyType requiredKey = KeyType.Red;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip _openDoor;
        [SerializeField] private AudioClip _closeDoor;

        [Header("Final Door")]
        [SerializeField] private bool _isFinalDoor = false;

        [Header("Tween Settings")]
        [SerializeField] private float _openDuration = 1f;

        public event Action Opened;

        private bool isOpen;
        private DiContainer _container;
        private PlayerInventory _inventory;
        private PageSwitcher _pageSwitcher;
        private AudioService _audioService;

        private Quaternion _initialRotation;
        private Quaternion _targetRotation;

        [Inject]
        private void Construct(DiContainer container, PageSwitcher pageSwitcher, AudioService audioService)
        {
            _container = container;
            _pageSwitcher = pageSwitcher;
            _audioService = audioService;
        }

        private void Start()
        {
            _inventory = _container.Resolve<PlayerInventory>();
            _initialRotation = transform.rotation;
            _targetRotation = _initialRotation * Quaternion.Euler(0, 90f, 0);
        }

        public void Interact()
        {
            if (isOpen)
                return;

            if (!_useKey || (_useKey && _inventory.HasKey(requiredKey)))
            {
                if (_useKey)
                    _inventory.UseKey(requiredKey);

                _audioService?.PlayOneShot(_openDoor);
                gameObject.layer = LayerMask.NameToLayer("Default");

                transform.DORotateQuaternion(_targetRotation, _openDuration)
                         .OnComplete(() =>
                         {
                             isOpen = true;

                             if (_isFinalDoor)
                             {
                                 SaveAndCompleteLevel();
                             }
                         });
            }
            else
            {
                Debug.Log("Key required: " + requiredKey);
                _audioService?.PlayOneShot(_closeDoor);
            }
        }

        private void CompleteLevel()
        {
            _pageSwitcher.Open(PageName.Complete).Forget();
        }

        private void SaveNotesProgress()
        {
            foreach (var note in Note.AllNotes)
            {
                if (note.IsCollected)
                {
                    PlayerPrefs.SetInt($"Note_{note.NoteId}", 1);
                }
            }
            PlayerPrefs.Save();

            Debug.Log("Notes saved via Door script.");
        }

        private void SaveAndCompleteLevel()
        {
            SaveNotesProgress();
            //await System.Threading.Tasks.Task.Delay(1000);
            Opened?.Invoke();
            CompleteLevel();
        }
    }
}
