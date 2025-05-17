using UnityEngine;
using Components.Interaction;

namespace Components.Props
{
    public class Trap : MonoBehaviour, IInteractable
    {
        [Header("Trap Type")]
        [SerializeField] private TrapType trapType;

        [Header("Damage Settings")]
        [SerializeField] private int damage = 100;

        [Header("Target Layers")]
        [SerializeField] private LayerMask enemyLayer;

        private bool isActive = false;
        private Collider triggerCollider;

        private void Start()
        {
            triggerCollider = GetComponent<Collider>();
            if (triggerCollider == null || !triggerCollider.isTrigger)
            {
                Debug.LogError("Trap must have a Collider set as Trigger!");
            }

            ActivateTrap();
        }

        private void ActivateTrap()
        {
            isActive = true;
            Debug.Log($"Trap [{trapType}] activated!");
        }

        private void DeactivateTrap()
        {
            isActive = false;
            Debug.Log($"Trap [{trapType}] deactivated!");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;

            int layerMask = 1 << other.gameObject.layer;
            if ((enemyLayer.value & layerMask) == 0) return;

            var victim = other.GetComponentInParent<IDamageable>();
            if (victim != null)
            {
                victim.TakeDamage(damage);
                Debug.Log($"Trap [{trapType}] dealt {damage} to {other.name}");
                DeactivateTrap();
            }
        }

        public void Interact()
        {
            isActive = false;
            Debug.Log($"Trap [{trapType}] manually deactivated via interact.");
        }
    }
}
