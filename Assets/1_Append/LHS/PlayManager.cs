using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    public GameObject BlockPrefab;
    public List<GameObject> blockList = new List<GameObject>();
    private readonly List<GameObject> newBlockList = new List<GameObject>();

    private GameObject highestBlock;

    [HideInInspector] public float currentTowerHeight; // Vector.y의 값
    [HideInInspector] public float GOAL_TOWER_HEIGHT = 3.0f; // Vector.y의 값

    public TMP_Text elapsedTimeText;
    private float totalElapsedTime = 0.0f;
    private float timeLimit = 15.0f;
    private bool gameEnded = false;

    // 컴포넌트
    MineralDataManager mineralDataManager;
    SabotageEventManager sabotageEventManager;

    void Awake()
    {
        TryGetComponent<MineralDataManager>(out mineralDataManager);
        TryGetComponent<SabotageEventManager>(out sabotageEventManager);
    }
    // dev UI
    [SerializeField] private GameObject towerHeightLine;
    private float nextTurnTime = 2f;

    // ==== 추가된 프로퍼티(HEAD) ====
    public GameObject HighestBlock => highestBlock;

    public Vector3 HighestTopPoint
    {
        get
        {
            if (highestBlock == null) return Vector3.zero;
            var col = highestBlock.GetComponent<Collider2D>();
            if (col == null) return highestBlock.transform.position;
            var b = col.bounds;
            return new Vector3(highestBlock.transform.position.x, b.max.y, highestBlock.transform.position.z);
        }
    }

    void OnEnable()
    {
        EventBus.Instance.Subscribe(Consts.END_GAME, EndGame);
        EventBus.Instance.Subscribe(Consts.BLOCK_LANDED, AddBlock);
        EventBus.Instance.Subscribe("RespawnBlock", RespawnBlock);
    }

    void OnDisable()
    {
        EventBus.Instance.Unsubscribe(Consts.END_GAME, EndGame);
        EventBus.Instance.Unsubscribe(Consts.BLOCK_LANDED, AddBlock);
        EventBus.Instance.Unsubscribe("RespawnBlock", RespawnBlock);
    }

    void Start()
    {
        StartCoroutine(GameTimer());
        //CreateBlock();
        mineralDataManager.GenerateBlock(); // 기존 CreateBlock 메서드 대신
    }

    void Update()
    {
        if (gameEnded) return;

        //elapsedTimeText.text = ((int)totalElapsedTime).ToString();

        CheckHighestBlock();
        CheckTowerHeight();

        if (currentTowerHeight > 1f) sabotageEventManager.TriggerMoleEvent();

        if (currentTowerHeight > 2f) sabotageEventManager.TriggerSinkHoleEvent();
    }

    IEnumerator GameTimer()
    {
        while (totalElapsedTime < timeLimit)
        {
            totalElapsedTime += Time.deltaTime;
            yield return null;
        }

        EventBus.Instance.Publish(Consts.GAME_OVER);
    }

    void EndGame()
    {
        gameEnded = true;
    }

    void CheckHighestBlock()
    {
        // 타워 높이 갱신 & 최상단 블럭 갱신
        currentTowerHeight = float.NegativeInfinity;
        highestBlock = null;

        foreach (var block in blockList)
        {
            float height = block.GetComponent<Collider2D>().bounds.max.y;

            //block.GetComponent<SpriteRenderer>().color = Color.white; // 임시

            if (block == null) return;

            if (height > currentTowerHeight)
            {
                currentTowerHeight = height;
                highestBlock = block;
            }
        }

        if (float.IsNegativeInfinity(currentTowerHeight))
            currentTowerHeight = 0f;
    }

    void CheckTowerHeight()
    {
        // 목표 위치에 도달하면 게임 클리어
        if (currentTowerHeight > GOAL_TOWER_HEIGHT)
        {
            var gameManager = GameObject.FindFirstObjectByType<GameManager>();
            if (gameManager != null) gameManager.isWin = true;
            EventBus.Instance.Publish(Consts.END_GAME);
        }
    }
    #region gimmicks
    void Wind()
    {
        Debug.Log("휭");
    }
    void Mole()
    {
        Debug.Log("두더지");
    }
    #endregion

    #region 개발용

    public void CreateBlock()
    {
        // 광물 생성 및 드롭
        // DropButton.gameObject.SetActive(false);

        //GameObject newBlock = Instantiate(BlockPrefab);
        //EventBus.Instance.Publish("SpawnBlock", newBlock);
        //newBlockList.Add(newBlock);

        // newBlock.transform.position = new Vector3(
        //     blockSpawnPoint.transform.position.x,
        //     blockSpawnPoint.transform.position.y,
        //     blockSpawnPoint.transform.position.z
        //     );

        // StartCoroutine(WaitAndShowButton());
    }
    bool isLanded = false;
    void AddBlock()
    {
        //if (newBlockList.Count > 0)
        //{
        //    blockList.Add(newBlockList[0]);
        //    newBlockList.RemoveAt(0);
        //}

        isLanded = true;
    }

    void RespawnBlock()
    {
        StartCoroutine(WaitAndCreateBlock());
    }

    IEnumerator WaitAndCreateBlock()
    {
        //yield return new WaitForSeconds(nextTurnTime);

        ////쓰러지 고있는지 판단해서 쓰러지면 더 기다리게 함
        //while (CheckTowerIsNotSafe())
        //{
        //    yield return new WaitForSeconds(nextTurnTime);
        //}
        //EventBus.Instance.Publish("SetCameraHeight", CalculateSetCameraHeight());
        //yield return new WaitForSeconds(1f);    // 카메라 움직이는 동안 생성 기다림
        //CreateBlock();

        while (!isLanded)
        {
            yield return null;
        }

        isLanded = false;

        mineralDataManager.GenerateBlock(); // 기존 CreateBlock 메서드 대신
    }

    // 타워가 안정한지 체크
    bool CheckTowerIsNotSafe()
    {
        if (highestBlock == null) return false; // 첫 번째 블럭이 떨어진 경우

        var velocityX = highestBlock.GetComponent<Rigidbody2D>().linearVelocityX;
        var velocityY = highestBlock.GetComponent<Rigidbody2D>().linearVelocityY;
        var isNotSafe = (velocityY <= -0.03f);
        
        return isNotSafe;
    }

    // 카메라 높이값 계산
    float CalculateSetCameraHeight()
    {
        if (highestBlock == null) return 0f;

        // 화면 중하단 좌표 (미사용이지만 남겨둠)
        Vector3 screenLowerCenter =
            new Vector3(Screen.width * 0.5f, Screen.height * 0.2f, Consts.CAMERA_OFFSET);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenLowerCenter);

        float height = highestBlock.transform.position.y + Consts.HEIGHT_OFFSET;
        return height;
    }
    #endregion 

    public bool HasActiveBlock() // 바닥에 떨어진 블럭이 있는지 확인하는 메서드
    {
        return blockList.Any(obj => obj.activeSelf);
    }
}
