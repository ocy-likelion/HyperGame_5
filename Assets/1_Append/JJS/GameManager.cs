using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    public UIManager uiManager; // UI 매니저 참조
    private bool gameEnd = false; // 게임 종료 여부
    public bool isWin = false; // 게임 승리 여부
    public float timerDuration = 30f; // 제한 시간
    private float currentTime;
    public int score = 0; // 게임 점수;

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
}
