using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class SabotageEventManager : MonoBehaviour
{
    // 프리팹
    [Header("프리팹")]
    [SerializeField] GameObject prefab_Mole;
    [SerializeField] GameObject prefab_BlockTopObject;

    // 씬 오브젝트
    [Header("씬 오브젝트")]
    [SerializeField] GameObject NaturalGasObj;
    [SerializeField] GameObject Text_SabotageAlarm;
    [SerializeField] BlockController blockController;
    [SerializeField] GameObject backGround;
    [SerializeField] GameObject ground;
    [SerializeField] GameObject lava;

    // 컴포넌트
    PlayManager playManager;

    // 필드
    Sequence eventSeq;
    // 두더지 관련
    const float MOLE_TIMING = 7f;
    bool isTriggeredMole = false;
    // 지진 관련
    readonly Vector2 EARTHQUAKE_POS = new Vector2(0, -1f);
    const float EARTHQUAKE_DURATION = 0.85f;
    const float EARTHQUAKE_AMOUNT = 0.35f;
    const float EARTHQUAKE_TIMING = 17f;
    bool isTriggeredEarthQuake = false;
    // 미니 두더쥐 관련
    const float MINIMOLE_TIMING = 27f;
    bool isTriggeredMiniMole = false;
    // 거대 두더지 관련
    const float MOLEKING_TIMING = 37f;
    bool isTriggeredMoleKing = false;
    // 천연가스 관련
    const float NATURALGAS_TIMING = 47f;
    bool isTriggeredNaturalGas = false;
    // 용암 관련
    readonly Vector3 LAVA_START_POS = new Vector3(0, -12f, 0);
    Vector3 LAVA_END_POS;
    const float LAVA_DURATION = 60f;
    const int LAVA_OFFSET = 5; // 용암 위치 오프셋 값

    void Awake()
    {
        DOTween.KillAll();
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
        if (!isTriggeredMole && playManager.GetElaspedTime() > MOLE_TIMING) // 10초 : 두더지 3마리
        {
            TriggerMoleEvent();
        }
        if (!isTriggeredEarthQuake && playManager.GetElaspedTime() > EARTHQUAKE_TIMING) // 20초 : 지진
        {
            TriggerEarthQuakeEvent();
        }
        if (!isTriggeredMiniMole && playManager.GetElaspedTime() > MINIMOLE_TIMING) // 30초 : 미니 두더지 9마리
        {
            TriggerMiniMoleEvent();
        }
        if (!isTriggeredMoleKing && playManager.GetElaspedTime() > MOLEKING_TIMING) // 40초 : 몰킹 1마리
        {
            TriggerMoleKingEvent();
        }
        if (!isTriggeredNaturalGas && playManager.GetElaspedTime() > NATURALGAS_TIMING) // 50초 : 천연가스
        {
            TriggerNaturalGasEvent();
        }
    }

    void ResetEventBoolean()
    {
        if (eventSeq != null && eventSeq.IsActive())
        {
            eventSeq.Kill(); // 진행 중인 Tween 종료
            eventSeq = null; // 참조 제거
        }

        isTriggeredMole = false;
        isTriggeredEarthQuake = false;
        isTriggeredMiniMole = false;
        isTriggeredMoleKing = false;
        isTriggeredNaturalGas = false;
    }
    void CommonSabotageEvent(string message, float displayTime, Action callback)
    {
        TMP_Text sabotageText = Text_SabotageAlarm.GetComponent<TMP_Text>();
        sabotageText.text = message;
        Text_SabotageAlarm.SetActive(true);

        eventSeq = DOTween.Sequence();
        eventSeq.Append(sabotageText.DOFade(1f, 0.5f));
        eventSeq.AppendInterval(displayTime);
        eventSeq.Append(sabotageText.DOFade(0f, 0.5f)
            .OnComplete(() => Text_SabotageAlarm.SetActive(false)));
        eventSeq.AppendCallback(new TweenCallback(() =>
        {
            callback?.Invoke(); // callback이 null이면 안전하게 무시
        }));
    }
    public void TriggerMoleEvent() // 기본 두더지 이벤트
    {
        if (!isTriggeredMole)
        {
            isTriggeredMole = true;
            CommonSabotageEvent("두더지 떼가 몰려옵니다..", 2.5f, () =>
            {
                for (int i = -1; i < 2; i++)
                {
                    GameObject go = Instantiate(prefab_Mole);
                    go.transform.position = blockController.GetBlockSpawnPoint() + new Vector3(i, UnityEngine.Random.Range(1f, 3f), 0);
                    go.GetComponent<SpriteOutlineCollider>().BuildCollider();

                    Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                    if (rb != null)
                        rb.AddTorque(UnityEngine.Random.Range(-10f, 10f), ForceMode2D.Impulse);
                }
            });
        }
    }
    public void TriggerMiniMoleEvent() // 작은 두더지 이벤트
    {
        if (!isTriggeredMiniMole)
        {
            isTriggeredMiniMole = true;
            CommonSabotageEvent("작은 두더지 떼가 몰려옵니다!", 2.5f, () =>
            {
                for (int i = -1; i < 2; i++)
                    for (int j = 3; j < 5; j++)
                    {
                        GameObject go = Instantiate(prefab_Mole);
                        go.transform.position = blockController.GetBlockSpawnPoint() + new Vector3(i, j, 0);
                        go.transform.localScale = Vector3.one * 0.5f;
                        go.GetComponent<SpriteOutlineCollider>().BuildCollider();

                        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                        if (rb != null)
                            rb.AddTorque(UnityEngine.Random.Range(-10f, 10f), ForceMode2D.Impulse);
                    }
            });
        }
    }
    public void TriggerMoleKingEvent() // 거대 두더지 이벤트
    {
        if (!isTriggeredMoleKing)
        {
            isTriggeredMoleKing = true;
            CommonSabotageEvent("거대한 두더지가 내려옵니다!", 2.5f, () =>
            {
                GameObject go = Instantiate(prefab_Mole);
                go.transform.position = blockController.GetBlockSpawnPoint() + new Vector3(0, 3, 0);
                go.transform.localScale = Vector3.one * 2.5f;
                go.GetComponent<SpriteOutlineCollider>().BuildCollider();

                Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.AddTorque(UnityEngine.Random.Range(-10f, 10f), ForceMode2D.Impulse);

                ShakeCamera(EARTHQUAKE_DURATION);
            });
        }
    }
    public void TriggerNaturalGasEvent() // 천연가스 이벤트
    {
        if (!isTriggeredNaturalGas)
        {
            isTriggeredNaturalGas = true;

            CommonSabotageEvent("어디선가 천연가스가\n새고 있습니다..!", 2.5f, () =>
            {
                // 텍스트 페이드 아웃 후 별도 시퀀스로 가스 이벤트 실행
                Sequence gasSeq = DOTween.Sequence();
                gasSeq.AppendCallback(() =>
                {
                    NaturalGasObj.SetActive(true);
                });
                gasSeq.AppendInterval(3f);
                gasSeq.AppendCallback(() =>
                {
                    NaturalGasObj.SetActive(false);
                });
            });
        }
    }
    public void TriggerEarthQuakeEvent() // 지진 이벤트
    {
        if (!isTriggeredEarthQuake)
        {
            isTriggeredEarthQuake = true;
            CommonSabotageEvent("곧 지진이 일어날 것\n같습니다..!!!", 2.5f, () =>
            {
                Vector3 originalPos = ground.transform.position;

                ground.transform.DOShakePosition(EARTHQUAKE_DURATION, EARTHQUAKE_AMOUNT, 10, 90, false, true)
                    .OnComplete(() => ground.transform.position = originalPos);
                ShakeCamera(EARTHQUAKE_DURATION);

                // 여진
                float aftershockDuration = EARTHQUAKE_DURATION * 1.5f;
                float aftershockAmount = EARTHQUAKE_AMOUNT / 3f;

                eventSeq.AppendInterval(EARTHQUAKE_DURATION);
                eventSeq.AppendCallback(() =>
                {
                    ground.transform.DOShakePosition(aftershockDuration, aftershockAmount, 10, 90, false, true)
                        .OnComplete(() => ground.transform.position = originalPos);
                    ShakeCamera(aftershockDuration);
                });
            });
        }
    }

    void ShakeCamera(float duration) // 카메라가 흔들리는 효과(배경 흔들기)
    {
        Vector3 originPos = backGround.transform.position;
        backGround.transform.DOKill();
        backGround.transform.DOShakePosition(duration, new Vector3(12f, 12f, 0f), 10, 90f)
            .OnComplete(() => backGround.transform.position =  originPos);
    }
    IEnumerator SurgeLavaCoroutine() // 용암이 차오르는 코루틴
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
