using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [Header("UI 구성요소")]
    public GameObject TutorialUI;
    public GameObject PauseUI;
    public GameObject ResultUI;

    [Header("타이머 관련")]
    public float timerDuration = 30f;
    public Slider Timer;
    public Image TimerImage;
    [Range(0f, 1f)] public float shakeStartNormalized = 0.4f; // 남은비율이 이 값 이하부터 흔들기
    public float minAmp = 0f;   // 최소 진폭(px)
    public float maxAmp = 18f;  // 최대 진폭(px)
    public int minVibrato = 10;
    public int maxVibrato = 40;
    public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


    [Header("사운드 관련")]
    public Image SoundButton;
    public Sprite SoundButtonOn;
    public Sprite SoundButtonOff;

    [Header("클리어탭")]
    public Image CliarImage;
    public Sprite SuccessSprite;
    public Sprite FailSprite;
    public TextMeshProUGUI ClearScore;

    [Header("튜토리얼")]
    public Image TutorialImage;
    public TextMeshProUGUI TutorialText;
    public Sprite[] TutorialImages;
    public Text[] TutorialTexts;
    private int currentTutorialIndex = 0;

    bool SoundOn = true;
    bool Success = true; // 성공 여부
    RectTransform timerRT;
    Vector2 basePos;
    Tween valueTw;
    Tween loopTw;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timerRT = TimerImage.rectTransform;
        basePos = timerRT.anchoredPosition;
        StartTimer();
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void ShowTutorialUI()
    {
        TutorialUI.SetActive(true);
        PauseUI.SetActive(false);
        ResultUI.SetActive(false);
        ShowTutorialImage();
    }

    public void ShowPauseUI()
    {
        PauseUI.SetActive(true);
        TutorialUI.SetActive(false);
        ResultUI.SetActive(false);
    }
    public void ShowResultUI()
    {
        ResultUI.SetActive(true);
        PauseUI.SetActive(false);
        TutorialUI.SetActive(false);
    }

    public void CloseUI()
    {
        TutorialUI.SetActive(false);
        PauseUI.SetActive(false);
        ResultUI.SetActive(false);
    }

    public void Sound()
    {
        SoundOn = !SoundOn;
        if (SoundOn)
        {
            SoundButton.sprite = SoundButtonOn;
        }
        else
        {
            SoundButton.sprite = SoundButtonOff;
        }
    }

    public void Result()
    {
        if (Success) CliarImage.sprite = SuccessSprite;
        else CliarImage.sprite = FailSprite;

        //ClearScore.text = "Score: " + Score.ToString();
    }

    public void StartTimer()
    {
        // 기존 트윈 정리
        DOTween.Kill(Timer); DOTween.Kill("ShakeLoop");

        Timer.minValue = 0; Timer.maxValue = 1; Timer.value = 1;

        // 1) 값 1→0
        valueTw = Timer.DOValue(0f, timerDuration)
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                DOTween.Kill("ShakeLoop");
                timerRT.anchoredPosition = basePos;
                OnTimerEnd();
            });

        // 2) 흔들림 루프 시작
        RunShakeLoop(); // 아래 함수
    }

    void RunShakeLoop()
    {
        // duration 동안 0→1로 흘러가는 가상 타이머를 이용해 매 프레임 앵커를 갱신
        loopTw = DOVirtual.Float(0f, 1f, 0.05f, _ =>
        {
            if (!valueTw.IsActive() || !valueTw.IsPlaying()) return;

            float norm = Timer.value; // 1→0
            float t = Mathf.InverseLerp(shakeStartNormalized, 0f, norm);
            if (t <= 0f)
            {
                timerRT.anchoredPosition = basePos;
                return;
            }

            t = intensityCurve.Evaluate(t);
            float amp = Mathf.Lerp(minAmp, maxAmp, t);
            // 랜덤 흔들림
            Vector2 jitter = Random.insideUnitCircle * amp;
            timerRT.anchoredPosition = basePos + jitter;

        }).SetLoops(-1, LoopType.Restart)
          .SetId("ShakeLoop")
          .SetUpdate(true);
    }
    void OnTimerEnd()
    {
        // 타이머 종료 시 처리할 로직
        Debug.Log("타이머가 종료되었습니다.");
        // 예: 게임 오버 처리, UI 업데이트 등
    }

    public void ShowTutorialImage()
    {
        TutorialImage.sprite = TutorialImages[0];
        TutorialText.text = TutorialTexts[0].text;
    }

    public void NextTutorialImage()
    {
        if (currentTutorialIndex < TutorialImages.Length - 1) 
        { 
            currentTutorialIndex += 1;
            TutorialImage.sprite = TutorialImages[currentTutorialIndex];
            TutorialText.text = TutorialTexts[currentTutorialIndex].text;
        }
    }

    public void PreviousTutorialImage()
    {
        if (currentTutorialIndex > 0)
        {
            currentTutorialIndex -= 1;
            TutorialImage.sprite = TutorialImages[currentTutorialIndex];
            TutorialText.text = TutorialTexts[currentTutorialIndex].text;
        }
    }
}
