using UnityEngine;

public class GameManager : MonoBehaviour
{
    public UIManager uiManager;
    private bool gameEnd = false;
    public bool isWin = false;
    public float timerDuration = 30f;
    private float currentTime;
    public int score = 5000;

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

        if (currentTime <= 0f || isWin)
        {
            gameEnd = true;
            EndGame();
        }
    }

    private void EndGame()
    {
        if (isWin)
        {
            int timeBonus = Mathf.Max(0, Mathf.FloorToInt(currentTime) * 100);
            int from = score;
            int to = from + timeBonus;

            uiManager.PlaySuccessBonus(timeBonus, from, to, onComplete: () =>
            {
                score = to;                 // ← 애니 끝난 뒤 실제 점수 커밋
                uiManager.ShowResultUI();   // 결과창 오픈
                uiManager.Result(true);
            });
        }
        else
        {
            uiManager.ShowResultUI();
            uiManager.Result(false);
        }
    }



    public void Test() { isWin = true; }
}
