using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI 구성요소")]
    public GameObject TutorialUI;
    public GameObject PauseUI;
    public GameObject ResultUI;

    [Header("타이머 관련")]
    public Slider Timer;
    public Image TimerImage;
    [Range(0f, 1f)] public float shakeStartNormalized = 0.4f;
    public float minAmp = 0f;
    public float maxAmp = 18f;
    public int minVibrato = 10;
    public int maxVibrato = 40;
    public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("사운드 관련")]
    public Image SoundButton;
    public Sprite SoundButtonOn;
    public Sprite SoundButtonOff;
    public TextMeshProUGUI SoundText;

    [Header("클리어탭")]
    public Image CliarImage;
    public Sprite SuccessSprite;
    public Sprite FailSprite;
    public TextMeshProUGUI ClearScore;
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

    public bool isPaused = false;

    public TextMeshProUGUI ScoreText;
    public GameManager gameManager;

    RectTransform timerRT;
    Vector2 basePos;
    Tween valueTw;
    Tween loopTw;

    const string SHAKE_ID = "TimerShake";

    void Start()
    {
        timerRT = TimerImage.rectTransform;
        basePos = timerRT.anchoredPosition;
        StartTimer();
    }

    void OnDisable() { KillTimerTweens(); }
    void OnDestroy() { KillTimerTweens(); }

    void Update()
    {
        if (gameManager) ScoreText.text = gameManager.score.ToString();

        if (!PlayedGame.hadPlayed)
        {
            ShowTutorialUI();
            PlayedGame.hadPlayed = true;
        }
    }

    public void ShowTutorialUI()
    {
        PauseGame();
        TutorialUI.SetActive(true);
        PauseUI.SetActive(false);
        ResultUI.SetActive(false);
        ShowTutorialImage();
    }

    public void ShowPauseUI()
    {
        PauseGame();
        PauseUI.SetActive(true);
        TutorialUI.SetActive(false);
        ResultUI.SetActive(false);
    }

    public void ShowResultUI()
    {
        PauseGame();
        ResultUI.SetActive(true);
        PauseUI.SetActive(false);
        TutorialUI.SetActive(false);
    }

    public void CloseUI()
    {
        ResumeGame();
        TutorialUI.SetActive(false);
        PauseUI.SetActive(false);
        ResultUI.SetActive(false);
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // ★ 게임 세계 정지
        // (UI 전용 트윈/파티클은 unscaled로 따로 돌림)
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
            SoundText.text = "음소거";
        }
        else
        {
            SoundButton.sprite = SoundButtonOff;
            SoundText.text = "소리 켜기";
        }
    }

    public void Result(bool success)
    {
        if (success)
        {
            ActivateEffectUnscaled(SuccessEffect); // 결과 이펙트는 unscaled로 재생
            CliarImage.sprite = SuccessSprite;
            ClearScoreText.text = gameManager ? gameManager.score.ToString() : "";
        }
        else
        {
            ActivateEffectUnscaled(FailEffect);
            CliarImage.sprite = FailSprite;
            ClearScoreText.text = "";
        }
    }

    public void StartTimer()
    {
        KillTimerTweens(); // 기존 트윈 정리

        Timer.minValue = 0;
        Timer.maxValue = 1;
        Timer.value = 1;

        // 1) 값 1→0  ★ scaled time으로 변경 (timeScale=0이면 자동 정지)
        valueTw = Timer.DOValue(0f, gameManager.timerDuration)
            .SetEase(Ease.Linear)
            .SetUpdate(false) // ★ 중요: unscaled(=true) 제거
            .SetLink(Timer.gameObject, LinkBehaviour.KillOnDestroy | LinkBehaviour.PauseOnDisable)
            .OnComplete(() =>
            {
                DOTween.Kill(SHAKE_ID);
                if (this && timerRT) timerRT.anchoredPosition = basePos;
                OnTimerEnd();
            });

        // 2) 흔들림 루프 시작  ★ 이것도 scaled time으로 (일시정지 시 멈춤)
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

            float norm = Timer.value;               // 1→0
            float t = Mathf.InverseLerp(shakeStartNormalized, 0f, norm);
            if (t <= 0f) { timerRT.anchoredPosition = basePos; return; }

            t = intensityCurve.Evaluate(t);
            float amp = Mathf.Lerp(minAmp, maxAmp, t);
            Vector2 jitter = Random.insideUnitCircle * amp;
            timerRT.anchoredPosition = basePos + jitter;

        }).SetLoops(-1, LoopType.Restart)
          .SetId(SHAKE_ID)
          .SetUpdate(false) // ★ 중요: unscaled(=true) 제거
          .SetLink(gameObject, LinkBehaviour.KillOnDestroy | LinkBehaviour.PauseOnDisable)
          .OnKill(() => { if (this && timerRT) timerRT.anchoredPosition = basePos; });
    }

    void KillTimerTweens()
    {
        valueTw?.Kill(); valueTw = null;
        loopTw?.Kill(); loopTw = null;
        if (Timer) DOTween.Kill(Timer);
        DOTween.Kill(SHAKE_ID);
    }

    void OnTimerEnd()
    {
        Debug.Log("타이머가 종료되었습니다.");
        // gameManager?.EndGame();
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
        // 게임 멈춤 상태 해제
        Time.timeScale = 1f;

        KillTimerTweens(); // 씬 갈아끼우기 전에 안전 종료
        SceneManager.LoadScene("MainScene");
    }


    // 결과 이펙트가 timeScale=0이어도 보이게(없으면 자동 부착)
    void ActivateEffectUnscaled(GameObject fx)
    {
        if (!fx) return;
        if (!fx.TryGetComponent<UnscaledParticleDriver>(out _))
            fx.AddComponent<UnscaledParticleDriver>();
        fx.SetActive(false);
        fx.SetActive(true); // 재생 트리거
    }
}

/// timeScale=0에서도 파티클이 재생되도록 강제 시뮬레이션
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
