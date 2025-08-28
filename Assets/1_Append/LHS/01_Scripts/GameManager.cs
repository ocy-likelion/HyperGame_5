using UnityEngine;
using static Enums;

public class GameManager : MonoBehaviour
{
    // 상수
    public const float GAME_TIME_LIMIT = 60f; // 제한 시간
    private const float TIME_WARNING_THRESHOLD = 5f;

    // private 필드(인스펙터 노출)
    [SerializeField] private PlayManager playManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject touchArea;

    // private 필드
    private bool isGameEnd = false;
    private bool isTimeFiveSecond = false;
    private float remainTime;
    private int score = 10000;

    // public 필드
    public bool IsClear = false;
    public float GameElaspedTime;

    // public Getter
    public bool IsGameEnd => isGameEnd;
    public float RemainTime => remainTime;
    public int Score => score;

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
        remainTime = GAME_TIME_LIMIT;
    }
    private void Update()
    {
        if (isGameEnd) return;

        remainTime -= Time.deltaTime;
        GameElaspedTime += Time.deltaTime;

        if (remainTime <= TIME_WARNING_THRESHOLD && !isTimeFiveSecond) // 남은 시간이 5초 이하인 경우
        {
            uiManager.BlinkColorFilter1Hz(new Color(1f, 166f / 255f, 166f / 255f), total: 5f);
            isTimeFiveSecond = true;
            RealSoundManager.Instance.PlayOneShot(SfxClips.TimeEmergency);
        }

        if (remainTime <= 0f || IsClear) // 시간이 다 되거나 클리어한 경우
        {
            isGameEnd = true; // 재진입 방지
            EventBus.Instance.Publish(Consts.END_GAME);
        }
    }

    // 메인
    private void EndGame()
    {
        uiManager.HideHoldCountdownUI();

        if (!isGameEnd)
        {
            isGameEnd = true;
        }

        Time.timeScale = 0f; // 게임 정지
        touchArea.SetActive(false); // 터치 막기

        if (IsClear) // 클리어한 경우 별도의 클리어 UI 등장
        {
            int timeBonus = Mathf.Max(0, Mathf.FloorToInt(remainTime) * 100);
            int from = score;
            int to = from + timeBonus;

            uiManager.KillTimerTweens(); // 안전장치

            uiManager.PlaySuccessBonus(timeBonus, from, to, onComplete: () =>
            {
                score = to; // 합산 끝난 뒤 커밋
                uiManager.ShowResultUI(); // 결과창
                uiManager.Result(true);
            });
        }
        else // 아닌 경우 실패 UI 등장
        {
            uiManager.KillTimerTweens(); // 안전장치
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

    // Etc
    public void Test() { IsClear = true; }
}
