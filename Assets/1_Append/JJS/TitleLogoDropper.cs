using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class TitleLogoDropper : MonoBehaviour
{
    [Header("Target (생략 시 본인)")]
    public RectTransform target;

    [Header("Drop")]
    public float dropHeight = 600f;
    public float dropDuration = 0.45f;
    public AnimationCurve dropEase; // null이면 OutCubic

    [Header("Impact & Settle")]
    public float impactScaleX = 1.14f;
    public float impactScaleY = 0.86f;
    public float settleDuration = 0.28f;
    public AnimationCurve settleEase; // null이면 OutBack

    [Header("Mid Effect")]
    public GameObject midEffect;                 // 비활성 파티클 오브젝트
    [Range(0f, 1f)] public float midEffectTime = 0.45f;

    [Header("Playback")]
    public bool playOnEnable = true;
    public bool waitOneFrameBeforePlay = true;   // 레이아웃/캔버스 배치 끝난 뒤 재생
    public bool useUnscaledTime = true;          // Pause 중에도 재생

    Vector2 basePos;
    Sequence seq;

    void Reset() { target = GetComponent<RectTransform>(); }

    void OnEnable()
    {
        if (playOnEnable) StartCoroutine(PlayRoutine());
    }

    public void PlayNow()
    {
        if (seq != null) seq.Kill();
        PlayInternal();
    }

    IEnumerator PlayRoutine()
    {
        if (waitOneFrameBeforePlay) yield return null; // 한 프레임 대기(레이아웃 완료)
        Canvas.ForceUpdateCanvases();
        PlayInternal();
    }

    void PlayInternal()
    {
        if (target == null) target = GetComponent<RectTransform>();

        // 시작 상태
        basePos = target.anchoredPosition;
        target.anchoredPosition = basePos + new Vector2(0f, dropHeight);
        target.localScale = Vector3.one;
        if (midEffect) midEffect.SetActive(false);

        seq = DOTween.Sequence().SetUpdate(useUnscaledTime);

        // 1) 낙하
        var fall = target.DOAnchorPos(basePos, dropDuration).SetUpdate(useUnscaledTime);
        if (dropEase != null) fall.SetEase(dropEase);
        else fall.SetEase(Ease.OutCubic);
        seq.Append(fall);

        // 1-1) 중간 이펙트
        float midAt = Mathf.Clamp01(midEffectTime) * dropDuration;
        seq.InsertCallback(midAt, () =>
        {
            if (midEffect) ActivateEffectUnscaled(midEffect);
        });

        // 2) 착지 스쿼시
        seq.AppendCallback(() =>
        {
            target.localScale = new Vector3(impactScaleX, impactScaleY, 1f);
        });

        // 3) 바운스 복귀
        var settle = target.DOScale(Vector3.one, settleDuration).SetUpdate(useUnscaledTime);
        if (settleEase != null) settle.SetEase(settleEase);
        else settle.SetEase(Ease.OutBack);
        seq.Append(settle);

      
    }

    void OnDisable()
    {
        if (seq != null) seq.Kill();
    }

    // 파티클을 timeScale=0에서도 바로 재생
    void ActivateEffectUnscaled(GameObject fx)
    {
        if (!fx) return;

        // 켜두기(이미 켜져 있어도 OK)
        if (!fx.activeSelf) fx.SetActive(true);

        var systems = fx.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in systems)
        {
            if (ps == null) continue;
            var main = ps.main;
            main.useUnscaledTime = true;                          // ★ 핵심
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play(true);
        }
    }
}
