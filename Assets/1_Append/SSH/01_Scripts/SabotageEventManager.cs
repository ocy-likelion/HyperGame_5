using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class SabotageEventManager : MonoBehaviour
{
    [Header("컴포넌트")]
    PlayManager playManager;

    [Header("프리팹")]
    [SerializeField] GameObject prefab_Mole;

    [Header("씬 오브젝트")]
    [SerializeField] GameObject Text_SabotageAlarm;
    [SerializeField] BlockController blockController;
    [SerializeField] Camera mainCam;
    [SerializeField] GameObject ground;
    [SerializeField] GameObject lava;

    [Header("주요 프로퍼티")]
    // 두더지 관련
    readonly Vector2 MOLE_GEN_POS = new Vector2(0, 6f);
    readonly Vector2 FEATHER_GEN_POS = new Vector2(0, 6.5f);
    const float MOLE_TIMING = 7f;
    bool isTriggeredMole = false;
    // 싱크홀 관련
    readonly Vector2 SINKHOLE_POS = new Vector2(0, -1f);
    const float SINKHOLE_DURATION = 0.25f;
    const float SINKHOLE_AMOUNT = 0.8f;
    const float SHAKE_CAMERA_AMOUNT = 1.2f;
    const float SINKHOLE_TIMING = 17f;
    bool isTriggeredSinkHole = false;
    // 용암 관련
    readonly Vector3 LAVA_START_POS = new Vector3(0, -12f, 0);
    Vector3 LAVA_END_POS;
    const float LAVA_DURATION = 30f;
    const int LAVA_OFFSET = 5; // 용암 위치 오프셋 값

    void Awake()
    {
        TryGetComponent<PlayManager>(out playManager);

        LAVA_END_POS = new Vector3(0, playManager.goalTowerHeight - LAVA_OFFSET, 0);
    }
    void Start()
    {
        ResetEventBoolean();
        StartSurgeLava();
    }
    void Update()
    {
        if (!isTriggeredMole && playManager.GetElaspedTime() > MOLE_TIMING)
        {
            TriggerMoleEvent();
        }
        if (!isTriggeredSinkHole && playManager.GetElaspedTime() > SINKHOLE_TIMING)
        {
            TriggerSinkHoleEvent();
        }
    }
    void ResetEventBoolean()
    {
        isTriggeredMole = false;
        isTriggeredSinkHole = false;
    }

    void TriggerMoleEvent() // 두더지 이벤트 메서드
    {
        if (!isTriggeredMole)
        {
            isTriggeredMole = true;

            TMP_Text sabotageText = Text_SabotageAlarm.GetComponent<TMP_Text>();
            sabotageText.text = "두더지 떼가 몰려옵니다..";
            Text_SabotageAlarm.SetActive(true);

            Sequence seq = DOTween.Sequence();

            seq.Append(sabotageText.DOFade(1f, 0.5f));

            seq.AppendInterval(2.5f);

            seq.Append(sabotageText.DOFade(0f, 0.5f)
                .OnComplete(() => Text_SabotageAlarm.SetActive(false)));

            seq.AppendCallback(() =>
            {
                for (int i = 0; i < 3; i++)
                {
                    GameObject go = Instantiate(prefab_Mole);
                    go.transform.position = blockController.GetBlockSpawnPoint()
                                            + new Vector3(Random.Range(-2f, 2f), Random.Range(1f, 3f), 0);
                    go.transform.eulerAngles = new Vector3(0, 0, Random.Range(0f, 180f));
                }
            });
        }
    }
    void TriggerSinkHoleEvent() // 싱크홀 이벤트 메서드
    {
        if (!isTriggeredSinkHole)
        {
            isTriggeredSinkHole = true;

            TMP_Text sabotageText = Text_SabotageAlarm.GetComponent<TMP_Text>();
            sabotageText.text = "곧 지진이 일어날 것 같습니다..!!!";
            Text_SabotageAlarm.SetActive(true);

            Vector3 originalPos = ground.transform.position;

            Sequence seq = DOTween.Sequence();
            seq.Append(sabotageText.DOFade(1f, 0.5f));
            seq.AppendInterval(2.5f);
            seq.Append(sabotageText.DOFade(0f, 0.5f).OnComplete(() => Text_SabotageAlarm.SetActive(false)));

            seq.AppendCallback(() =>
            {
                ground.transform.DOShakePosition(SINKHOLE_DURATION, SINKHOLE_AMOUNT, 10, 90, false, true)
                    .OnComplete(() => ground.transform.position = originalPos);
                ShakeCamera(SINKHOLE_DURATION, SHAKE_CAMERA_AMOUNT);
            });
        }
    }
    void ShakeCamera(float duration, float strength) // 카메라 쉐이킹 메서드
    {
        mainCam.transform.DOShakePosition(duration, strength, 10, 90f, false, true);
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

            if (lava.transform.position.y + 5 > playManager.currentTowerHeight && playManager.currentTowerHeight > -3 && playManager.HasActiveBlock())
            {
                Debug.Log("용암이 따라잡음!!");
            }

            yield return null;
        }

        // 용암의 최종 도착
        lava.transform.position = LAVA_END_POS;
    }

    public void StartSurgeLava()
    {
        StartCoroutine(SurgeLavaCoroutine());
    }
}
