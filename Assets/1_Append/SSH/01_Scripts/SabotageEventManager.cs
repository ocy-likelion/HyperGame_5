using DG.Tweening;
using System.Collections;
using UnityEngine;

public class SabotageEventManager : MonoBehaviour
{
    [Header("컴포넌트")]
    PlayManager playManager;

    [Header("프리팹")]
    [SerializeField] GameObject prefab_Mole;

    [Header("씬 오브젝트")]
    [SerializeField] GameObject platform;
    [SerializeField] GameObject deadLine;
    [SerializeField] GameObject lava;
    [SerializeField] BlockController blockController;

    [Header("주요 프로퍼티")]
    // 싱크홀 관련
    readonly Vector2 SINKHOLE_POS = new Vector2(0, -1f);
    const float SINKHOLE_DURATION = 0.25f;
    const float SHAKE_CAMERA_AMOUNT = 1.2f;
    bool isTriggeredSinkHole;
    // 특수 블럭 관련
    readonly Vector2 MOLE_GEN_POS = new Vector2(0, 6f);
    readonly Vector2 FEATHER_GEN_POS = new Vector2(0, 6.5f);
    bool isTriggeredMole;
    // 용암 관련
    readonly Vector3 LAVA_START_POS = new Vector3(0, -12f, -0.1f);
    Vector3 LAVA_END_POS;
    const float LAVA_DURATION = 30f;

    void Awake()
    {
        TryGetComponent<PlayManager>(out playManager);
    }
    void Start()
    {
        LAVA_END_POS = new Vector3(0, playManager.GOAL_TOWER_HEIGHT - 5, -0.1f); // -5를 하는 이유 : 용암 경계선을 맞추기 위한 오프셋 값

        StartCoroutine(SurgeLavaCoroutine());
    }

    public void ResetAllProperties() // 모든 방해 이벤트 트리거 불리언을 초기화
    {
        isTriggeredSinkHole = false;
        isTriggeredMole = false;
    }
    public void TriggerSinkHoleEvent() // 싱크홀 이벤트 메서드
    {
        if (isTriggeredSinkHole) return;

        platform.transform.DOMove((Vector2)platform.transform.position + SINKHOLE_POS, SINKHOLE_DURATION);
        deadLine.transform.DOMove((Vector2)deadLine.transform.position + SINKHOLE_POS, SINKHOLE_DURATION);
        ShakeCamera(SINKHOLE_DURATION, SHAKE_CAMERA_AMOUNT);
        isTriggeredSinkHole = true;
    }
    public void TriggerMoleEvent() // 두더지 이벤트 메서드
    {
        if (isTriggeredMole) return;

        GameObject go = Instantiate(prefab_Mole);
        go.transform.position = blockController.GetBlockSpawnPos() + new Vector3(Random.Range(-2f, 2f), 0, 0);
        isTriggeredMole = true;
    }
    public void ShakeCamera(float duration, float strength) // 카메라 쉐이킹 메서드
    {
        Camera.main.transform.DOShakePosition(duration, strength, 10, 90f, false, true);
    }
    IEnumerator SurgeLavaCoroutine() // 용암이 차오르는 메서드
    {
        // 경과한 시간
        float elapsed = 0f; 

        // 용암이 움직이는 메서드
        while (elapsed < LAVA_DURATION)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / LAVA_DURATION);
            lava.transform.position = Vector3.Lerp(LAVA_START_POS, LAVA_END_POS, t);

            if (lava.transform.position.y + 5 > playManager.currentTowerHeight && playManager.HasActiveBlock() && playManager.currentTowerHeight > -3) // + 5는 오프셋값
            {
                Debug.Log("용암이 블럭을 따라잡음");
            }
            yield return null;
        }

        // 용암의 최종 도착
        lava.transform.position = LAVA_END_POS;
    }
}
