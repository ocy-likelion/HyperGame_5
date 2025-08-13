using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayManager : MonoBehaviour
{
    public Button DropButton;

    public GameObject BlockPrefab;
    List<GameObject> blockList = new List<GameObject>();
    GameObject highestBlock;

    public GameObject blockSpawnPoint;
    float blockSpawnPointFreqeuncy = 1.5f;

    float currentTowerHeight;
    float goalTowerHeight = 10.0f; // 임시

    float totalElapsedTime = 0.0f;

    void Start()
    {
        StartCoroutine("GameTimer");
    }

    void Update()
    {
        #region 임시 blockSpawnPoint 이동, 드래그 앤 드롭으로 고쳐야 함
        blockSpawnPoint.transform.position = new Vector3(Mathf.Sin(Time.time * blockSpawnPointFreqeuncy), 4.0f, 0.0f);
        #endregion

        CheckHighestBlock();
        CheckTowerHeight();  // 블럭을 막 생성했을 때의 위치로 타워 높이가 갱신되는 문제

        Debug.Log(currentTowerHeight);
    }

    IEnumerator GameTimer()
    {
        totalElapsedTime += Time.deltaTime;
        yield return null;
    }

    void CheckHighestBlock()
    {
        // 기믹 활용을 위한 최상단 블럭 갱신
        foreach (var block in blockList)
        {
            float height = block.GetComponent<Collider2D>().bounds.max.y;
            if (height > currentTowerHeight)
            {
                currentTowerHeight = height;
                highestBlock = block;
            }
        }
    }

    void CheckTowerHeight()
    {
        // 타워 높이 갱신
        if (currentTowerHeight > goalTowerHeight)
        {
            EventBus.Instance.Publish(Consts.GAMECLEAR);
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
        blockList.Add(newBlock);

        StartCoroutine("WaitAndShowButton");
    }

    IEnumerator WaitAndShowButton()
    {
        yield return new WaitForSeconds(nextTurnTime);

        DropButton.gameObject.SetActive(true);
    }
    #endregion 
}
