using UnityEngine;
using Services;
using Zenject;

namespace Components.Interaction
{
    public class InteractionRaycaster : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private float _rayDistance = 3f;
        [SerializeField] private LayerMask _interactableLayer;

        public static event System.Action<int> OnCrosshairChange; // 0 - default, 1 - interactable

        private Camera _camera;
        private InputService _input;

        [Inject]
        private void Construct(InputService input)
        {
            _input = input;
        }

        private void Start()
        {
            _camera = Camera.main;
            OnCrosshairChange?.Invoke(0);
        }

        private void Update()
        {
            if (!Physics.Raycast(_camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)), out var hit, _rayDistance, _interactableLayer))
            {
                OnCrosshairChange?.Invoke(0);
                return;
            }

            if (hit.collider.CompareTag("Item"))
            {
                OnCrosshairChange?.Invoke(1);

                if (_input.Action)
                {
                    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                    interactable?.Interact();
                }
            }
            else
            {
                OnCrosshairChange?.Invoke(0);
            }
        }
    }
}
