using UnityEngine;
using Components.Interaction;
using DG.Tweening;

namespace Components.Props
{
    public class InteractableProp : MonoBehaviour, IInteractable
    {
        [Header("Prop Settings")]
        [SerializeField] private PropType propType;
        [SerializeField] private Transform targetTransform;
        [SerializeField] private float rotationAngle = 90f; // угол вращения по Y
        [SerializeField] private float moveDistance = 1f;   // используется для Shelf
        [SerializeField] private float animationDuration = 0.5f;

        private bool isOpen = false;
        private Vector3 initialRotation;
        private Vector3 initialPosition;

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
                    ToggleRotationY();
                    break;

                case PropType.Shelf:
                    ToggleShelfZ();
                    break;

                case PropType.SimpleDoor:
                    if (!isOpen)
                        OpenOnceRotationY();
                    break;
            }
        }

        private void ToggleRotationY()
        {
            float targetY = isOpen
                ? initialRotation.y
                : initialRotation.y + rotationAngle;

            targetTransform.DOLocalRotate(
                new Vector3(initialRotation.x, targetY, initialRotation.z),
                animationDuration
            );

            isOpen = !isOpen;
        }

        private void OpenOnceRotationY()
        {
            float targetY = initialRotation.y + rotationAngle;

            targetTransform.DOLocalRotate(
                new Vector3(initialRotation.x, targetY, initialRotation.z),
                animationDuration
            );

            isOpen = true;
        }

        private void ToggleShelfZ()
        {
            float targetZ = isOpen
                ? initialPosition.z
                : initialPosition.z + moveDistance;

            targetTransform.DOLocalMoveZ(targetZ, animationDuration);

            isOpen = !isOpen;
        }
    }
}
