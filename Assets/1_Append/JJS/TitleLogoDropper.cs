using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class TitleLogoDropper : MonoBehaviour
{
    [Header("타이틀 배경이미지")]
    public GameObject titleObject;
    public GameObject GradeLogo;

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

    [Header("Buttons Enter (로고 완료 후)")]
    [Tooltip("등장시킬 버튼 RectTransform 2개(또는 그 이상)를 넣으세요(권장 2개).")]
    public RectTransform[] buttons;
    [Tooltip("로고 착지-바운스가 끝난 뒤 버튼 등장까지 대기 시간")]
    public float btnStartDelay = 0.05f;
    [Tooltip("버튼 간 순차 시간 간격(스태거)")]
    public float btnStagger = 0.06f;
    [Tooltip("버튼이 아래쪽에서 이만큼 위로 이동하며 등장(Y 오프셋)")]
    public float btnFromYOffset = 140f;
    [Tooltip("버튼이 올라오는 시간")]
    public float btnMoveDuration = 0.35f;
    public AnimationCurve btnEase; // null이면 OutCubic
    [Tooltip("등장 시 페이드인 적용 여부")]
    public bool btnUseFade = true;
    [Tooltip("버튼 페이드인 시간")]
    public float btnFadeDuration = 0.20f;

    [Header("Playback")]
    public bool playOnEnable = true;
    public bool waitOneFrameBeforePlay = true;   // 레이아웃/캔버스 배치 끝난 뒤 재생
    public bool useUnscaledTime = true;          // Pause 중에도 재생

    // 내부
    Vector2 logoBasePos;
    Vector2[] btnBasePos;
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

        // --- 로고 시작 상태 ---
        logoBasePos = target.anchoredPosition;
        target.anchoredPosition = logoBasePos + new Vector2(0f, dropHeight);
        target.localScale = Vector3.one;

        GradeLogo.SetActive(true);

        var cg = GradeLogo.GetComponent<CanvasGroup>();
        if (cg == null) cg = GradeLogo.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        // 2초 유지 후 0.5초 동안 페이드아웃
        DOVirtual.DelayedCall(2f, () =>
        {
            cg.DOFade(0f, 0.5f).SetUpdate(useUnscaledTime).OnComplete(() =>
            {
                GradeLogo.SetActive(false);
                cg.alpha = 1f; // 다음에 다시 쓸 때 대비 초기화
            });
        }).SetUpdate(useUnscaledTime);

        // --- 버튼 시작 상태 ---
        InitButtonsForEnter();

        // --- midEffect 꺼두기 ---
        if (midEffect) midEffect.SetActive(false);

        seq = DOTween.Sequence().SetUpdate(useUnscaledTime);

        // 1) 낙하
        var fall = target.DOAnchorPos(logoBasePos, dropDuration).SetUpdate(useUnscaledTime);
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
            if (titleObject != null)
            {
                titleObject.transform.DOShakePosition(
                    1f,                          // duration
                    new Vector3(3f, 3f, 0f),    // strength
                    10,                          // vibrato
                    45f                          // randomness
                ).SetUpdate(useUnscaledTime);
            }
            target.localScale = new Vector3(impactScaleX, impactScaleY, 1f);
        });

        // 3) 바운스 복귀
        var settle = target.DOScale(Vector3.one, settleDuration).SetUpdate(useUnscaledTime);
        if (settleEase != null) settle.SetEase(settleEase);
        else settle.SetEase(Ease.OutBack);
        seq.Append(settle);

        // 4) 버튼 등장
        seq.AppendInterval(btnStartDelay);
        seq.Append(PlayButtonsEnter()); // 버튼 시퀀스를 통째로 Append
    }

    void OnDisable()
    {
        if (seq != null) seq.Kill();
    }

    // --- 버튼 초기화: 위치/알파/인터랙션 ---
    void InitButtonsForEnter()
    {
        if (buttons == null || buttons.Length == 0) return;

        if (btnBasePos == null || btnBasePos.Length != buttons.Length)
            btnBasePos = new Vector2[buttons.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            var rt = buttons[i];
            if (rt == null) continue;

            btnBasePos[i] = rt.anchoredPosition;

            // 아래에서 올라오게 시작 위치 세팅
            rt.anchoredPosition = btnBasePos[i] + new Vector2(0f, -btnFromYOffset);

            // 페이드/입력 막기
            var cg = GetOrAddCanvasGroup(rt.gameObject);
            cg.alpha = btnUseFade ? 0f : 1f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }

    // --- 버튼 등장 시퀀스 생성 ---
    Sequence PlayButtonsEnter()
    {
        

        var s = DOTween.Sequence().SetUpdate(useUnscaledTime);

        if (buttons == null || buttons.Length == 0) return s;

        for (int i = 0; i < buttons.Length; i++)
        {
            var rt = buttons[i];
            if (rt == null) continue;

            float startAt = i * Mathf.Max(0f, btnStagger);

            // 이동 트윈
            var move = rt.DOAnchorPos(btnBasePos[i], btnMoveDuration)
                         .SetUpdate(useUnscaledTime);

            if (btnEase != null) move.SetEase(btnEase);
            else move.SetEase(Ease.OutCubic);

            s.Insert(startAt, move);

            // 페이드 트윈
            if (btnUseFade)
            {
                var cg = GetOrAddCanvasGroup(rt.gameObject);
                var fade = cg.DOFade(1f, btnFadeDuration).SetUpdate(useUnscaledTime);
                s.Insert(startAt, fade);
            }

            // 끝나면 입력 가능
            s.InsertCallback(startAt + Mathf.Max(btnMoveDuration, btnUseFade ? btnFadeDuration : 0f), () =>
            {
                var cg = GetOrAddCanvasGroup(rt.gameObject);
                cg.interactable = true;
                cg.blocksRaycasts = true;
            });
        }

        return s;
    }

    CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    // 파티클을 timeScale=0에서도 바로 재생
    void ActivateEffectUnscaled(GameObject fx)
    {
        
        if (!fx) return;

        if (!fx.activeSelf) fx.SetActive(true);

        var systems = fx.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in systems)
        {
            if (ps == null) continue;
            var main = ps.main;
            main.useUnscaledTime = true; // ★ 핵심
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play(true);
        }
    }
}
