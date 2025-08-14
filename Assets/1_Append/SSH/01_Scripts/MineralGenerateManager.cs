using DG.Tweening;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MineralGenerateManager : MonoBehaviour
{
    [Header("컴포넌트")]
    SabotageEventManager sabotageEventManager;

    [Header("씬 오브젝트")]
    [SerializeField] GameObject prefab_Proxy;

    [Header("주요 프로퍼티")]
    readonly Vector2 GEN_POS = new Vector2(0, 5f); // 광물 생성 지점
    int mineralCount = 0; // 생성한 광물 개수

    // 각 광물의 등장 확률
    const float STONE_PROB = 0.75f;
    const float COPPER_PROB = 0.20f;
    const float SILVER_PROB = 0.10f;
    const float GOLD_PROB = 0.05f;

    void Awake()
    {
        sabotageEventManager = GetComponent<SabotageEventManager>();

        mineralCount = 0;
    }

    public void GenerateRandomMineral() // 확률에 따라 무작위 광물을 생성하는 메서드
    {
        // 확률 난수 생성
        float n = Random.Range(0f, 1f);

        // 확률에 따라 광물 프리팹 생성
        if (n <= GOLD_PROB)
        {
            GenerateMineralAsync(MineralTypeEnum.Gold);
        }
        else if (n <= GOLD_PROB + SILVER_PROB)
        {
            GenerateMineralAsync(MineralTypeEnum.Silver);
        }
        else if (n <= GOLD_PROB + SILVER_PROB + COPPER_PROB)
        {
            GenerateMineralAsync(MineralTypeEnum.Copper);
        }
        else
        {
            GenerateMineralAsync(MineralTypeEnum.Stone);
        }

        // 광물 개수 추가
        mineralCount++;

        // 이벤트 생성
        sabotageEventManager.EventCheckByMineralCount(mineralCount);
    }

    void GenerateMineralAsync(MineralTypeEnum type) // 광물 종류에 따라 프리팹을 생성하는 메서드
    {
        // type에 따라 각기 다른 스프라이트 어드레스 설정
        StringBuilder address = new();
        address.Append($"Sprite_{type.ToString()}");
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

        // 어드레서블로 스프라이트 불러오기
        AsyncOperationHandle<Sprite> spriteLoadHandle = Addressables.LoadAssetAsync<Sprite>(address.ToString()); // address.ToString()
        spriteLoadHandle.Completed += OnSpriteLoadCompleted;
    }

    void OnSpriteLoadCompleted(AsyncOperationHandle<Sprite> opHandle) // 스프라이트 불러오기 성공 시 프리팹을 생성하는 메서드
    {
        if (opHandle.Status == AsyncOperationStatus.Succeeded)
        {
            // 오브젝트 생성
            prefab_Proxy.SetActive(true);
            prefab_Proxy.transform.position = GEN_POS;

            //// 블럭 크게 키우기
            //if (mineralCount % 4 == 0)
            //{
            //    prefab_Proxy.transform.DOScale(prefab_Proxy.transform.localScale * 1.5f, 0.2f).SetEase(Ease.OutQuart);
            //}

            // 스프라이트 할당
            prefab_Proxy.GetComponent<SpriteRenderer>().sprite = opHandle.Result;

            // 콜라이더 생성
            prefab_Proxy.GetComponent<SpriteOutlineCollider>().BuildCollider();
        }
    }
}

