using UnityEngine;
using System.Collections;
using Components.Interaction;

namespace Components.Props
{
    public class GasTrap : MonoBehaviour, IInteractable
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip gasTriggerClip;
        [SerializeField] private AudioClip gasLoopClip;

        [Header("Trap Settings")]
        [SerializeField] private int damage = 10;
        [SerializeField] private float slowMultiplier = 0.5f;
        [SerializeField] private float slowDuration = 3f;
        [SerializeField] private LayerMask targetLayer;

        [Header("Visuals")]
        [SerializeField] private GameObject activeIndicator;
        [SerializeField] private GameObject gasEffectObject;

        private bool isActive = true;
        private Collider triggerCollider;

        private void Start()
        {
            triggerCollider = GetComponent<Collider>();
            if (!triggerCollider || !triggerCollider.isTrigger)
                Debug.LogError("GasTrap must have a Collider set as Trigger!");

            if (!audioSource)
                Debug.LogError("GasTrap requires an AudioSource component");

            EnableTrap();
        }

        private void EnableTrap()
        {
            isActive = true;

            if (triggerCollider)
                triggerCollider.enabled = true;

            if (gasLoopClip)
            {
                audioSource.clip = gasLoopClip;
                audioSource.loop = true;
                audioSource.Play();
            }

            if (activeIndicator)
                activeIndicator.SetActive(true);

            if (gasEffectObject)
                gasEffectObject.SetActive(false);
        }

        private void DisableTrap()
        {
            isActive = false;

            if (triggerCollider)
                triggerCollider.enabled = false;

            if (audioSource.isPlaying)
                audioSource.Stop();

            if (activeIndicator)
                activeIndicator.SetActive(false);

            if (gasEffectObject)
                gasEffectObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;
            if ((targetLayer.value & (1 << other.gameObject.layer)) == 0) return;

            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
            }

            if (other.TryGetComponent<ISlowable>(out var slowable))
            {
                slowable.ApplySlow(slowMultiplier, slowDuration);
            }

            if (gasTriggerClip)
            {
                audioSource.PlayOneShot(gasTriggerClip);
            }

            if (gasEffectObject)
            {
                gasEffectObject.SetActive(true);
                StartCoroutine(DisableEffectAfterDelay(slowDuration));
            }
        }

        private IEnumerator DisableEffectAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (gasEffectObject)
                gasEffectObject.SetActive(false);
        }

        public void Interact()
        {
            if (isActive)
                DisableTrap();
            else
                EnableTrap();
        }
    }
}