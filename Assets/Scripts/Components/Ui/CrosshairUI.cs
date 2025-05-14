using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Components.Interaction;

namespace Components.Ui
{
    public class CrosshairUI : MonoBehaviour
    {
        [Header("Crosshair UI")]
        [SerializeField] private Image _crosshairImage;
        [SerializeField] private Sprite[] _crosshairStates;

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
            _crosshairImage.sprite = _crosshairStates[index];
        }
    }
}
