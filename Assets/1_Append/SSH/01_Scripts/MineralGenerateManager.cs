using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MineralGenerateManager : MonoBehaviour
{
    [Header("프리팹")]
    [SerializeField] GameObject prefab_Mineral;

    [Header("주요 프로퍼티")]
    readonly Vector2 GEN_POS = new Vector2(0, 4f); // 광물 생성 지점

    // 각 광물의 등장 확률
    const float STONE_PROB = 0.75f;
    const float COPPER_PROB = 0.20f;
    const float SILVER_PROB = 0.10f;
    const float GOLD_PROB = 0.05f;

    public async void GenerateRandomMineral() // 확률에 따라 무작위 광물을 생성하는 메서드
    {
        // 확률 난수 생성
        float n = Random.Range(0f, 1f);

        // 확률에 따라 광물 프리팹 생성
        if (n <= GOLD_PROB)
        {
            await GenerateMineralAsync(MineralTypeEnum.Gold);
        }
        else if (n <= GOLD_PROB + SILVER_PROB)
        {
            await GenerateMineralAsync(MineralTypeEnum.Silver);
        }
        else if (n <= GOLD_PROB + SILVER_PROB + COPPER_PROB)
        {
            await GenerateMineralAsync(MineralTypeEnum.Copper);
        }
        else
        {
            await GenerateMineralAsync(MineralTypeEnum.Stone);
        }
    }

    async Task GenerateMineralAsync(MineralTypeEnum type) // 광물 종류에 따라 프리팹을 생성하는 메서드
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
            GameObject prefab = Instantiate(prefab_Mineral);
            prefab.transform.position = GEN_POS;

            // 스프라이트 할당
            prefab.GetComponent<SpriteRenderer>().sprite = opHandle.Result;
        }
    }
}

