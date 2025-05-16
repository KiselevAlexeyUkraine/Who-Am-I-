using UnityEngine;
using Services;
using Zenject;
using Components.Player;
using Components.Items;

namespace Components.Interaction
{
    public class InteractionRaycaster : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private float _rayDistance = 3f;
        [SerializeField] private LayerMask _interactableLayer;

        [Header("Pickup Settings")]
        [SerializeField] private Transform _holdPoint;
        [SerializeField] private string _heldLayerName = "Held";
        [SerializeField] private Vector3 _pickupRotationEuler = Vector3.zero;

        public static event System.Action<int> OnCrosshairChange;

        private Camera _camera;
        private InputService _input;
        private PlayerInventory _inventory;
        private DiContainer _container;

        private Transform _heldItem;
        private Rigidbody _heldItemRb;
        private int _originalLayer;
        private Collider _disabledCollider;

        [Inject]
        private void Construct(InputService input, DiContainer container)
        {
            _input = input;
            _container = container;
        }

        private void Start()
        {
            _camera = Camera.main;
            _inventory = _container.Resolve<PlayerInventory>();
            OnCrosshairChange?.Invoke(0);
        }

        private void Update()
        {
            if (_heldItem != null)
            {
                _heldItem.position = _holdPoint.position;
            }

            if (_heldItem != null && _input.PickupReleased)
            {
                DropItem();
                return;
            }

            if (!Physics.Raycast(_camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)), out var hit, _rayDistance, _interactableLayer))
            {
                OnCrosshairChange?.Invoke(0);
                return;
            }

            GameObject target = hit.collider.gameObject;
            OnCrosshairChange?.Invoke(1);

            if (target.CompareTag("Draggable") && _input.PickupPressed && _heldItem == null)
            {
                TryPickup(hit.collider);
                return;
            }

            if (_input.Action)
            {
                string lowerName = target.name.ToLower();

                if (lowerName.Contains("health"))
                {
                    _inventory?.AddMedkit();
                    Destroy(target);
                }
                else if (lowerName.Contains("battery"))
                {
                    _inventory?.AddBattery();
                    Destroy(target);
                }
                else if (lowerName.Contains("red key"))
                {
                    _inventory?.AddKey(KeyType.Red);
                    Destroy(target);
                }
                else if (lowerName.Contains("blue key"))
                {
                    _inventory?.AddKey(KeyType.Blue);
                    Destroy(target);
                }
                else if (lowerName.Contains("yellow key"))
                {
                    _inventory?.AddKey(KeyType.Yellow);
                    Destroy(target);
                }
                else
                {
                    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                    interactable?.Interact();
                }
            }
        }

        private void TryPickup(Collider collider)
        {
            if (collider.attachedRigidbody == null) return;

            _heldItemRb = collider.attachedRigidbody;
            _heldItem = _heldItemRb.transform;

            _heldItem.parent = _holdPoint;
            _heldItem.localRotation = Quaternion.Euler(_pickupRotationEuler);

            _heldItemRb.isKinematic = true;

            _originalLayer = _heldItem.gameObject.layer;
            int heldLayer = LayerMask.NameToLayer(_heldLayerName);
            if (heldLayer != -1)
            {
                _heldItem.gameObject.layer = heldLayer;
            }

            _disabledCollider = collider;
            _disabledCollider.enabled = false;
        }

        private void DropItem()
        {
            _heldItem.parent = null;

            // Локальная позиция holdPoint относительно игрока
            Vector3 localOffset = transform.InverseTransformPoint(_holdPoint.position);

            // Обнуляем Y и Z, чтобы предмет оказался под игроком
            Vector3 localDropOffset = localOffset;
            localDropOffset.y = 0f;
            localDropOffset.z = 0f;

            // Переводим в мировую позицию
            Vector3 worldDropPosition = transform.TransformPoint(localDropOffset);
            _heldItem.position = worldDropPosition;

            _heldItemRb.isKinematic = false;
            _heldItem.gameObject.layer = _originalLayer;

            if (_disabledCollider != null)
            {
                _disabledCollider.enabled = true;
            }

            _heldItem = null;
            _heldItemRb = null;
            _disabledCollider = null;
        }
    }
}
