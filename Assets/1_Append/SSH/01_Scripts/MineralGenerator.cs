using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MineralGenerator : MonoBehaviour
{
    [SerializeField] GameObject prefab_Mineral;
    Vector2 GEN_POS = new Vector2(0, 4f);

    const float STONE_PROB = 0.40f;
    const float COPPER_PROB = 0.25f;
    const float SILVER_PROB = 0.15f;
    const float GOLD_PROB = 0.05f;

    public async void GenerateRandomMineral()
    {
        float n = Random.Range(0f, 1f);

        if (n <= GOLD_PROB)
            await GenerateMineralAsync(MineralTypeEnum.Gold);
        else if (n <= GOLD_PROB + SILVER_PROB)
            await GenerateMineralAsync(MineralTypeEnum.Silver);
        else if (n <= GOLD_PROB + SILVER_PROB + COPPER_PROB)
            await GenerateMineralAsync(MineralTypeEnum.Copper);
        else
            await GenerateMineralAsync(MineralTypeEnum.Stone);
    }

    async Task GenerateMineralAsync(MineralTypeEnum type)
    {
        // 스프라이트의 어드레스 설정
        StringBuilder address = new();
        address.Append($"Sprite_{type.ToString()}");
        switch (type)
        {
            case MineralTypeEnum.Stone:
                address.Append($"_{Random.Range(1, 4)}"); // TODO : 4 -> 11
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

        // 어드레서블로 게임오브젝트 불러오기
        AsyncOperationHandle<Sprite> spriteLoadHandle = Addressables.LoadAssetAsync<Sprite>("Sprite_Stone_1"); // address.ToString()
        Sprite sprite = await spriteLoadHandle.Task;

        // 오브젝트 생성
        GameObject prefab = Instantiate(prefab_Mineral);
        prefab.transform.position = GEN_POS;
        prefab.GetComponent<SpriteRenderer>().sprite = sprite;
    }
}

