using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayManager : MonoBehaviour
{
    public Button DropButton;

    public GameObject BlockPrefab;
    List<GameObject> blockList = new List<GameObject>();
    List<GameObject> newBlockList = new List<GameObject>();
    GameObject highestBlock;

    public GameObject blockSpawnPoint;
    float blockSpawnPointFreqeuncy = 1.5f;

    public float currentTowerHeight;
    float goalTowerHeight = 2.0f; // 임시

    public TMP_Text elapsedTimeText;
    float totalElapsedTime = 0.0f;
    float timeLimit = 15.0f;
    bool gameEnded = false;

    void OnEnable()
    {
        EventBus.Instance.Subscribe(Consts.END_GAME, EndGame);
        EventBus.Instance.Subscribe(Consts.BLOCK_LANDED, AddBlock);
    }

    void OnDisable()
    {
        EventBus.Instance.Unsubscribe(Consts.END_GAME, EndGame);
        EventBus.Instance.Unsubscribe(Consts.BLOCK_LANDED, AddBlock);
    }

    void Start()
    {
        StartCoroutine(GameTimer());
    }

    void Update()
    {
        #region 임시 blockSpawnPoint 이동, 드래그 앤 드롭으로 고쳐야 함
        blockSpawnPoint.transform.position = new Vector3(Mathf.Sin(Time.time * blockSpawnPointFreqeuncy), 4.0f, 0.0f);
        #endregion

        if (gameEnded == true) return;

        elapsedTimeText.text = ((int)totalElapsedTime).ToString();

        CheckHighestBlock();
        CheckTowerHeight();

        towerHeightLine.transform.position = new Vector3(0.0f, currentTowerHeight, 0.0f);
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
        // 타워 높이 갱신
        // 기믹 활용을 위한 최상단 블럭 갱신
        currentTowerHeight = -9999.0f;
        
        foreach (var block in blockList)
        {
            float height = block.GetComponent<Collider2D>().bounds.max.y;

            //block.GetComponent<SpriteRenderer>().color = Color.white; // 임시
            
            if (height > currentTowerHeight)
            {
                currentTowerHeight = height;
                highestBlock = block;
                
                //block.GetComponent<SpriteRenderer>().color = Color.blue; // 임시
            }
        }
    }

    void CheckTowerHeight()
    {
        // 목표 위치에 도달하면 게임 클리어
        if (currentTowerHeight > goalTowerHeight)
        {
            GameManager gameManager = GameObject.FindFirstObjectByType<GameManager>();
            gameManager.isWin = true;
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

    [SerializeField] GameObject towerHeightLine;
    float nextTurnTime = 0.5f;

    public void CreateBlock()
    {
        // 광물 생성 및 드롭
        DropButton.gameObject.SetActive(false);

        GameObject newBlock = Instantiate(BlockPrefab);

        newBlock.transform.position = new Vector3(
            blockSpawnPoint.transform.position.x,
            blockSpawnPoint.transform.position.y,
            blockSpawnPoint.transform.position.z
            );

        newBlockList.Add(newBlock);

        StartCoroutine(WaitAndShowButton());
    }

    void AddBlock()
    {
        if (newBlockList.Count > 0)
        {
            blockList.Add(newBlockList[0]);
            newBlockList.RemoveAt(0);
        }
    }

    IEnumerator WaitAndShowButton()
    {
        yield return new WaitForSeconds(nextTurnTime);

        DropButton.gameObject.SetActive(true);
    }
    #endregion 
}
