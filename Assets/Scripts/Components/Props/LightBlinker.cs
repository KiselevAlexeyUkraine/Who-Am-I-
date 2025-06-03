using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

[RequireComponent(typeof(Light))]
public class LightBlinker : MonoBehaviour
{
    [Tooltip("Интенсивность света при включении")][SerializeField] private float intensity = 1f;

    [Tooltip("Материал, у которого будет меняться эмиссия")]
    [SerializeField] private Renderer targetRenderer;

    [Tooltip("Цвет свечения (эмиссии)")]
    [SerializeField] private Color targetColor = Color.white;

    [Tooltip("Список easing-кривых для случайного выбора")]
    [SerializeField]
    private List<Ease> possibleEases = new List<Ease> {
    Ease.Linear, Ease.InOutSine, Ease.InOutQuad, Ease.InOutCubic,
    Ease.InOutBack, Ease.InOutElastic, Ease.InOutExpo
};

    private Light _light;
    private Material _material;

    private void Start()
    {
        _light = GetComponent<Light>();

        if (targetRenderer != null)
        {
            _material = targetRenderer.material;
            ApplyInitialColor();
            StartBlinking();
        }
        else
        {
            Debug.LogWarning("LightBlinker: targetRenderer не назначен!");
        }
    }

    private void StartBlinking()
    {
        BlinkRandomly();
    }

    private void BlinkRandomly()
    {
        float interval = Random.Range(0.2f, 0.7f);
        Ease randomEase = possibleEases[Random.Range(0, possibleEases.Count)];

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        seq.Append(DOTween.To(
            () => _light != null ? _light.intensity : 0f,
            x => {
                if (_light != null) _light.intensity = x;
                UpdateMaterial(x);
            },
            0f,
            interval
        ).SetEase(randomEase));

        seq.Append(DOTween.To(
            () => _light != null ? _light.intensity : 0f,
            x => {
                if (_light != null) _light.intensity = x;
                UpdateMaterial(x);
            },
            intensity,
            interval
        ).SetEase(randomEase));

        seq.OnComplete(BlinkRandomly);
    }

    private void ApplyInitialColor()
    {
        if (_material != null && _material.HasProperty("_EmissionColor"))
        {
            _material.SetColor("_EmissionColor", Color.black);
            _material.EnableKeyword("_EMISSION");
        }
    }

    private void UpdateMaterial(float currentIntensity)
    {
        if (_material == null || !_material.HasProperty("_EmissionColor")) return;

        Color blended = Color.Lerp(Color.black, targetColor, currentIntensity / intensity);
        _material.SetColor("_EmissionColor", blended);
        _material.EnableKeyword("_EMISSION");
    }

}