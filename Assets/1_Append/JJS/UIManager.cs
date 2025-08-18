using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI �������")]
    public GameObject TutorialUI;
    public GameObject PauseUI;
    public GameObject ResultUI;

    [Header("Ÿ�̸� ����")]
    public Slider Timer;
    public Image TimerImage;
    [Range(0f, 1f)] public float shakeStartNormalized = 0.4f;
    public float minAmp = 0f;
    public float maxAmp = 18f;
    public int minVibrato = 10;
    public int maxVibrato = 40;
    public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("���� ����")]
    public Image SoundButton;
    public Sprite SoundButtonOn;
    public Sprite SoundButtonOff;
    public TextMeshProUGUI SoundText;

    [Header("Ŭ������")]
    public Image CliarImage;
    public Sprite SuccessSprite;
    public Sprite FailSprite;
    public TextMeshProUGUI ClearScore;
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

   
    [Header("�˾� ȿ��")]
    [Range(0.5f, 1f)] public float popStartScale = 0.85f;
    public float popStep1 = 0.18f;     // 1.05����
    public float popStep2 = 0.10f;     // 1.00����
    public float fadeIn = 0.15f;
    public float closeStep1 = 0.08f;   // 1.00 -> 0.92
    public float closeStep2 = 0.12f;   // 0.92 -> 0.75
    public float fadeOut = 0.12f;
    public float popOvershoot = 2.2f;

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
            ActivateEffectUnscaled(SuccessEffect); // ��� ����Ʈ�� unscaled�� ���
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
        KillTimerTweens(); // ���� Ʈ�� ����

        Timer.minValue = 0;
        Timer.maxValue = 1;
        Timer.value = 1;

     
        valueTw = Timer.DOValue(0f, gameManager.timerDuration)
            .SetEase(Ease.Linear)
            .SetUpdate(false) 
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

            float norm = Timer.value;               // 1��0
            float t = Mathf.InverseLerp(shakeStartNormalized, 0f, norm);
            if (t <= 0f) { timerRT.anchoredPosition = basePos; return; }

            t = intensityCurve.Evaluate(t);
            float amp = Mathf.Lerp(minAmp, maxAmp, t);
            Vector2 jitter = Random.insideUnitCircle * amp;
            timerRT.anchoredPosition = basePos + jitter;

        }).SetLoops(-1, LoopType.Restart)
          .SetId(SHAKE_ID)
          .SetUpdate(false)
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
        Debug.Log("Ÿ�̸Ӱ� ����Ǿ����ϴ�.");
       
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
        // ���� ���� ���� ����
        Time.timeScale = 1f;

        KillTimerTweens(); // �� ���Ƴ���� ���� ���� ����
        SceneManager.LoadScene("MainSceneTest");
    }


    void ActivateEffectUnscaled(GameObject fx)
    {
        if (!fx) return;
        if (!fx.TryGetComponent<UnscaledParticleDriver>(out _))
            fx.AddComponent<UnscaledParticleDriver>();
        fx.SetActive(false);
        fx.SetActive(true); // ��� Ʈ����
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
