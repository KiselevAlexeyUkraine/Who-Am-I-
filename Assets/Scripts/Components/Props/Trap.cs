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

        private bool isActive = false;
        private Collider triggerCollider;

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

            Vector3 initialPos = movingPart.localPosition;
            initialPos.y = inactiveY;
            movingPart.localPosition = initialPos;

            ActivateTrap();
        }

        private void ActivateTrap()
        {
            movingPart.DOLocalMoveY(activeY, moveDuration)
                      .SetEase(moveEase)
                      .SetUpdate(true)
                      .OnComplete(() =>
                      {
                          isActive = true;
                          Debug.Log($"Trap [{trapType}] activated at Y:{activeY}");
                      });
        }

        private void DeactivateTrap()
        {
            movingPart.DOLocalMoveY(inactiveY, moveDuration)
                      .SetEase(moveEase)
                      .SetUpdate(true)
                      .OnComplete(() =>
                      {
                          isActive = false;
                          Debug.Log($"Trap [{trapType}] deactivated at Y:{inactiveY}");
                      });
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
            if (isActive)
                DeactivateTrap();
            else
                ActivateTrap();
        }
    }
}
