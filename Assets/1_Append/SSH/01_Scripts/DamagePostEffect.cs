using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class DamagePostEffect : MonoBehaviour
{
    // 상수
    private const float MAX_INTENSITY = 0.5f; // 최대 강도
    private const float FADE_IN_TIME = 0.1f;     // 깜빡일 때 올라가는 시간
    private const float FADE_OUT_TIME = 0.5f;    // 서서히 사라지는 시간

    // private 필드(인스펙터 노출)
    [SerializeField] private Volume postProcessVolume;

    // private 필드
    private Vignette vignette;

    // 싱글턴
    private static DamagePostEffect instance;
    public static DamagePostEffect Instance => instance;

    // 유니티 콜백
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        if (postProcessVolume.profile.TryGet(out vignette))
        {
            vignette.intensity.value = 0f; // 시작 시 초기화
        }
        else
        {
            Debug.Log("Vignette 없음");
        }
    }

    public void PlayDamageEffect()
    {
        if (vignette == null)
        {
            Debug.Log("Vignette 없음");

            return;
        }

        DOTween.Kill(this); // 중복 방지
        vignette.intensity.value = 0f;

        // 피격 연출
        DOTween.To(() => vignette.intensity.value,
                   x => vignette.intensity.value = x,
                   MAX_INTENSITY,
                   FADE_IN_TIME)
               .OnComplete(() =>
               {
                   DOTween.To(() => vignette.intensity.value,
                              x => vignette.intensity.value = x,
                              0f,
                              FADE_OUT_TIME);
               });
    }
}
