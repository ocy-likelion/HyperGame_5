using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GimmickManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TowerManager towerManager;
    [SerializeField] CameraShaker camShaker;

    [Header("FX Prefabs (Optional)")]
    [Tooltip("바람 아이콘/이펙트 프리팹 (SpriteRenderer/Particle System 등)")]
    public GameObject windFxPrefab;
    [Tooltip("기반 흔들림 아이콘/이펙트 프리팹")]
    public GameObject shakeFxPrefab;
    [Tooltip("상단 타격 아이콘/이펙트 프리팹")]
    public GameObject hitFxPrefab;

    [Header("Strength Tweaks")]
    public float shakeIntensity = 1.5f;     // 기반 흔들림 세기 (기존 0.5 → 1.5)
    public float shakeDuration  = 1.2f;
    public int   topHitCount    = 3;
    public float topHitForce    = 12f;      // 상단 타격 힘 (기존 5 → 12)
    public float windForce      = 20f;      // 바람 힘 (기존 2 → 20)
    public float windDuration   = 3f;

    [Header("Auto Trigger")]
    public float intervalSeconds = 15f;     // 15초마다 자동 발동

    private void Awake()
    {
        if (towerManager == null)
            towerManager = FindFirstObjectByType<TowerManager>();
    }

    private void Start()
    {
        // 씬 시작 시 15초 간격 자동 발동
        StartAutoGimmicks(intervalSeconds);
    }

    // ─────────────────────────────────────────────────────────
    // 1. 기반 흔들림 (시각화: 화면 좌상단 근처에 아이콘 생성)
    public void TriggerBaseShake()
    {
        StartCoroutine(BaseShakeRoutine(shakeIntensity, shakeDuration));
        SpawnCornerFx(shakeFxPrefab, new Vector2(0.12f, 0.88f), 1.2f);
        Debug.Log("<color=yellow>[Gimmick]</color> Base Shake!");
    }

    private IEnumerator BaseShakeRoutine(float intensity, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            var list = towerManager != null ? towerManager.GetAllBlocks() : null;
            if (list != null)
            {
                foreach (var block in list)
                {
                    if (block == null) continue;
                    block.transform.position += (Vector3)Random.insideUnitCircle * intensity * Time.deltaTime;
                }
            }
            time += Time.deltaTime;
            yield return null;
        }
    }

    // ─────────────────────────────────────────────────────────
    // 2. 최상단 블록 n개 타격 (시각화: 각 타격 지점 위에 아이콘 생성)
    public void TriggerTopHit()
    {
        var topBlocks = towerManager != null ? towerManager.GetTopBlocks(topHitCount) : null;
        if (topBlocks == null) return;

        foreach (var block in topBlocks)
        {
            if (block == null) continue;
            var rb = block.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = new Vector2(Random.Range(0, 2) == 0 ? -1f : 1f, 0.2f); // 좌/우 + 약간 위로
                rb.AddForce(dir.normalized * topHitForce, ForceMode2D.Impulse);

                // 시각효과: 타격 위치 위에 FX
                if (hitFxPrefab != null)
                {
                    var fx = Instantiate(hitFxPrefab, block.transform.position + Vector3.up * 0.8f, Quaternion.identity);
                    Destroy(fx, 1.0f);
                }

                // 디버그 레이
                Debug.DrawRay(block.transform.position, (Vector3)dir.normalized * 2f, Color.red, 1.0f);
            }
        }

        Debug.Log("<color=orange>[Gimmick]</color> Top Hit x" + topHitCount);
    }

    // ─────────────────────────────────────────────────────────
    // 3. 천연가스 바람 (지속 힘, 시각화: 우상단 근처에 바람 아이콘 + 디버그 레이)
    public void TriggerWind()
    {
        StartCoroutine(WindRoutine(windForce, windDuration));
        SpawnCornerFx(windFxPrefab, new Vector2(0.88f, 0.88f), windDuration);
        Debug.Log("<color=cyan>[Gimmick]</color> Wind!");
    }

    private IEnumerator WindRoutine(float force, float duration)
    {
        float time = 0f;

        // 랜덤 방향 (좌/우)
        int lr = Random.Range(0, 2) == 0 ? -1 : 1;
        Vector2 windDir = new Vector2(lr, 0f);

        // 디버그 레이 (카메라 중앙에서 바람 방향 표시)
        if (Camera.main != null)
        {
            Vector3 origin = Camera.main.transform.position;
            origin.z = 0f;
            Debug.DrawRay(origin, (Vector3)windDir.normalized * 5f, Color.cyan, duration);
        }

        while (time < duration)
        {
            var list = towerManager != null ? towerManager.GetAllBlocks() : null;
            if (list != null)
            {
                foreach (var block in list)
                {
                    if (block == null) continue;
                    var rb = block.GetComponent<Rigidbody2D>();
                    if (rb != null)
                        rb.AddForce(windDir * force * Time.deltaTime, ForceMode2D.Force);
                }
            }
            time += Time.deltaTime;
            yield return null;
        }
    }

    // ─────────────────────────────────────────────────────────
    // 랜덤 기믹 1개 실행 (가시성 있는 버전)
    public void TriggerRandomGimmick()
    {
        int idx = Random.Range(0, 3); // 0~2
        switch (idx)
        {
            case 0: TriggerBaseShake(); break;
            case 1: TriggerTopHit();    break;
            case 2: TriggerWind();      break;
        }
    }

    // ─────────────────────────────────────────────────────────
    // 자동 랜덤 기믹 루프 (15초 기본)
    private Coroutine autoRoutine;

    public void StartAutoGimmicks(float intervalSec)
    {
        if (autoRoutine != null) StopCoroutine(autoRoutine);
        autoRoutine = StartCoroutine(AutoGimmickLoop(intervalSec));
    }

    public void StopAutoGimmicks()
    {
        if (autoRoutine != null)
        {
            StopCoroutine(autoRoutine);
            autoRoutine = null;
        }
    }

    private IEnumerator AutoGimmickLoop(float intervalSec)
    {
        while (true)
        {
            // 게임이 Playing이 아닐 땐 대기
            while (GameManager.Instance != null &&
                   GameManager.Instance.CurrentState != GameState.Playing)
            {
                yield return null;
            }

            yield return new WaitForSeconds(intervalSec);

            if (GameManager.Instance == null ||
                GameManager.Instance.CurrentState != GameState.Playing)
                continue;

            TriggerRandomGimmick();
        }
    }

    // ─────────────────────────────────────────────────────────
    // 화면 코너에 간단 FX 아이콘 생성 (WorldSpace)
    // viewportPos: (0~1, 0~1) 화면 비율 좌표. 예) (0.1, 0.9) = 좌상단 근처
    private void SpawnCornerFx(GameObject prefab, Vector2 viewportPos, float life = 1.5f)
    {
        if (prefab == null || Camera.main == null) return;
        Vector3 world = Camera.main.ViewportToWorldPoint(new Vector3(viewportPos.x, viewportPos.y, 10f));
        var fx = Instantiate(prefab, world, Quaternion.identity);
        Destroy(fx, life);
    }
    
    // 테스트 씬용
    public void TriggerBaseShake(float intensity, float duration)
    {
        StartCoroutine(BaseShakeRoutine(intensity, duration));
        if (camShaker) camShaker.Shake(0.3f, 0.5f); // 기반 흔들릴 때 화면도 덜컥
    }

    public void TriggerTopHit(int topCount, float force)
    {
        var topBlocks = towerManager.GetTopBlocks(topCount);
        foreach (var block in topBlocks)
        {
            var rb = block.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.AddForce(Vector2.right * Random.Range(-1f, 1f) * force, ForceMode2D.Impulse);
        }
        if (camShaker) camShaker.Shake(0.25f, 0.3f); // 위에서 맞은 느낌
    }

    public void TriggerWind(float force, float duration)
    {
        StartCoroutine(WindRoutine(force, duration));
        if (camShaker) camShaker.Shake(0.15f, 0.25f); // 바람 불 때 살짝 흔들
    }
}
