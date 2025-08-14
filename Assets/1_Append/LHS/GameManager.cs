using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Button DropButton; // unused
    public Image ClearPanel;
    public Image PausePanel;
    public Image GameOverPanel;
    public TMP_Text CountdownText; // unused

    public UIManager uiManager; // UI 매니저 참조
    private bool gameEnd = false; // 게임 종료 여부
    public bool isWin = false; // 게임 승리 여부
    public float timerDuration = 30f; // 제한 시간
    private float currentTime;
    public int score = 0; // 게임 점수;


    void OnEnable()
    {
        EventBus.Instance.Subscribe(Consts.GAMECLEAR, GameClear);
        EventBus.Instance.Subscribe(Consts.GAMEOVER, GameOver);
    }

    void OnDisable()
    {
        EventBus.Instance.Unsubscribe(Consts.GAMECLEAR, GameClear);
        EventBus.Instance.Unsubscribe(Consts.GAMEOVER, GameOver);
    }

    void Start()
    {
        currentTime = timerDuration; // 시작 시 타이머 초기화

    }

    void Update()
    {
        if (gameEnd) return; // 이미 종료 상태면 더 이상 진행 안 함

        // 타이머 감소
        currentTime -= Time.deltaTime;

        // 조건 체크
        if (currentTime <= 0 || isWin)
        {
            gameEnd = true;
            EndGame();
        }
    }

    private void EndGame()
    {
        // 결과 UI 활성화
        uiManager.ShowResultUI();
        uiManager.Result(isWin);
    }

    // 테스트용 승리 트리거
    public void Test()
    {
        isWin = true;
    }


    #region 게임 진행

    public void StartGame()
    {
        // 게임 시작
    }

    public void PauseGame()
    {
        // 게임 일시정지
        Time.timeScale = 0.0f;
        PausePanel.gameObject.SetActive(true);
    }

    public void ResumeGame()
    {
        // 게임 재개
        Time.timeScale = 1.0f;
        PausePanel.gameObject.SetActive(false);
    }

    public void RestartGame()
    {
        // 게임 재시작
        Time.timeScale = 1.0f;
        ClearPanel.gameObject.SetActive(false);
        GameOverPanel.gameObject.SetActive(false);
        SceneManager.LoadScene(Consts.GAMESCENE);
    }

    public void QuitGame()
    {
        // 메인 메뉴로?
        //SceneManager.LoadScene();
    }

    void GameClear()
    {
        // 게임 클리어
        Time.timeScale = 0.0f;
        ClearPanel.gameObject.SetActive(true);
    }

    void GameOver()
    {
        // 게임 오버
        Time.timeScale = 0.0f;
        GameOverPanel.gameObject.SetActive(true);
    }
    #endregion

    #region 소리
    void SoundOn()
    {
        // 음소거 해제
    }

    void SoundOff()
    {
        // 음소거
    }
    #endregion

    #region 개발용

    #endregion
}
