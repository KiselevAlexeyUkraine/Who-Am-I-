using UnityEngine;
using Zenject;
using Components.Ui.Pages;

namespace Components.Triggers
{
    [RequireComponent(typeof(Collider))]
    public class TutorialTrigger : MonoBehaviour
    {
        private PageSwitcher _pageSwitcher;
        private bool _triggered = false;

        [Inject]
        private void Construct(PageSwitcher pageSwitcher)
        {
            _pageSwitcher = pageSwitcher;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered) return;

            if (other.CompareTag("Player"))
            {
                _triggered = true;
                _pageSwitcher.Open(PageName.Tutorial).Forget();
            }
        }
    }
}
