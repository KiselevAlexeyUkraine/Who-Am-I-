using UnityEngine;
using Components.Interaction;
using DG.Tweening;

namespace Components.Props
{
    public class EasterEgg : MonoBehaviour, IInteractable
    {
        [Header("Animation Settings")]
        [SerializeField] private float _rotateSpeed = 90f;
        [SerializeField] private float _moveAmount = 0.5f;
        [SerializeField] private float _moveDuration = 1f;

        [Header("Visual Settings")]
        [SerializeField] private Renderer _glowRenderer;
        [SerializeField] private Color _glowColor = Color.yellow;

        private AudioSource _audioSource;
        private bool _activated = false;
        private Vector3 _initialPosition;
        private Tween _rotationTween;

        private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _initialPosition = transform.position;
            EnableGlow(false);
        }

        private void Update()
        {
            if (_activated && !_audioSource.isPlaying)
            {
                StopEffects();
            }
        }

        public void Interact()
        {
            if (_activated || _audioSource.isPlaying) return;

            _activated = true;
            _audioSource?.Play();
            EnableGlow(true);

            // Вращение по локальной оси Y
            _rotationTween = transform.DORotate(
                     new Vector3(0f, 360f, 0f),
                     2f,
                     RotateMode.WorldAxisAdd) // <- глобальная ось Y
                 .SetEase(Ease.Linear)
                 .SetLoops(-1);

            // Подпрыгивание
            transform.DOMoveY(_initialPosition.y + _moveAmount, _moveDuration / 2f)
                     .SetLoops(2, LoopType.Yoyo)
                     .SetEase(Ease.InOutSine);
        }

        private void StopEffects()
        {
            _rotationTween?.Kill();
            EnableGlow(false);
        }

        private void EnableGlow(bool enable)
        {
            if (_glowRenderer != null)
            {
                Material mat = _glowRenderer.material;

                if (enable)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor(EmissionColorID, _glowColor);
                }
                else
                {
                    mat.SetColor(EmissionColorID, Color.black);
                }
            }
        }
    }
}
