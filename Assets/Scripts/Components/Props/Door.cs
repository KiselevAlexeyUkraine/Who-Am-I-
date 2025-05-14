using UnityEngine;
using Components.Interaction;

namespace Components.Props
{
    public class Door : MonoBehaviour, IInteractable
    {
        private Animator animator;
        private AudioSource audioSource;
        private bool isOpen;
        private Collider boxCollider;

        private void Start()
        {
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            boxCollider = GetComponent<Collider>();
        }

        public void Interact()
        {
            if (!isOpen)
            {
                animator.Play("Open");
                audioSource.Play();
                isOpen = !isOpen;
                boxCollider.enabled = false;
            }
        }
    }
}