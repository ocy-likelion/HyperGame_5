using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayManager : MonoBehaviour
{
    // 상수
    public const float GOAL_HEIGHT = 2.25f; // 블럭 쌓기의 목표 높이
    private const float STABLE_DURATION = 3f; // 블럭들이 안정화된 후 버텨야하는 시간
    private const float UNSTABLE_VELOCITY_THRESHOLD = -0.03f; // 블록이 불안정하다고 판단할 Y축 속도 임계값

    // 씬 오브젝트
    [Header("씬 오브젝트")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject towerHeightLine; // 유니티 에디터 전용

    // private 필드(컴포넌트)
    private SpawnBlockManager spawnBlockManager;

    // private 필드
    private List<GameObject> topBlockList = new List<GameObject>(); // 플레이어가 떨어뜨린 블럭 오브젝트 리스트
    private GameObject highestBlock; // 가장 높이 있는 블럭 오브젝트
    private float currentTowerHeight; // 현재 쌓은 블럭(이하 타워)의 높이
    private float stableElapsedTime = 0f; // 타워가 안정을 찾은 후 경과된 시간
    private bool isBlockLanded; // 떨어뜨린 블럭이 쌓인 블럭들에 닿았는지 여부

    // public Getter
    public List<GameObject> BlockList => topBlockList;
    public GameObject HighestBlock => highestBlock;
    public float CurrentTowerHeight => currentTowerHeight;
    public Vector3 HighestTopPoint // 가장 높은 지점의 Position 값
    {
        get
        {
            if (highestBlock == null) return Vector3.zero;

            Collider2D col = highestBlock.GetComponent<Collider2D>();
            if (col == null) return highestBlock.transform.position;

            return new Vector3(highestBlock.transform.position.x, col.bounds.max.y, highestBlock.transform.position.z);
        }
    }

    // 유니티 콜백
    private void Awake()
    {
        TryGetComponent(out spawnBlockManager);
    }
    private void OnEnable()
    {
        EventBus.Instance.Subscribe(Consts.BLOCK_LANDED, OnBlockLanded);
        EventBus.Instance.Subscribe(Consts.RESPAWN_BLOCK, RespawnBlock);
    }
    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe(Consts.BLOCK_LANDED, OnBlockLanded);
        EventBus.Instance.Unsubscribe("RespawnBlock", RespawnBlock);
    }
    private void Start()
    {
        spawnBlockManager.SpawnRandomBlock();
    }
    private void Update()
    {
        if (gameManager.IsGameEnd) return;

        UpdateTowerStatus(); // 최고 블럭 및 높이 갱신, 안정 상태 체크, 클리어 체크

#if UNITY_EDITOR
        if (towerHeightLine != null)
        {
            towerHeightLine.transform.position = new Vector3(0.0f, currentTowerHeight, 0.0f);
        }
#endif
    }

    // 블럭
    private void OnBlockLanded() // 떨어뜨린 블럭이 쌓인 블럭들과 닿았을 때 호출되는 이벤트 메서드
    {
        isBlockLanded = true;
    }
    private void RespawnBlock() // 떨어뜨릴 블럭을 다시 생성
    {
        StartCoroutine(SpawnBlockWhenLanded());
    }
    private IEnumerator SpawnBlockWhenLanded() // 블럭이 쌓인 블럭들과 닿았을 때 다음 블럭을 생성하는 코루틴
    {
        // 떨어뜨린 블럭이 쌓인 블럭들에 닿을 때까지 대기
        while (!isBlockLanded)
        {
            yield return null;
        }

        isBlockLanded = false; // 블럭 랜딩 여부 초기화
        spawnBlockManager.SpawnRandomBlock(); // 블럭 생성

        // LEGACY
        // EventBus.Instance.Publish("SetCameraHeight", GetCameraHeight()); // 떨어짐과 동시에 카메라 위치 설정
    }

    // 쌓여진 블럭 상태 체크
    bool IsTowerUnstable() // 타워가 안정한지 체크
    {
        if (highestBlock == null) return false;

        var rb = highestBlock.GetComponent<Rigidbody2D>();
        if (rb == null) return false;

        // 표준 Rigidbody2D 속도 사용
        var velocityY = rb.linearVelocity.y;
        bool isNotSafe = (velocityY <= UNSTABLE_VELOCITY_THRESHOLD);
        return isNotSafe;
    }
    /// <summary>
    /// 매 프레임 타워 상태 갱신
    /// - 최고 블록 높이 계산
    /// - 쌓인 블럭들이 안정 상태인지 체크
    /// - 목표 높이 달성 시 클리어 처리
    /// </summary>
    private void UpdateTowerStatus()
    {
        // 최고 블럭 및 높이 갱신
        highestBlock = null;
        currentTowerHeight = float.NegativeInfinity;

        foreach (GameObject topBlock in topBlockList)
        {
            if (topBlock == null) continue;

            Collider2D col = topBlock.GetComponent<Collider2D>();
            if (col == null) continue;

            float topY = col.bounds.max.y;
            if (topY > currentTowerHeight)
            {
                currentTowerHeight = topY;
                highestBlock = topBlock;
            }
        }

        if (float.IsNegativeInfinity(currentTowerHeight)) // 블록 없을 경우 높이 초기화
            currentTowerHeight = 0f;

        // 쌓인 블럭들이 안정 상태인지 체크
        if (currentTowerHeight < GOAL_HEIGHT || IsTowerUnstable()) // 안정하지 못하거나 목표 높이에 도달하지 못한 경우 리턴
        {
            stableElapsedTime = 0f;
            uiManager?.ResetHoldCountdown();
            return;
        }

        stableElapsedTime += Time.deltaTime; // 안정 시간 누적
        uiManager.UpdateHoldCountdown(Mathf.Max(0f, STABLE_DURATION - stableElapsedTime)); // 안정된 시간만큼 UI 갱신

        // 목표 높이 달성 시 클리어 처리
        if (stableElapsedTime >= STABLE_DURATION)
        {
            uiManager.HideHoldCountdownUI();
            gameManager.IsClear = true; // 클리어 여부
            EventBus.Instance.Publish(Consts.END_GAME);
        }
    }

    // 카메라
    public float GetCameraHeight() // 카메라 높이값 계산 및 반환
    {
        if (highestBlock == null) return 0f;

        return highestBlock.transform.position.y + Consts.CAMERA_HEIGHT_OFFSET;
    }

    // Etc
    public bool IsExistTopBlock() // 현재 땅 위에 블럭이 있는지 검사
    {
        foreach (GameObject topBlock in topBlockList)
        {
            if (topBlock.activeSelf)
                return true;
        }

        return false;
    }
}
