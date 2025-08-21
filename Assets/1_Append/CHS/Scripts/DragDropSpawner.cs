using UnityEngine;

public class DragDropSpawner : MonoBehaviour
{
    [Header("Spawn")]
    public DraggableBlock blockPrefab;      // 드래그·드랍 가능한 프리팹(필수)
    public Transform spawnPoint;            // 생성 위치(없으면 스포너 위치 사용)
    public float nextSpawnDelay = 0.2f;     // 드랍 후 다음 블록 생성 지연(안정화)

    [Header("Bounds (optional)")]
    public float minX = -4.5f;
    public float maxX =  4.5f;
    public float clampY =  4.0f;

    DraggableBlock _current;
    float _delayTimer;
    bool _awaitNext;

    void Start()
    {
        TrySpawn();
    }

    void Update()
    {
        // 드랍 완료 후 잠깐 대기 → 다음 스폰
        if (_awaitNext)
        {
            _delayTimer -= Time.unscaledDeltaTime;
            if (_delayTimer <= 0f)
            {
                _awaitNext = false;
                TrySpawn();
            }
        }

        // 프리뷰 중일 때 화면 밖으로 마우스가 나가면 살짝 위치 구속(선택)
        if (_current && _current.IsPreview)
        {
            var cam = Camera.main;
            var m = Input.mousePosition;
            var w = cam ? cam.ScreenToWorldPoint(m) : Vector3.zero;

            // 가로/세로 클램프
            w.x = Mathf.Clamp(w.x, minX, maxX);
            w.y = Mathf.Min(w.y, clampY);
            w.z = 0f;
            // DraggableBlock이 Update에서 MovePosition하므로 여기선 참고만
        }
    }

    void TrySpawn()
    {
        if (_current != null) return;
        if (!blockPrefab)
        {
            Debug.LogError("[DragDropSpawner] blockPrefab이 비었어요. 프리팹을 지정하세요.");
            return;
        }

        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
        var block = Instantiate(blockPrefab, pos, Quaternion.identity);
        block.OnDropped += HandleDropped;

        // 혹시 SortingOrder가 너무 낮아 안 보이면 약간 올려주기(선택)
        var sr = block.GetComponent<SpriteRenderer>();
        if (sr) sr.sortingOrder = 10;

        _current = block;
    }

    void HandleDropped(DraggableBlock dropped)
    {
        if (_current == dropped)
        {
            _current = null;
        }

        // 다음 생성 예약
        _awaitNext = true;
        _delayTimer = nextSpawnDelay;
    }
}
