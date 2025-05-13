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
        [SerializeField] private Sprite[] _crosshairStates; // [0] - default, [1] - interactable

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
            if (_crosshairStates != null && _crosshairStates.Length > index && _crosshairImage != null)
            {
                _crosshairImage.sprite = _crosshairStates[index];
            }
        }
    }
}
