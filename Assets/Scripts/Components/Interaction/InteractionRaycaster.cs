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

        public static event System.Action<int> OnCrosshairChange;

        private Camera _camera;
        private InputService _input;
        private PlayerInventory _inventory;
        private DiContainer _container;

        [Inject]
        private void Construct(InputService input)
        {
            _input = input;
        }

        [Inject]
        private void Construct(DiContainer container)
        {
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
            if (!Physics.Raycast(_camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)), out var hit, _rayDistance, _interactableLayer))
            {
                OnCrosshairChange?.Invoke(0);
                return;
            }

            var target = hit.collider.gameObject;

            if (target.CompareTag("Item"))
            {
                OnCrosshairChange?.Invoke(1);

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
            else
            {
                OnCrosshairChange?.Invoke(0);
            }
        }
    }
}
