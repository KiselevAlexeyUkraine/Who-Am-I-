using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

[RequireComponent(typeof(Light))]
public class LightBlinker : MonoBehaviour
{
    [Tooltip("Интенсивность света при включении")]
    [SerializeField]
    private float intensity = 1f;

    [Tooltip("Материал, у которого будет меняться цвет")]
    [SerializeField]
    private Renderer targetRenderer;

    [Tooltip("Список easing-кривых для случайного выбора")]
    [SerializeField]
    private List<Ease> possibleEases = new List<Ease> {
        Ease.Linear, Ease.InOutSine, Ease.InOutQuad, Ease.InOutCubic,
        Ease.InOutBack, Ease.InOutElastic, Ease.InOutExpo
    };

    [Tooltip("Таймер. Моргание запускается, когда он == 0")]
    [SerializeField]
    private float timer = 5f;

    private Light _light;
    private bool _started = false;
    private Material _material;

    private void Start()
    {
        _light = GetComponent<Light>();

        if (targetRenderer != null)
        {
            _material = targetRenderer.material;
        }
    }

    private void Update()
    {
        if (!_started && timer <= 0)
        {
            _started = true;
            StartBlinking();
        }

        if (timer > 0)
        {
            timer -= Time.unscaledDeltaTime;
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

        seq.Append(DOTween.To(() => _light.intensity, x => {
            _light.intensity = x;
            UpdateMaterialColor(x);
        }, 0f, interval).SetEase(randomEase));

        seq.Append(DOTween.To(() => _light.intensity, x => {
            _light.intensity = x;
            UpdateMaterialColor(x);
        }, intensity, interval).SetEase(randomEase));

        seq.OnComplete(BlinkRandomly);
    }

    private void UpdateMaterialColor(float currentIntensity)
    {
        if (_material != null)
        {
            Color color = Color.Lerp(Color.black, Color.white, currentIntensity / intensity);
            _material.color = color;
        }
    }
}