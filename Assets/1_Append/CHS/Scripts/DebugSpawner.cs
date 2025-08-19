// Assets/1_Append/CHS/Scripts/DebugSpawner.cs
using UnityEngine;

public class DebugSpawner : MonoBehaviour
{
    [Header("Spawn")]
    public Vector2 spawnPos = new Vector2(0, 4f);
    public float spawnInterval = 1.0f;
    public int maxBlocks = 20;

    [Header("Block visuals/physics")]
    public Vector2 spriteSize = new Vector2(1f, 1f); // 눈에 보이는 크기
    public Color colorMin = new Color(0.2f, 0.7f, 1f);
    public Color colorMax = new Color(1f, 0.6f, 0.2f);
    public float gravityScale = 1f;

    [Header("Optional")]
    public TowerManager towerManager; // 있으면 연결

    float timer;
    int spawned;
    static Sprite _runtimeSprite; // 1x1 텍스처로 만든 런타임 스프라이트

    void OnEnable()
    {
        // 1x1 텍스처 → 스프라이트를 런타임으로 생성 (프로젝트에 아무 리소스가 없어도 보임!)
        if (_runtimeSprite == null)
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _runtimeSprite = Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 100f);
        }
        Debug.Log("[DebugSpawner] Ready. Will spawn blocks.");
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (spawned < maxBlocks && timer >= spawnInterval)
        {
            timer = 0f;
            SpawnOne();
        }
    }

    void SpawnOne()
    {
        var go = new GameObject("DebugBlock_" + spawned);
        go.transform.position = spawnPos;

        // 보이게 만들기
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = _runtimeSprite;           // 투명 아님!
        sr.sortingOrder = 10;
        sr.color = Color.Lerp(colorMin, colorMax, Random.value);
        go.transform.localScale = new Vector3(spriteSize.x, spriteSize.y, 1f);

        // 충돌/물리
        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;               // 스케일로 크기 결정되니 1로 두기

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // TowerManager에 등록(있을 때만)
        if (towerManager != null)
            towerManager.RegisterBlock(go);

        spawned++;
        Debug.Log($"[DebugSpawner] Spawned {go.name} at {spawnPos}");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(spawnPos, spriteSize);
    }
}
