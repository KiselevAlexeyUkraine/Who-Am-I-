// File: Components/Props/InteractableProp.cs

using UnityEngine;
using Components.Interaction;
using DG.Tweening;
using Services;
using Zenject;

namespace Components.Props
{
    public class InteractableProp : MonoBehaviour, IInteractable
    {
        [Header("Prop Settings")]
        [SerializeField] private PropType propType;
        [SerializeField] private Transform targetTransform;
        [SerializeField] private float rotationAngle = 90f;

        [Header("Movement Settings")]
        [SerializeField] private float moveDistanceZ = 1f;
        [SerializeField] private float moveDistanceX = 1f;

        [SerializeField] private float animationDuration = 0.5f;

        [Header("Audio Settings")]
        [SerializeField] private AudioClip cabinetDoorOpenSound;
        [SerializeField] private AudioClip shelfOpenSound;
        [SerializeField] private AudioClip drawerOpenSound;

        private bool isOpen = false;
        private Vector3 initialRotation;
        private Vector3 initialPosition;

        private AudioService _audioService;

        [Inject]
        public void Construct(AudioService audioService)
        {
            _audioService = audioService;
        }

        private void Start()
        {
            if (targetTransform == null)
                targetTransform = transform;

            initialRotation = targetTransform.localEulerAngles;
            initialPosition = targetTransform.localPosition;
        }

        public void Interact()
        {
            switch (propType)
            {
                case PropType.CabinetDoor:
                    ToggleRotationY(cabinetDoorOpenSound);
                    break;

                case PropType.Shelf:
                    ToggleShelfZ(shelfOpenSound);
                    break;

                case PropType.Drawer:
                    ToggleDrawerX(drawerOpenSound);
                    break;

                case PropType.SimpleDoor:
                    if (!isOpen)
                        OpenOnceRotationY(cabinetDoorOpenSound);
                    break;
            }
        }

        private void ToggleRotationY(AudioClip clip)
        {
            float targetY = isOpen
                ? initialRotation.y
                : initialRotation.y + rotationAngle;

            targetTransform.DOLocalRotate(
                new Vector3(initialRotation.x, targetY, initialRotation.z),
                animationDuration
            );

            PlaySound(clip);
            isOpen = !isOpen;
        }

        private void OpenOnceRotationY(AudioClip clip)
        {
            float targetY = initialRotation.y + rotationAngle;

            targetTransform.DOLocalRotate(
                new Vector3(initialRotation.x, targetY, initialRotation.z),
                animationDuration
            );

            PlaySound(clip);
            isOpen = true;
        }

        private void ToggleShelfZ(AudioClip clip)
        {
            float targetZ = isOpen
                ? initialPosition.z
                : initialPosition.z + moveDistanceZ;

            targetTransform.DOLocalMoveZ(targetZ, animationDuration);

            PlaySound(clip);
            isOpen = !isOpen;
        }

        private void ToggleDrawerX(AudioClip clip)
        {
            float targetX = isOpen
                ? initialPosition.x
                : initialPosition.x + moveDistanceX;

            targetTransform.DOLocalMoveX(targetX, animationDuration);

            PlaySound(clip);
            isOpen = !isOpen;
        }

        private void PlaySound(AudioClip clip)
        {
            if (_audioService != null && clip != null)
                _audioService.PlayOneShot(clip);
        }
    }

}
