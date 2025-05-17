using UnityEngine;

namespace Components.Props
{
    public class Trap : MonoBehaviour
    {
        [Header("Trap Timing")]
        public float activationTime = 2f;
        public float activeDuration = 5f;

        [Header("Damage Settings")]
        public int damage = 100;

        private bool isActive = false;
        private Collider triggerCollider;

        private void Start()
        {
            triggerCollider = GetComponent<Collider>();
            if (triggerCollider == null || !triggerCollider.isTrigger)
            {
                Debug.LogError("Trap must have a Collider set as Trigger!");
            }

            Invoke(nameof(ActivateTrap), activationTime);
        }

        private void ActivateTrap()
        {
            isActive = true;
            Debug.Log("Trap activated!");
            Invoke(nameof(DeactivateTrap), activeDuration);
        }

        private void DeactivateTrap()
        {
            isActive = false;
            Debug.Log("Trap deactivated!");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;

            IDamageable victim = other.GetComponent<IDamageable>();
            if (victim != null)
            {
                victim.TakeDamage(damage);
            }
        }
    }
}

