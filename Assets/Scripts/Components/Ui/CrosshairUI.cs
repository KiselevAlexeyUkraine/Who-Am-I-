using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Components.Interaction;

namespace Components.Ui
{
    public class CrosshairUI : MonoBehaviour
    {
        [Header("Crosshair UI")]
        [SerializeField] private Image _crosshairNormal;
        [SerializeField] private Image _crosshairInteract;

        private void OnEnable()
        {
            InteractionRaycaster.OnCrosshairChange += UpdateCrosshair;
            UpdateCrosshair(0);
        }

        private void OnDisable()
        {
            InteractionRaycaster.OnCrosshairChange -= UpdateCrosshair;
        }

        private void UpdateCrosshair(int index)
        {
            bool isInteract = index == 1;

            _crosshairNormal.gameObject.SetActive(!isInteract);
            _crosshairInteract.gameObject.SetActive(isInteract);
        }
    }
}
