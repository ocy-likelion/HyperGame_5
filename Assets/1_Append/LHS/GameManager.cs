using UnityEngine;
using static Enums;

public class GameManager : MonoBehaviour
{
    public UIManager uiManager;
    private bool gameEnd = false;
    public bool isWin = false;
    public float timerDuration = 30f;
    private float currentTime;
    public int score = 5000;
    private bool isTimeFiveSecond = false;
    void OnEnable()
    {
        EventBus.Instance.Subscribe(Consts.END_GAME, EndGame);
    }

    void OnDisable()
    {
        EventBus.Instance.Unsubscribe(Consts.END_GAME, EndGame);
    }

    void Start()
    {
        currentTime = timerDuration;
    }

    void Update()
    {
        if (gameEnd) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 5f && !isTimeFiveSecond)
        {
            uiManager.BlinkColorFilter1Hz(new Color(1f, 166f / 255f, 166f / 255f), total: 5f);
            isTimeFiveSecond = true;
            RealSoundManager.Instance.PlayOneShot(SfxClips.TimeEmergency);
        }

        if (currentTime <= 0f || isWin)
        {
            gameEnd = true;     // 재진입 방지
            EndGame();
        }
        
    }

    private void EndGame()
    {
        uiManager?.HideHoldCountdownUI();
        if (gameEnd == false) gameEnd = true; 
        Time.timeScale = 0f; 

        if (isWin)
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

  
    public void Test() { isWin = true; }
}
