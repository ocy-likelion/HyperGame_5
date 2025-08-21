using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleSplashFX : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private Vector2 startScale = new Vector2(0.3f, 0.2f);
    [SerializeField] private Vector2 endScaleMin = new Vector2(1.2f, 0.8f);
    [SerializeField] private Vector2 endScaleMax = new Vector2(2.0f, 1.2f);
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Color")]
    [SerializeField] private Color startColor = new Color(1f, 0.5f, 0.2f, 0.85f); // 주황빛
    [SerializeField] private Color endColor = new Color(1f, 0.5f, 0.2f, 0.0f);

    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _sr.sortingOrder = 999; // 항상 위에 보이도록(필요시 조정)
        transform.localScale = startScale;
        _sr.color = startColor;
    }

    /// <summary>
    /// intensity: 0(작게) ~ 1(크게)
    /// </summary>
    public void Play(float intensity = 0.5f)
    {
        StopAllCoroutines();
        StartCoroutine(Co_Play(intensity));
    }

    private IEnumerator Co_Play(float intensity)
    {
        intensity = Mathf.Clamp01(intensity);
        Vector2 endScale = Vector2.Lerp(endScaleMin, endScaleMax, intensity);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = ease.Evaluate(Mathf.Clamp01(t / duration));

            transform.localScale = Vector2.Lerp(startScale, endScale, k);
            _sr.color = Color.Lerp(startColor, endColor, k);

            yield return null;
        }

        Destroy(gameObject);
    }
}