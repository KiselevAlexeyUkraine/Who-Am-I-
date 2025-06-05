using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

namespace Components.Props
{
    [RequireComponent(typeof(Light))]
    public class LightBlinker : MonoBehaviour
    {
        [SerializeField] private float intensity = 1f;
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color targetColor = Color.white;

        [SerializeField]
        private List<Ease> possibleEases = new()
        {
            Ease.Linear, Ease.InOutSine, Ease.InOutQuad, Ease.InOutCubic,
            Ease.InOutBack, Ease.InOutElastic, Ease.InOutExpo
        };

        private Light _light;
        private Material _material;
        private Sequence _blinkSequence;

        private void Awake()
        {
            _light = GetComponent<Light>();

            if (targetRenderer != null)
                _material = targetRenderer.material;
        }

        private void Start()
        {
            if (_material == null)
            {
                Debug.LogWarning("[LightBlinker] Renderer or material is missing.");
                return;
            }

            ApplyInitialColor();
            StartBlinking();
        }

        private void StartBlinking()
        {
            BlinkOnce();
        }

        private void BlinkOnce()
        {
            if (_light == null || _material == null) return;

            float duration = Random.Range(0.2f, 0.7f);
            Ease ease = possibleEases[Random.Range(0, possibleEases.Count)];

            _blinkSequence = DOTween.Sequence().SetUpdate(true);
            _blinkSequence.Append(DOTween.To(
                () => _light.intensity,
                x => ApplyLightAndEmission(x),
                0f,
                duration
            ).SetEase(ease));

            _blinkSequence.Append(DOTween.To(
                () => _light.intensity,
                x => ApplyLightAndEmission(x),
                intensity,
                duration
            ).SetEase(ease));

            _blinkSequence.OnComplete(BlinkOnce);
        }

        private void ApplyLightAndEmission(float value)
        {
            if (_light != null)
                _light.intensity = value;

            if (_material != null && _material.HasProperty("_EmissionColor"))
            {
                Color emissive = Color.Lerp(Color.black, targetColor, value / intensity);
                _material.SetColor("_EmissionColor", emissive);
                _material.EnableKeyword("_EMISSION");
            }
        }

        private void ApplyInitialColor()
        {
            if (_material != null && _material.HasProperty("_EmissionColor"))
            {
                _material.SetColor("_EmissionColor", Color.black);
                _material.EnableKeyword("_EMISSION");
            }
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
            _blinkSequence?.Kill();
        }
    }
}