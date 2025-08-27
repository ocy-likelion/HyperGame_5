using DG.Tweening;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Linq;

public class SpawnBlockManager : MonoBehaviour
{
    // 상수
    private const float STONE_PROB = 0.05f;
    private const float COPPER_PROB = 0.45f;
    private const float SILVER_PROB = 0.35f;
    private const float GOLD_PROB = 0.15f;

    // 씬 오브젝트
    [Header("씬 오브젝트")]
    [SerializeField] private Transform topBlockObjectParent;
    [SerializeField] private FallingProxyBlockObjectPool fallingProxyBlockObjectPool;
    [SerializeField] private EffectObjectPool effectObjectPool;

    // 프리팹
    [Header("프리팹")]
    [SerializeField] private GameObject prefab_FallingProxyBlock;

    // private 필드(컴포넌트)
    private PlayManager playManager;

    // private 필드
    private readonly Vector3[] rots = { new Vector3(0, 0, 0), new Vector3(0, 0, 90), new Vector3(0, 0, 180), new Vector3(0, 0, 270) };

    // public Getter
    public Transform TopBlockObjectParent => topBlockObjectParent;

    // 유니티 콜백
    private void Awake()
    {
        TryGetComponent(out playManager);
    }

    // 불럭 생성
    public void SpawnRandomBlock() // 무작위 블럭 생성
    {
        float prob = Random.Range(0f, 1f);
        MineralTypeEnum type;

        // 확률에 따른 타입 할당
        if (prob <= GOLD_PROB)
        {
            type = MineralTypeEnum.Gold;
        }
        else if (prob <= GOLD_PROB + SILVER_PROB)
        {
            type = MineralTypeEnum.Silver;
        }
        else if (prob <= GOLD_PROB + SILVER_PROB + COPPER_PROB)
        {
            type = MineralTypeEnum.Copper;
        }
        else
        {
            type = MineralTypeEnum.Stone;
        }

        CreateBlockObjectAsync(type);
    }
    private void CreateBlockObjectAsync(MineralTypeEnum type) // 비동기로 블럭의 스프라이트를 불러오고 오브젝트도 생성
    {
        StringBuilder address = new StringBuilder($"Sprite_{type.ToString()}"); // 불러올 스프라이트 어드레스

        // 타입에 따른 스프라이트 설정
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

        // 비동기로 스프라이트 로드
        AsyncOperationHandle<Sprite> spriteLoadHandle = Addressables.LoadAssetAsync<Sprite>(address.ToString());
        spriteLoadHandle.Completed += (opHandle) => OnSpriteLoadCompleted(opHandle);
    }
    private void OnSpriteLoadCompleted(AsyncOperationHandle<Sprite> opHandle) // 블럭 스프라이트 로드 이벤트 메서드
    {
        if (opHandle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject proxyBlock = fallingProxyBlockObjectPool.Get(); // 떨어뜨릴 블럭 오브젝트를 풀에서 가져오기

            proxyBlock.GetComponent<FallingProxyBlockObject>().InitFallingProxyBlock(this, fallingProxyBlockObjectPool, effectObjectPool); // 블럭 오브젝트 초기화
            EventBus.Instance.Publish("SpawnBlock", proxyBlock); // 블럭의 위치를 설정
            proxyBlock.transform.eulerAngles = GetRandomRotation(); // 0, 90, 180, 270도 회전 중 무작위로 설정
            proxyBlock.transform.SetParent(topBlockObjectParent); // 부모 오브젝트 설정

            proxyBlock.GetComponent<SpriteRenderer>().sprite = opHandle.Result; // 스프라이트 할당
            proxyBlock.GetComponent<SpriteOutlineCollider>().BuildCollider(); // 콜라이더 생성

            GameObject go = proxyBlock.GetComponent<FallingProxyBlockObject>().InstantiateTopBlock();
            playManager.BlockList.Add(go); // 쌓이는 블럭 오브젝트를 PlayManager의 BlockList에 추가
        }
        else
        {
            Debug.LogError("로드할 스프라이트가 어드레서블에 없습니다.");
        }
    }

    // Etc
    private Vector3 GetRandomRotation() // 0, 90, 180, 270도 중 랜덤한 값 반환
    {
        int randomIndex = UnityEngine.Random.Range(0, rots.Length);
        return rots[randomIndex];
    }
}

public enum MineralTypeEnum
{
    None, Stone, Copper, Silver, Gold
}
