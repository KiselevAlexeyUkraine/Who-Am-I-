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
        [SerializeField] private LayerMask _playerLayer;

        [Header("Interaction Filtering")]
        [SerializeField] private LayerMask _ignoreInteractionLayer;

        [Header("Pickup Settings")]
        [SerializeField] private Transform _holdPoint;
        [SerializeField] private string _heldLayerName = "Held";
        [SerializeField] private Vector3 _pickupRotationEuler = Vector3.zero;
        [SerializeField] private Vector3 _dropRotationEuler = Vector3.zero;

        [Header("Audio Settings")]
        [SerializeField] private AudioClip _pickupClip;
        [SerializeField] private AudioClip _dropClip;

        public static event System.Action<int> OnCrosshairChange;

        private Camera _camera;
        private InputService _input;
        private PlayerInventory _inventory;
        private DiContainer _container;
        private AudioService _audioService;

        private Transform _heldItem;
        private Rigidbody _heldItemRb;
        private int _originalLayer;
        private Collider _disabledCollider;

        [Inject]
        private void Construct(InputService input, DiContainer container, AudioService audioService)
        {
            _input = input;
            _container = container;
            _audioService = audioService;
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
                _heldItem.position = _holdPoint.position;

            if (_heldItem != null && _input.PickupReleased)
            {
                DropItem();
                return;
            }

            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit[] hits = Physics.RaycastAll(ray, _rayDistance, _interactableLayer | _ignoreInteractionLayer | _playerLayer, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                GameObject target = hit.collider.gameObject;
                int targetLayer = target.layer;

                if (((1 << targetLayer) & _ignoreInteractionLayer) != 0)
                {
                    OnCrosshairChange?.Invoke(0);
                    return;
                }

                if (((1 << targetLayer) & _playerLayer) != 0)
                    continue;

                OnCrosshairChange?.Invoke(1);

                if (target.CompareTag("Draggable") && _input.PickupPressed && _heldItem == null)
                {
                    TryPickup(hit.collider);
                    return;
                }

                if (_input.Action)
                {
                    PickupItemType itemType = DetectPickupType(target.name);

                    switch (itemType)
                    {
                        case PickupItemType.Health:
                            _inventory?.AddMedkit();
                            Destroy(target);
                            break;
                        case PickupItemType.Battery:
                            _inventory?.AddBattery();
                            Destroy(target);
                            break;
                        case PickupItemType.RedKey:
                            _inventory?.AddKey(KeyType.Red);
                            Destroy(target);
                            break;
                        case PickupItemType.BlueKey:
                            _inventory?.AddKey(KeyType.Blue);
                            Destroy(target);
                            break;
                        case PickupItemType.YellowKey:
                            _inventory?.AddKey(KeyType.Yellow);
                            Destroy(target);
                            break;
                        case PickupItemType.Note:
                            Note note = target.GetComponent<Note>();
                            if (note != null && !note.IsCollected)
                            {
                                note.Collect();
                                _inventory?.AddNote(note.NoteId.ToString());
                            }
                            break;
                        case PickupItemType.Unknown:
                            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                            interactable?.Interact();
                            return;
                    }

                    _audioService?.PlayOneShot(_pickupClip);
                }

                return;
            }

            OnCrosshairChange?.Invoke(0);
        }

        private PickupItemType DetectPickupType(string name)
        {
            string lowerName = name.ToLower();

            if (lowerName.Contains("health")) return PickupItemType.Health;
            if (lowerName.Contains("battery")) return PickupItemType.Battery;
            if (lowerName.Contains("red key")) return PickupItemType.RedKey;
            if (lowerName.Contains("blue key")) return PickupItemType.BlueKey;
            if (lowerName.Contains("yellow key")) return PickupItemType.YellowKey;
            if (lowerName.Contains("note") || lowerName.Contains("paper")) return PickupItemType.Note;

            return PickupItemType.Unknown;
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
                _heldItem.gameObject.layer = heldLayer;

            _disabledCollider = collider;
            _disabledCollider.enabled = false;

            _audioService?.PlayOneShot(_pickupClip);
        }

        private void DropItem()
        {
            _heldItem.parent = null;

            Vector3 localOffset = transform.InverseTransformPoint(_holdPoint.position);
            Vector3 localDropOffset = localOffset;
            localDropOffset.y = 0f;
            localDropOffset.z = 0f;
            Vector3 worldDropPosition = transform.TransformPoint(localDropOffset);
            _heldItem.position = worldDropPosition;
            _heldItem.rotation = Quaternion.Euler(_dropRotationEuler);

            _heldItemRb.isKinematic = false;
            _heldItem.gameObject.layer = _originalLayer;

            if (_disabledCollider != null)
                _disabledCollider.enabled = true;

            _heldItem = null;
            _heldItemRb = null;
            _disabledCollider = null;

            _audioService?.PlayOneShot(_dropClip);
        }

        private enum PickupItemType
        {
            Health,
            Battery,
            RedKey,
            BlueKey,
            YellowKey,
            Note,
            Unknown
        }
    }
}
