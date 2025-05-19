using UnityEngine;
using Components.Interaction;
using Components.Items;
using Components.Player;
using Zenject;
using System;
using Components.Ui.Pages;

namespace Components.Props
{
    public class Door : MonoBehaviour, IInteractable
    {
        [Header("Key Settings")]
        [SerializeField] private bool _useKey = true;
        [SerializeField] private KeyType requiredKey = KeyType.Red;
        [SerializeField] private AudioClip _closeDoor;

        [Header("Final Door")]
        [SerializeField] private bool _isFinalDoor = false;

        public event Action Opened;

        private Animator _animator;
        private AudioSource _audioSource;
        private bool isOpen;

        private DiContainer _container;
        private PlayerInventory _inventory;
        private PageSwitcher _pageSwitcher;

        [Inject]
        private void Construct(DiContainer container, PageSwitcher pageSwitcher)
        {
            _container = container;
            _pageSwitcher = pageSwitcher;
        }

        private void Start()
        {
            _inventory = _container.Resolve<PlayerInventory>();
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
        }

        public void Interact()
        {
            if (isOpen)
                return;

            if (!_useKey || (_useKey && _inventory.HasKey(requiredKey)))
            {
                if (_useKey)
                    _inventory.UseKey(requiredKey);

                _animator.Play("Open");
                _audioSource?.Play();
                isOpen = true;
                gameObject.layer = LayerMask.NameToLayer("Default");

                if (_isFinalDoor)
                {
                    Opened?.Invoke();
                    CompleteLevel();
                }
            }
            else
            {
                Debug.Log("Key required: " + requiredKey);
                _audioSource?.PlayOneShot(_closeDoor);
            }
        }

        private void CompleteLevel()
        {
            _pageSwitcher.Open(PageName.Complete).Forget();
        }
    }
}
