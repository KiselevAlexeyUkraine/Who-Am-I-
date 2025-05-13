using UnityEngine;
using Components.Interaction;

namespace Components.Props
{
    public class Door : MonoBehaviour, IInteractable
    {
        private Animator animator;
        private AudioSource audioSource;
        private bool isOpen;

        private void Start()
        {
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
        }

        public void Interact()
        {
            if (!isOpen)
            {
                animator.Play("Open");
            }
            else
            {
                animator.Play("Close");
            }

            audioSource.Play();
            isOpen = !isOpen;
        }
    }
}