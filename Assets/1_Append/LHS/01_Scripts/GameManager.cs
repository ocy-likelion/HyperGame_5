using UnityEngine;
using static Enums;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject touchArea;

    private bool isGameEnd = false;
    public bool isClear = false;
    public float timerDuration = 30f;
    private float currentTime;
    private int score = 10000;
    public int Score => score;
    private bool isTimeFiveSecond = false;

    // public Getter
    public bool IsGameEnd => isGameEnd;

    // 유니티 콜백
    private void OnEnable()
    {
        EventBus.Instance.Subscribe(Consts.END_GAME, EndGame);
    }
    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe(Consts.END_GAME, EndGame);
    }
    private void Start()
    {
        currentTime = timerDuration;
    }
    private void Update()
    {
        if (isGameEnd) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 5f && !isTimeFiveSecond)
        {
            uiManager.BlinkColorFilter1Hz(new Color(1f, 166f / 255f, 166f / 255f), total: 5f);
            isTimeFiveSecond = true;
            RealSoundManager.Instance.PlayOneShot(SfxClips.TimeEmergency);
        }

        if (currentTime <= 0f || isClear)
        {
            isGameEnd = true;     // 재진입 방지
            EndGame(); // 아래 Pulish하면 실행됨!
            EventBus.Instance.Publish(Consts.END_GAME);
        }
        
    }

    // 메인
    private void EndGame()
    {
        uiManager.HideHoldCountdownUI();
        if (isGameEnd == false) isGameEnd = true; 
        Time.timeScale = 0f;
        touchArea.SetActive(false); // 터치 막기

        if (isClear)
        {
            int timeBonus = Mathf.Max(0, Mathf.FloorToInt(currentTime) * 100);
            int from = score;
            int to = from + timeBonus;

           
            uiManager.KillTimerTweens();

            uiManager.PlaySuccessBonus(timeBonus, from, to, onComplete: () =>
            {
                score = to;                 // 합산 끝난 뒤 커밋
                uiManager.ShowResultUI();   // 결과창
                uiManager.Result(true);
            });
        }
        else
        {
            uiManager.KillTimerTweens();    // 안전장치
            uiManager.ShowResultUI();
            uiManager.Result(false);
        }
    }
    public void SetScore(int score)
    {
        this.score = score;
    }
    public void ModifyScore(int score)
    {
        this.score += score;
    }
    public void Test() { isClear = true; }
}
