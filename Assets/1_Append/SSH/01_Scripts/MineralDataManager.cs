using DG.Tweening;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Linq;

public class MineralDataManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform topParent;
    //private SabotageEventManager sabotageEventManager;
    private PlayManager playManager;

    [Header("Prefabs")]
    [SerializeField] private GameObject prefab_BlockDropProxy;
    [SerializeField] ProxyObjectPool blockDropProxyPool;
    [SerializeField] EffectObjectPool effectObjectPool;

    [Header("Properties")]
    private readonly Vector2 GEN_POS = new Vector2(0, 5f); // Mineral generation point
    private int mineralCount = 0; // Generated mineral count
    private float x = 0; // Horizontal position control
    private List<GameObject> ingameBlockList = new List<GameObject>(); // List of active blocks

    // Mineral appearance probabilities
    private const float STONE_PROB = 0.05f;
    private const float COPPER_PROB = 0.45f;
    private const float SILVER_PROB = 0.35f;
    private const float GOLD_PROB = 0.15f;

    private void Awake()
    {
        mineralCount = 0;
        //sabotageEventManager = GetComponent<SabotageEventManager>();
        playManager = GetComponent<PlayManager>();
    }

    public void GenerateRandomMineral()
    {
        float n = Random.Range(0f, 1f);
        MineralTypeEnum type;

        // Determine mineral type based on probability
        if (n <= GOLD_PROB)
        {
            type = MineralTypeEnum.Gold;
        }
        else if (n <= GOLD_PROB + SILVER_PROB)
        {
            type = MineralTypeEnum.Silver;
        }
        else if (n <= GOLD_PROB + SILVER_PROB + COPPER_PROB)
        {
            type = MineralTypeEnum.Copper;
        }
        else
        {
            type = MineralTypeEnum.Stone;
        }

        GenerateMineralAsync(type);
        mineralCount++;
        //sabotageEventManager.EventCheckByMineralCount(mineralCount);
    }

    private void GenerateMineralAsync(MineralTypeEnum type)
    {
        // Set sprite address based on mineral type
        StringBuilder address = new StringBuilder($"Sprite_{type.ToString()}");
        switch (type)
        {
            case MineralTypeEnum.Stone:
                address.Append($"_{Random.Range(1, 5)}");
                break;
            case MineralTypeEnum.Copper:
                address.Append($"_{Random.Range(1, 4)}");
                break;
            case MineralTypeEnum.Silver:
                address.Append($"_{Random.Range(1, 3)}");
                break;
            case MineralTypeEnum.Gold:
                address.Append($"_1");
                break;
        }

        // Load sprite asynchronously
        AsyncOperationHandle<Sprite> spriteLoadHandle = Addressables.LoadAssetAsync<Sprite>(address.ToString());
        spriteLoadHandle.Completed += (opHandle) => OnSpriteLoadCompleted(opHandle);
    }

    private void OnSpriteLoadCompleted(AsyncOperationHandle<Sprite> opHandle)
    {
        if (opHandle.Status == AsyncOperationStatus.Succeeded)
        {
            // Calculate max Y position from existing blocks
            float maxY = 0;
            foreach (var block in ingameBlockList)
            {
                if (block == null) continue;

                float temp = block.transform.position.y;
                maxY = Mathf.Max(temp, maxY);
            }

            float tempY = maxY + 10f;
            Vector3 spawnPosition = new Vector3(x, tempY, 0);

            // Instantiate prefab
            GameObject proxyBlock = blockDropProxyPool.Get();

            proxyBlock.GetComponent<BlockDropProxy>().InstantiateProxyObject(this, blockDropProxyPool, effectObjectPool);
            proxyBlock.transform.position = spawnPosition;
            proxyBlock.transform.eulerAngles = GetRandomRotation();
            proxyBlock.transform.SetParent(topParent);
            proxyBlock.GetComponent<SpriteRenderer>().sprite = opHandle.Result;
            proxyBlock.GetComponent<SpriteOutlineCollider>().BuildCollider();
            playManager.blockList.Add(proxyBlock.GetComponent<BlockDropProxy>().InstantiateTopObject()); // 탑 오브젝트를 바로 PlayManager의 BlockList에 넣기
            EventBus.Instance.Publish("SpawnBlock", proxyBlock); // 프록시 오브젝트를 이벤트 버스로 퍼블리시
        }
    }

    public void StartSlidingToObject()
    {
        if (ingameBlockList.Count > 0)
        {
            var lastBlock = ingameBlockList[ingameBlockList.Count - 1];
            var blockTop = lastBlock.GetComponent<BlockOnlyTop>();
            if (blockTop != null)
            {
                blockTop.ApplySlideMotion();
            }
        }
    }

    public void AddLastBlock(GameObject _object)
    {
        if (ingameBlockList.Contains(_object)) return;

        ingameBlockList.Add(_object);
    }

    public void RightArrowButton()
    {
        x += 1f;
    }

    public void LeftArrowButton()
    {
        x -= 1f;
    }

    public Transform GetParentTopObject()
    {
        return topParent;
    }

    Vector3[] rots = { new Vector3(0, 0, 0), new Vector3(0, 0, 90), new Vector3(0, 0, 180), new Vector3(0, 0, 270) };
    Vector3 GetRandomRotation()
    {
        int randomIndex = UnityEngine.Random.Range(0, rots.Length);
        return rots[randomIndex];
    }
}
