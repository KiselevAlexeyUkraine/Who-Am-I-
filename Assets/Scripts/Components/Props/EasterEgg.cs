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
        private Material _cachedMaterial;

        private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _initialPosition = transform.position;
            if (_glowRenderer != null)
                _cachedMaterial = _glowRenderer.sharedMaterial;
        }

        private void Start()
        {
            EnableGlow(false);
        }

        public void Interact()
        {
            if (_activated || (_audioSource != null && _audioSource.isPlaying)) return;

            _activated = true;
            _audioSource?.Play();
            EnableGlow(true);

            _rotationTween = transform.DORotate(
                    new Vector3(0f, 360f, 0f),
                    2f,
                    RotateMode.WorldAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1);

            transform.DOMoveY(_initialPosition.y + _moveAmount, _moveDuration / 2f)
                     .SetLoops(2, LoopType.Yoyo)
                     .SetEase(Ease.InOutSine);

            if (_audioSource != null)
                Invoke(nameof(StopEffects), _audioSource.clip.length);
        }

        private void StopEffects()
        {
            _rotationTween?.Kill();
            EnableGlow(false);
            _activated = false;
        }

        private void EnableGlow(bool enable)
        {
            if (_cachedMaterial == null) return;

            if (enable)
            {
                _cachedMaterial.EnableKeyword("_EMISSION");
                _cachedMaterial.SetColor(EmissionColorID, _glowColor);
            }
            else
            {
                _cachedMaterial.SetColor(EmissionColorID, Color.black);
            }
        }
    }
}