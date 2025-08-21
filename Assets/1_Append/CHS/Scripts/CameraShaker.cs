using UnityEngine;
using System.Collections;

public class CameraShaker : MonoBehaviour
{
    Vector3 originPos;

    void Awake() => originPos = transform.localPosition;

    public void Shake(float intensity = 0.2f, float duration = 0.3f)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(intensity, duration));
    }

    IEnumerator ShakeRoutine(float intensity, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            transform.localPosition = originPos + (Vector3)Random.insideUnitCircle * intensity;
            t += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originPos;
    }
}