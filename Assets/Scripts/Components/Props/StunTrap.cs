using UnityEngine;
using System.Collections;
using Components.Interaction;

namespace Components.Props
{
    public class StunTrap : MonoBehaviour, IInteractable
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip stunClip;
        [SerializeField] private AudioClip loopClip;

        [Header("Stun Settings")]
        [SerializeField] private float stunDuration = 2f;
        [SerializeField] private LayerMask targetLayer;

        [Header("Visuals")]
        [SerializeField] private GameObject activeIndicator;
        [SerializeField] private GameObject triggerEffectObject;

        private bool isActive = true;
        private Collider triggerCollider;

        private void Start()
        {
            triggerCollider = GetComponent<Collider>();
            if (!triggerCollider || !triggerCollider.isTrigger)
                Debug.LogError("StunTrap must have a Collider set as Trigger!");

            if (!audioSource)
                Debug.LogError("StunTrap requires an AudioSource component");

            EnableTrap();
        }

        private void EnableTrap()
        {
            isActive = true;
            triggerCollider.enabled = true;

            if (loopClip)
            {
                audioSource.clip = loopClip;
                audioSource.loop = true;
                audioSource.Play();
            }

            if (activeIndicator)
                activeIndicator.SetActive(true);

            if (triggerEffectObject)
                triggerEffectObject.SetActive(false);
        }

        private void DisableTrap()
        {
            isActive = false;
            triggerCollider.enabled = false;

            if (audioSource.isPlaying)
                audioSource.Stop();

            if (activeIndicator)
                activeIndicator.SetActive(false);

            if (triggerEffectObject)
                triggerEffectObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;
            if ((targetLayer.value & (1 << other.gameObject.layer)) == 0) return;

            if (other.TryGetComponent<IStunnable>(out var stunnable))
            {
                stunnable.Stun(stunDuration);

                if (stunClip)
                {
                    audioSource.PlayOneShot(stunClip);
                }

                if (triggerEffectObject)
                {
                    triggerEffectObject.SetActive(true);
                    StartCoroutine(DisableEffectAfterStun());
                }
            }
        }

        private IEnumerator DisableEffectAfterStun()
        {
            yield return new WaitForSeconds(stunDuration);
            if (triggerEffectObject)
                triggerEffectObject.SetActive(false);
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
