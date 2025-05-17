using Components.Player;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace Components.Props
{
    public class SimplePitTrap : MonoBehaviour
    {
        [SerializeField] private int damage = 100;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private LayerMask enemyLayer;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("[PitTrap] Trigger entered by: " + other.name);

            int layerMask = 1 << other.gameObject.layer;

            if ((playerLayer.value & layerMask) != 0)
            {
                Debug.Log("[PitTrap] Player layer hit");

                var collider = other.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = false;
                    Debug.Log("[PitTrap] Player Collider disabled: " + collider.name);
                }

                StartCoroutine(DelayedDamage(other.gameObject, 1.5f));
            }
            else if ((enemyLayer.value & layerMask) != 0)
            {
                Debug.Log("[PitTrap] Enemy layer hit");

                if (other.TryGetComponent<Enemies.BasicEnemy>(out var basicEnemy))
                {
                    basicEnemy.enabled = false;
                    Debug.Log("[PitTrap] BasicEnemy script disabled.");
                }

                if (other.TryGetComponent<NavMeshAgent>(out var agent))
                {
                    agent.enabled = false;
                    Debug.Log("[PitTrap] Enemy NavMeshAgent disabled.");
                }
                if (other.TryGetComponent<Collider>(out var collider))
                {
                    collider.enabled = false;
                    Debug.Log("[PitTrap] Player Collider disabled: " + collider.name);
                }

                var rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    Debug.Log("[PitTrap] Player Collider disabled: " + collider.name);
                }

                if (other.TryGetComponent<Animator>(out var animator))
                {
                    animator.enabled = false;
                }

                StartCoroutine(DelayedDamage(other.gameObject, 0.5f));
            }


        }

        private IEnumerator DelayedDamage(GameObject target, float time)
        {
            yield return new WaitForSeconds(time);
            if (target.TryGetComponent<IDamageable>(out var victim))
            {
                victim.TakeDamage(damage);
                Debug.Log("[PitTrap] Damage applied after delay.");
            }
            else
            {
                Debug.LogWarning("[PitTrap] No IDamageable found after delay.");
            }
        }
    }
}
