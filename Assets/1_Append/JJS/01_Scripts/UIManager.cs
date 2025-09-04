using DG.Tweening;
using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("포스트프로세싱")]
    [SerializeField] private Volume warningScreenVolume;
    private ColorAdjustments colorAdj;
    const string FILTER_BLINK_ID = "FILTER_BLINK";

    [Header("타이틀")]
    public GameObject TitlePanel;
    // 게임이 한 번이라도 시작되었는지(리셋 후에도 유지) — 앱 재실행 시 초기화됨
    private static bool sGameStarted = false;

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

    [Header("튜토리얼")]
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

    [Header("클리어 카운트다운")]
    public TextMeshProUGUI HoldCountdownText;
    public float countPunch = 0.22f;       // 튀어나오는 강도
    public int countVibrato = 10;        // 진동
    public float countScaleUp = 1.25f;     // 기본 1 대비 최대 스케일
    public float countScaleDown = 0.95f;   // 마무리 축소
    public float countFadeIn = 0.06f;
    public float countFadeOut = 0.10f;


    [Header("점수 계산 로직 및 효과")]
    public TextMeshProUGUI text_Score;
    private int basicScore;
    private int blockScore = 200;
    public int BlockScore => blockScore;
    public bool isPaused = false;

    [SerializeField] private GameManager gameManager;

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

    int lastShownCount = -1;
    Tween countSeq;
    CanvasGroup holdCountCG;

    const string SHAKE_ID = "TimerShake";
    const string SCORE_SEQ_ID = "ScoreSeq";
    const string SCORE_SHAKE_ID = "ScoreShake";

    void Awake()
    {
        if (!warningScreenVolume.profile.TryGet(out colorAdj))
            colorAdj = warningScreenVolume.profile.Add<ColorAdjustments>(true);

        // override 활성화
        colorAdj.colorFilter.overrideState = true;
        colorAdj.postExposure.overrideState = true;

        if (HoldCountdownText != null)
        {
            holdCountCG = HoldCountdownText.GetComponent<CanvasGroup>();
            if (holdCountCG == null) holdCountCG = HoldCountdownText.gameObject.AddComponent<CanvasGroup>();
            HoldCountdownText.rectTransform.localScale = Vector3.one;
            holdCountCG.alpha = 0f;
            HoldCountdownText.gameObject.SetActive(false);
        }

        // 변수 초기화
        basicScore = gameManager.Score;
        text_Score.text = basicScore.ToString();
    }

    void Start()
    {
        timerRT = TimerImage.rectTransform;
        basePos = timerRT.anchoredPosition;

        if (text_Score != null)
        {
            scoreRT = text_Score.rectTransform;
            scoreBasePos = scoreRT.anchoredPosition;
            scoreBaseColor = text_Score.color;
        }

        if (!sGameStarted)
        {
            // 아직 Start 버튼을 누른 적이 없으면 타이틀 먼저
            PauseGame();
            ShowOnlyTitle();
        }
        else
        {
            HideTitleInstant();  
            ResumeGame();
            StartTimer();
        }
    }

    void OnDisable() { KillTimerTweens(); KillScoreTweens(); }
    void OnDestroy() { KillTimerTweens(); KillScoreTweens(); }

    void Update()
    {
        // 점수 애니 중에는 UI 텍스트 덮어쓰지 않음
        if (gameManager && !isAnimatingScore)
            text_Score.text = gameManager.Score.ToString();

        // 시작 후에만 튜토리얼 자동 오픈
        if (sGameStarted && !PlayedGame.hadPlayed)
        {
            ShowTutorialUI();
            PlayedGame.hadPlayed = true;
        }
    }

  
    void HideTitleInstant()
    {
        if (!TitlePanel) return;
        var cg = TitlePanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
        TitlePanel.SetActive(false);
    }

    // 타이틀만 켜고 다른 팝업은 끄는 헬퍼
    void ShowOnlyTitle()
    {
        if (TitlePanel) ShowPopup(TitlePanel);
        HidePopup(TutorialUI);
        HidePopup(PauseUI);
        HidePopup(ResultUI);
    }

    // 타이틀 Start 버튼에 연결
    public void OnPressStartGame()
    {
        if (sGameStarted) return;
        sGameStarted = true;

        HidePopup(TitlePanel);
        ResumeGame();
        StartTimer();

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
            SoundText.text = "사운드 ON";
        }
        else
        {
            SoundButton.sprite = SoundButtonOff;
            SoundText.text = "사운드 OFF";
        }

        RealSoundManager.Instance.OnClickMute();
    }

    public void Result(bool success)
    {
        RealSoundManager.Instance.GameEndFade();
        if (success)
        {
            ActivateEffectUnscaled(SuccessEffect);
            ClearImage.sprite = SuccessSprite;
            ClearScoreText.text = gameManager ? gameManager.Score.ToString() : "";
            //Bridge.SubmitScore(gameManager.score);
            RealSoundManager.Instance.PlayOneShot(Enums.SfxClips.Win);
        }
        else
        {
            ActivateEffectUnscaled(FailEffect);
            ClearImage.sprite = FailSprite;
            ClearScoreText.text = "0";
            RealSoundManager.Instance.PlayOneShot(Enums.SfxClips.Lose);
        }
    }

    public void StartTimer()
    {
        KillTimerTweens();

        Timer.minValue = 0;
        Timer.maxValue = 1;
        Timer.value = 1;

        if (gameManager == null) return;

        valueTw = Timer.DOValue(0f, GameManager.GAME_TIME_LIMIT)
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
        if (text_Score == null) { onComplete?.Invoke(); return; }

        int start = Mathf.Max(0, from);
        int end = Mathf.Max(0, to);
        int delta = end - start;

        // 강조색
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
        scoreSeq.Append(text_Score.DOColor(hiColor, 0.08f));
        scoreSeq.Join(scoreRT.DOPunchScale(Vector3.one * 0.12f, 0.18f, 8, 0.8f));

        // 2) 숫자 카운트 & 흔들림
        scoreSeq.Append(
            DOVirtual.Int(start, end, dur, v => text_Score.text = v.ToString()).SetUpdate(true)
        );
        scoreSeq.Join(
            scoreRT.DOShakeAnchorPos(shakeDur, new Vector2(strength, strength),
                                     Mathf.RoundToInt(vibrato), 90, false, true)
                   .SetId(SCORE_SHAKE_ID)
                   .SetUpdate(true)
        );

        // 3) 원래 색상으로 복귀
        scoreSeq.Append(text_Score.DOColor(scoreBaseColor, 0.22f));

        scoreSeq.OnComplete(() =>
        {
            if (scoreRT) { scoreRT.anchoredPosition = scoreBasePos; scoreRT.localScale = Vector3.one; }
            isAnimatingScore = false;
            onComplete?.Invoke();
        });
    }

    // 보너스 연출
    public void PlaySuccessBonus(int timeBonus, int from, int to, System.Action onComplete = null)
    {
        if (text_Score == null || scoreRT == null)
        {
            AnimateScoreChange(from, to, onComplete);
            return;
        }

        var green = new Color(0.2f, 1f, 0.2f);

        var parentRT = scoreRT.parent as RectTransform;
        var go = new GameObject("TimeBonusText", typeof(RectTransform));
        go.transform.SetParent(parentRT != null ? parentRT : scoreRT, false);
        var bonusRT = (RectTransform)go.transform;
        bonusRT.anchoredPosition = scoreBasePos + new Vector2(0f, 36f);

        var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.font = text_Score.font;
        tmp.fontSize = Mathf.Max(text_Score.fontSize * 0.85f, 18f);
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.text = $"+{timeBonus}";
        tmp.color = new Color(green.r, green.g, green.b, 0f);
        tmp.raycastTarget = false;

        var seq = DOTween.Sequence().SetUpdate(true);

        seq.Append(text_Score.DOColor(green, 0.06f));
        seq.Join(scoreRT.DOPunchScale(Vector3.one * 0.18f, 0.22f, 12, 0.9f));

        seq.Join(tmp.DOFade(1f, 0.12f));
        seq.Join(bonusRT.DOAnchorPosY(bonusRT.anchoredPosition.y + 32f, 0.45f).SetEase(Ease.OutCubic));
        seq.AppendInterval(0.1f);
        seq.Append(tmp.DOFade(0f, 0.22f));
        seq.AppendCallback(() => Destroy(go));

        seq.AppendCallback(() =>
        {
            AnimateScoreChange(from, to, () =>
            {
                onComplete?.Invoke();
            });
        });

        seq.Append(text_Score.DOColor(scoreBaseColor, 0.18f));
    }

    public void KillTimerTweens()
    {
        valueTw?.Kill(); valueTw = null;
        loopTw?.Kill(); loopTw = null;
        if (Timer) DOTween.Kill(Timer);
        DOTween.Kill(SHAKE_ID);
    }

   public void KillScoreTweens()
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
        UpdateTutorialIndexText();
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

    public async void Reset()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // 게임 일시정지
        Time.timeScale = 0f;

        // 광고 재생
        AdLoadStatus loadStatus = await ShowAd.LoadAndShowAdAsync();

        if (loadStatus == AdLoadStatus.Show) // 광고를 보여주는 중이라면
        {
            while (Bridge.GetAdStatus() != AdLoadStatus.Closed) // 광고가 닫힐 때까지 대기
            {
                await Task.Yield();
            }

            Logger.Instance.SetLog("광고 닫힘: 게임 재개");
            Debug.Log("광고 닫힘: 게임 재개");
        }
        else // 광고를 불러오는데 타임아웃했거나 실패했다면
        {
            Logger.Instance.SetLog("광고 로드 실패: 게임 재개");
            Debug.Log("광고 로드 실패: 게임 재개");
        }

        // 게임 재개 로직
        Time.timeScale = 1f;
        KillTimerTweens();
        SceneManager.LoadScene("MainScene");
#else
        Time.timeScale = 1f;
        KillTimerTweens();
        Logger.Instance.SetLog("광고 로드");
        Bridge.TEMPTEMP();
        SceneManager.LoadScene("MainScene");
#endif
    }


    void ActivateEffectUnscaled(GameObject fx)
    {
        if (!fx) return;
        if (!fx.TryGetComponent<UnscaledParticleDriver>(out _))
            fx.AddComponent<UnscaledParticleDriver>();
        fx.SetActive(false);
        fx.SetActive(true);
    }

    public void ShowHoldCountdownUI()
    {
        if (HoldCountdownText == null) return;
        DOTween.Kill(HoldCountdownText);
        HoldCountdownText.gameObject.SetActive(true);
        HoldCountdownText.rectTransform.localScale = Vector3.one;
        holdCountCG.alpha = 0f;
        holdCountCG.DOFade(1f, countFadeIn).SetUpdate(false).SetLink(HoldCountdownText.gameObject);
    }

    public void HideHoldCountdownUI()
    {
        if (HoldCountdownText == null) return;
        lastShownCount = -1;
        DOTween.Kill(HoldCountdownText);
        holdCountCG.DOFade(0f, countFadeOut)
            .OnComplete(() =>
            {
                if (HoldCountdownText != null)
                {
                    HoldCountdownText.gameObject.SetActive(false);
                    HoldCountdownText.rectTransform.localScale = Vector3.one;
                }
            })
            .SetUpdate(false).SetLink(HoldCountdownText.gameObject);
    }

    public void ResetHoldCountdown()
    {
        // 도중 취소될 때 숫자/트윈 초기화용
        HideHoldCountdownUI();
    }


    public void UpdateHoldCountdown(float secondsLeft)
    {
        if (HoldCountdownText == null) return;

        // 3.0~2.01 -> 3, 2.0~1.01 -> 2, 1.0~0.01 -> 1
        int display = Mathf.Clamp(Mathf.CeilToInt(secondsLeft), 1, 3);

        // 처음 진입 시 UI 표시
        if (!HoldCountdownText.gameObject.activeSelf) ShowHoldCountdownUI();

        // 같은 숫자면 애니 재생 X
        if (display == lastShownCount) return;
        lastShownCount = display;

        HoldCountdownText.text = display.ToString();

        var rt = HoldCountdownText.rectTransform;
        DOTween.Kill(HoldCountdownText);

        // scale 1 -> 1.25(빵) -> 0.95 -> 1.0 느낌
        DG.Tweening.Sequence s = DOTween.Sequence().SetUpdate(false).SetLink(HoldCountdownText.gameObject);

        // 스타트 순간 살짝 줄였다가 크게 빵
        rt.localScale = Vector3.one * 0.9f;

        s.Append(rt.DOScale(countScaleUp, 0.14f).SetEase(Ease.OutBack, 2.0f));
        s.Append(rt.DOScale(countScaleDown, 0.10f).SetEase(Ease.InOutSine));
        s.Append(rt.DOScale(1f, 0.08f).SetEase(Ease.OutSine));

        // 동시에 약한 펀치(선호에 따라 삭제 가능)
        s.Join(rt.DOPunchScale(Vector3.one * countPunch, 0.22f, countVibrato, 0.9f));
        countSeq = s;
    }

    public void BlinkColorFilter1Hz(Color onColor, float total = 3f)
    {
        if (colorAdj == null) return;

        DOTween.Kill(FILTER_BLINK_ID);

        var offColor = Color.white;
        float oneBlink = 0.5f;                 // half-period
        total = Mathf.Max(0f, total);
        int loops = Mathf.Max(1, Mathf.FloorToInt(total)); // 1초 = 1 loop

        var seq = DOTween.Sequence()
            .SetId(FILTER_BLINK_ID)
            .SetUpdate(false) // 타임스케일 영향 받음(일시정지 시 멈춤)
            .SetLink(Timer.gameObject, LinkBehaviour.KillOnDestroy | LinkBehaviour.PauseOnDisable)
            .OnComplete(() => colorAdj.colorFilter.value = offColor);

        for (int i = 0; i < loops; i++)
        {
            // ON (0.5s)
            seq.Append(DOTween.To(
                () => colorAdj.colorFilter.value,
                c => colorAdj.colorFilter.value = c,
                onColor, oneBlink).SetEase(Ease.InOutSine));

            // OFF (0.5s)
            seq.Append(DOTween.To(
                () => colorAdj.colorFilter.value,
                c => colorAdj.colorFilter.value = c,
                offColor, oneBlink).SetEase(Ease.InOutSine));
        }
    }
    
    public void GoToLeaderBoard()
    {
        Bridge.OpenLeaderBoard();
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
