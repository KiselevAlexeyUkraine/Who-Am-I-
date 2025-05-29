using UnityEngine;
using DG.Tweening;
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

        [Header("Moving Part Settings")]
        [SerializeField] private Transform movingPart;
        [SerializeField] private float activeY = 1f;
        [SerializeField] private float inactiveY = 0f;
        [SerializeField] private float moveDuration = 0.5f;
        [SerializeField] private Ease moveEase = Ease.InOutSine;

        private Collider triggerCollider;

        private bool isActive = false;
        private bool isAnimating = false;
        private bool hasDealtDamage = false;

        private void Start()
        {
            triggerCollider = GetComponent<Collider>();
            if (triggerCollider == null || !triggerCollider.isTrigger)
            {
                Debug.LogError("Trap must have a Collider set as Trigger!");
            }

            if (movingPart == null)
            {
                Debug.LogError("Trap requires a reference to the moving part!");
                enabled = false;
                return;
            }

            ActivateTrap();
        }

        private void ActivateTrap()
        {
            if (isAnimating) return;

            isAnimating = true;
            hasDealtDamage = false;

            movingPart.DOLocalMoveY(activeY, moveDuration)
                      .SetEase(moveEase)
                      .SetUpdate(true)
                      .OnComplete(() =>
                      {
                          isAnimating = false;
                          isActive = true;
                          Debug.Log($"Trap [{trapType}] activated at Y:{activeY}");
                      });
        }

        private void DeactivateTrap()
        {
            if (isAnimating) return;

            isAnimating = true;
            isActive = false;

            movingPart.DOLocalMoveY(inactiveY, moveDuration)
                      .SetEase(moveEase)
                      .SetUpdate(true)
                      .OnComplete(() =>
                      {
                          isAnimating = false;
                          Debug.Log($"Trap [{trapType}] deactivated at Y:{inactiveY}");
                      });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActive || hasDealtDamage) return;

            int layerMask = 1 << other.gameObject.layer;
            if ((enemyLayer.value & layerMask) == 0) return;

            if (other.TryGetComponent<IDamageable>(out var victim))
            {
                victim.TakeDamage(damage);
                Debug.Log($"Trap [{trapType}] dealt {damage} to {other.name}");

                hasDealtDamage = true;
                DeactivateTrap();
            }
        }

        public void Interact()
        {
            if (isAnimating) return;

            if (isActive)
                DeactivateTrap();
            else
                ActivateTrap();
        }
    }
}
