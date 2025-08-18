using TMPro;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Ul 팝업")]
    public GameObject TutorialUI;
    public GameObject PauseUI;
    public GameObject ResultUI;

    [Header("타이머")]
    public Slider Timer;
    public Image TimerImage;
    [Range(0f, 1f)] public float shakeStartNormalized = 0.4f;
    public float minAmp = 0f;
    public float maxAmp = 18f;
    public int minVibrato = 10;
    public int maxVibrato = 40;
    public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("사운드")]
    public Image SoundButton;
    public Sprite SoundButtonOn;
    public Sprite SoundButtonOff;
    public TextMeshProUGUI SoundText;

    [Header("클리어탭")]
    public Image ClearImage;
    public Sprite SuccessSprite;
    public Sprite FailSprite;
    public GameObject SuccessEffect;
    public GameObject FailEffect;
    public TextMeshProUGUI ClearScoreText;

    [Header("Ʃ�丮��")]
    public Image TutorialImage;
    public TextMeshProUGUI TutorialText;
    public Sprite[] TutorialImages;
    public string[] TutorialTexts;
    private int currentTutorialIndex = 0;
    public TextMeshProUGUI IndexText;

    [Header("팝업 효과")]
    [Range(0.5f, 1f)] public float popStartScale = 0.85f;
    public float popStep1 = 0.18f;
    public float popStep2 = 0.10f;
    public float fadeIn = 0.15f;
    public float closeStep1 = 0.08f;
    public float closeStep2 = 0.12f;
    public float fadeOut = 0.12f;
    public float popOvershoot = 2.2f;

    [Header("점수 계산 로직 및 효과")]
    public TextMeshProUGUI ScoreText;
    public int BasicScore;
    public int StoneScore = 100;
    public int BronzeScore = 200;
    public int SilverScore = 300;
    public int GoldScore = 500;

    public bool isPaused = false;

    public GameManager gameManager;

    RectTransform timerRT;
    Vector2 basePos;
    Tween valueTw;
    Tween loopTw;

    // 점수 애니 전용
    RectTransform scoreRT;
    Vector2 scoreBasePos;
    Color scoreBaseColor;
    bool isAnimatingScore = false;
    DG.Tweening.Sequence scoreSeq;

    const string SHAKE_ID = "TimerShake";
    const string SCORE_SEQ_ID = "ScoreSeq";
    const string SCORE_SHAKE_ID = "ScoreShake";

    void Start()
    {
        timerRT = TimerImage.rectTransform;
        basePos = timerRT.anchoredPosition;

        if (ScoreText != null)
        {
            scoreRT = ScoreText.rectTransform;
            scoreBasePos = scoreRT.anchoredPosition;
            scoreBaseColor = ScoreText.color;
        }
        
        // 시작 점수 초기화
        ScoreText.text = BasicScore.ToString();
        if (gameManager) gameManager.score = BasicScore;

        StartTimer();
    }

    void OnDisable() { KillTimerTweens(); KillScoreTweens(); }
    void OnDestroy() { KillTimerTweens(); KillScoreTweens(); }

    void Update()
    {
        // 점수 애니 중에는 UI 텍스트 덮어쓰지 않음
        if (gameManager && !isAnimatingScore)
            ScoreText.text = gameManager.score.ToString();

        if (!PlayedGame.hadPlayed)
        {
            ShowTutorialUI();
            PlayedGame.hadPlayed = true;
        }
    }

    void ShowPopup(GameObject panel)
    {
        if (!panel) return;
        var rt = panel.GetComponent<RectTransform>();
        var cg = panel.GetComponent<CanvasGroup>() ?? panel.AddComponent<CanvasGroup>();
        cg.interactable = true; cg.blocksRaycasts = true;

        DOTween.Kill(panel);
        panel.SetActive(true);
        rt.localScale = Vector3.one * popStartScale;
        cg.alpha = 0f;

        DOTween.Sequence().SetUpdate(true).SetLink(panel)
            .Append(cg.DOFade(1f, fadeIn))
            .Join(rt.DOScale(1.05f, popStep1).SetEase(Ease.OutCubic))
            .Append(rt.DOScale(1.00f, popStep2).SetEase(Ease.OutBack, popOvershoot));
    }

    void HidePopup(GameObject panel)
    {
        if (!panel) return;
        var rt = panel.GetComponent<RectTransform>();
        var cg = panel.GetComponent<CanvasGroup>() ?? panel.AddComponent<CanvasGroup>();

        DOTween.Kill(panel);
        DOTween.Sequence().SetUpdate(true).SetLink(panel)
            .Append(rt.DOScale(0.92f, closeStep1).SetEase(Ease.InCubic))
            .Append(rt.DOScale(0.75f, closeStep2).SetEase(Ease.InBack, 1.5f))
            .Join(cg.DOFade(0f, fadeOut))
            .OnComplete(() => { panel.SetActive(false); rt.localScale = Vector3.one; });
    }

    public void ShowTutorialUI()
    {
        PauseGame();
        ShowPopup(TutorialUI); HidePopup(PauseUI); HidePopup(ResultUI);
        ShowTutorialImage();
    }

    public void ShowPauseUI()
    {
        PauseGame();
        ShowPopup(PauseUI); HidePopup(TutorialUI); HidePopup(ResultUI);
    }

    public void ShowResultUI()
    {
        PauseGame();
        ShowPopup(ResultUI); HidePopup(PauseUI); HidePopup(TutorialUI);
    }

    public void CloseUI()
    {
        ResumeGame();
        HidePopup(TutorialUI); HidePopup(PauseUI); HidePopup(ResultUI);
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
    }

    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }

    public void Sound()
    {
        bool soundOn = SoundButton.sprite != SoundButtonOn;
        if (soundOn)
        {
            SoundButton.sprite = SoundButtonOn;
            SoundText.text = "���Ұ�";
        }
        else
        {
            SoundButton.sprite = SoundButtonOff;
            SoundText.text = "�Ҹ� �ѱ�";
        }
    }

    public void Result(bool success)
    {
        if (success)
        {
            ActivateEffectUnscaled(SuccessEffect);
            ClearImage.sprite = SuccessSprite;
            ClearScoreText.text = gameManager ? gameManager.score.ToString() : "";
        }
        else
        {
            ActivateEffectUnscaled(FailEffect);
            ClearImage.sprite = FailSprite;
            ClearScoreText.text = "";
        }
    }

    public void StartTimer()
    {
        KillTimerTweens();

        Timer.minValue = 0;
        Timer.maxValue = 1;
        Timer.value = 1;

        if (gameManager == null) return;

        valueTw = Timer.DOValue(0f, gameManager.timerDuration)
            .SetEase(Ease.Linear)
            .SetUpdate(false) // 타임스케일 영향 받음(일시정지 시 멈춤)
            .SetLink(Timer.gameObject, LinkBehaviour.KillOnDestroy | LinkBehaviour.PauseOnDisable)
            .OnComplete(() =>
            {
                DOTween.Kill(SHAKE_ID);
                if (this && timerRT) timerRT.anchoredPosition = basePos;
                OnTimerEnd();
            });

        RunShakeLoop();
    }

    void RunShakeLoop()
    {
        loopTw = DOVirtual.Float(0f, 1f, 0.05f, _ =>
        {
            if (!this || timerRT == null) return;
            if (valueTw == null || !valueTw.IsActive() || !valueTw.IsPlaying())
            {
                timerRT.anchoredPosition = basePos;
                return;
            }

            float norm = Timer.value; // 1 → 0
            float t = Mathf.InverseLerp(shakeStartNormalized, 0f, norm);
            if (t <= 0f) { timerRT.anchoredPosition = basePos; return; }

            t = intensityCurve.Evaluate(t);
            float amp = Mathf.Lerp(minAmp, maxAmp, t);
            Vector2 jitter = UnityEngine.Random.insideUnitCircle * amp;
            timerRT.anchoredPosition = basePos + jitter;

        }).SetLoops(-1, LoopType.Restart)
          .SetId(SHAKE_ID)
          .SetUpdate(false)
          .SetLink(gameObject, LinkBehaviour.KillOnDestroy | LinkBehaviour.PauseOnDisable)
          .OnKill(() => { if (this && timerRT) timerRT.anchoredPosition = basePos; });
    }

    // === 점수 애니메이션 ===
    public void AnimateScoreChange(int from, int to, Action onComplete = null)
    {
        if (ScoreText == null) { onComplete?.Invoke(); return; }

        int start = Mathf.Max(0, from);
        int end = Mathf.Max(0, to);
        int delta = end - start;

        // 강조색: 보너스(초록) / 패널티(빨강)
        Color hiColor = delta >= 0 ? new Color(0.2f, 1f, 0.2f) : Color.red;

        float absDelta = Mathf.Abs(delta);
        float dur = Mathf.Clamp(absDelta / 1200f, 0.35f, 1.0f);
        float shakeDur = Mathf.Clamp(dur * 0.65f, 0.25f, 0.8f);
        float vibrato = Mathf.Lerp(12f, 28f, Mathf.Clamp01(absDelta / 1500f));
        float strength = Mathf.Lerp(10f, 35f, Mathf.Clamp01(absDelta / 1500f));

        // 이전 것 정리
        KillScoreTweens();
        isAnimatingScore = true;

        scoreSeq = DOTween.Sequence().SetUpdate(true);

        // 1) 강조색 전환 & 살짝 펀치 스케일
        scoreSeq.Append(ScoreText.DOColor(hiColor, 0.08f));
        scoreSeq.Join(scoreRT.DOPunchScale(Vector3.one * 0.12f, 0.18f, 8, 0.8f));

        // 2) 숫자 카운트 & 흔들림
        scoreSeq.Append(
            DOVirtual.Int(start, end, dur, v => ScoreText.text = v.ToString()).SetUpdate(true)
        );
        scoreSeq.Join(
            // DOTween 버전에 따라 strength는 Vector2 권장
            scoreRT.DOShakeAnchorPos(shakeDur, new Vector2(strength, strength),
                                     Mathf.RoundToInt(vibrato), 90, false, true)
                   .SetId(SCORE_SHAKE_ID)
                   .SetUpdate(true)
        );

        // 3) 원래 색상으로 복귀
        scoreSeq.Append(ScoreText.DOColor(scoreBaseColor, 0.22f));

        scoreSeq.OnComplete(() =>
        {
            if (scoreRT) { scoreRT.anchoredPosition = scoreBasePos; scoreRT.localScale = Vector3.one; }
            isAnimatingScore = false;
            onComplete?.Invoke();
        });
    }

    // UIManager 내부에 추가
    public void PlaySuccessBonus(int timeBonus, int from, int to, System.Action onComplete = null)
    {
        if (ScoreText == null || scoreRT == null)
        {
            // fallback: 그냥 숫자 애니만 하고 끝
            AnimateScoreChange(from, to, onComplete);
            return;
        }

        // 초록 보너스 플래시 + 플로팅 텍스트(+1234) → 숫자 애니 → 결과 콜백
        var green = new Color(0.2f, 1f, 0.2f);

        // 플로팅 TMPUGUI 생성 (프리팹 필요 없음)
        var parentRT = scoreRT.parent as RectTransform;
        var go = new GameObject("TimeBonusText", typeof(RectTransform));
        go.transform.SetParent(parentRT != null ? parentRT : scoreRT, false);
        var bonusRT = (RectTransform)go.transform;
        bonusRT.anchoredPosition = scoreBasePos + new Vector2(0f, 36f);

        var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.font = ScoreText.font;
        tmp.fontSize = Mathf.Max(ScoreText.fontSize * 0.85f, 18f);
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.text = $"+{timeBonus}";
        tmp.color = new Color(green.r, green.g, green.b, 0f);
        tmp.raycastTarget = false;

        // 시퀀스
        var seq = DOTween.Sequence().SetUpdate(true);

        // 1) 점수텍스트 초록 플래시 + 펀치 스케일
        seq.Append(ScoreText.DOColor(green, 0.06f));
        seq.Join(scoreRT.DOPunchScale(Vector3.one * 0.18f, 0.22f, 12, 0.9f));

        // 2) 플로팅 보너스 텍스트: 위로 살짝 뜨면서 등장/소멸
        seq.Join(tmp.DOFade(1f, 0.12f));
        seq.Join(bonusRT.DOAnchorPosY(bonusRT.anchoredPosition.y + 32f, 0.45f).SetEase(Ease.OutCubic));
        seq.AppendInterval(0.1f);
        seq.Append(tmp.DOFade(0f, 0.22f));
        seq.AppendCallback(() => Destroy(go));

        // 3) 숫자 증가 애니 시작(기존 숫자 애니 메서드 재사용)
        seq.AppendCallback(() =>
        {
            AnimateScoreChange(from, to, () =>
            {
                onComplete?.Invoke(); // 여기서 결과창 열어주면 됨
            });
        });

        // 4) 점수텍스트 색 복귀(보너스 플래시만)
        seq.Append(ScoreText.DOColor(scoreBaseColor, 0.18f));
    }


    void KillTimerTweens()
    {
        valueTw?.Kill(); valueTw = null;
        loopTw?.Kill(); loopTw = null;
        if (Timer) DOTween.Kill(Timer);
        DOTween.Kill(SHAKE_ID);
    }

    void KillScoreTweens()
    {
        scoreSeq?.Kill(); scoreSeq = null;
        DOTween.Kill(SCORE_SHAKE_ID);
        if (this && scoreRT) scoreRT.anchoredPosition = scoreBasePos;
        isAnimatingScore = false;
    }

    void OnTimerEnd()
    {
        Debug.Log("타이머 종료");
        // 필요하면 실패 처리 연결:
        // ShowResultUI();
        // Result(false);
    }

    public void ShowTutorialImage()
    {
        if (TutorialImages.Length > 0) TutorialImage.sprite = TutorialImages[0];
        if (TutorialTexts.Length > 0) TutorialText.text = TutorialTexts[0];
        currentTutorialIndex = 0;
    }

    public void NextTutorialImage()
    {
        if (currentTutorialIndex < TutorialImages.Length - 1)
        {
            currentTutorialIndex++;
            TutorialImage.sprite = TutorialImages[currentTutorialIndex];
            TutorialText.text = TutorialTexts[currentTutorialIndex];
            UpdateTutorialIndexText();
        }
    }

    public void PreviousTutorialImage()
    {
        if (currentTutorialIndex > 0)
        {
            currentTutorialIndex--;
            TutorialImage.sprite = TutorialImages[currentTutorialIndex];
            TutorialText.text = TutorialTexts[currentTutorialIndex];
            UpdateTutorialIndexText();
        }
    }

    public void UpdateTutorialIndexText()
    {
        IndexText.text = $"{currentTutorialIndex + 1}/{TutorialImages.Length}";
    }

    public void Reset()
    {
        Time.timeScale = 1f;
        KillTimerTweens();
        SceneManager.LoadScene("MainSceneTest");
    }

    void ActivateEffectUnscaled(GameObject fx)
    {
        if (!fx) return;
        if (!fx.TryGetComponent<UnscaledParticleDriver>(out _))
            fx.AddComponent<UnscaledParticleDriver>();
        fx.SetActive(false);
        fx.SetActive(true);
    }
}

[DisallowMultipleComponent]
public class UnscaledParticleDriver : MonoBehaviour
{
    ParticleSystem[] systems;

    void Awake()
    {
        systems = GetComponentsInChildren<ParticleSystem>(true);
    }

    void OnEnable()
    {
        foreach (var ps in systems)
        {
            if (ps == null) continue;
            ps.Simulate(0f, true, true);
            ps.Play(true);
        }
    }

    void LateUpdate()
    {
        if (Time.timeScale != 0f) return;
        float dt = Time.unscaledDeltaTime;
        if (dt <= 0f) return;
        foreach (var ps in systems)
        {
            if (ps != null) ps.Simulate(dt, true, false);
        }
    }
}
