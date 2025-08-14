using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState { Playing, Clear, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("State")]
    public GameState CurrentState = GameState.Playing;

    [Header("Refs")]
    [SerializeField] private GimmickManager gimmickManager; // 인스펙터에서 연결 권장

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        // 참고: 인스펙터에서 연결 못 했다면 자동 탐색
        if (gimmickManager == null)
            gimmickManager = FindFirstObjectByType<GimmickManager>();

        // 30초마다 자동 기믹 발동 시작
        gimmickManager?.StartAutoGimmicks(30f);
    }

    // 메인 루프에서 클리어/오버가 결정되면 아래 둘 중 하나를 호출
    public void OnClear()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.Clear;
        gimmickManager?.StopAutoGimmicks();
        // TODO: 클리어 UI/점수 처리 등
        Debug.Log("[GameManager] CLEAR");
    }

    public void OnGameOver()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.GameOver;
        gimmickManager?.StopAutoGimmicks();
        // TODO: 게임오버 UI 처리 등
        Debug.Log("[GameManager] GAME OVER");
    }
}
