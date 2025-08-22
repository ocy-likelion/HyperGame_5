using DG.Tweening;
using System;
using System.Collections;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SabotageEventManager : MonoBehaviour
{
    // 프리팹
    [Header("프리팹")]
    [SerializeField] GameObject prefab_Mole;
    [SerializeField] GameObject prefab_BlockTopObject;

    // 씬 오브젝트
    [Header("씬 오브젝트")]
    [SerializeField] UIManager uiManager;
    [SerializeField] GameManager gameManager;
    MineralDataManager mineralDataManager;
    [SerializeField] EffectObjectPool effectObjectPool;
    [SerializeField] BlockController blockController;

    [SerializeField] GameObject NaturalGasObj;
    [SerializeField] GameObject text_LavaAlarm;
    [SerializeField] GameObject Text_SabotageAlarm;
    [SerializeField] GameObject backGround;
    [SerializeField] GameObject ground;
    [SerializeField] GameObject lava;

    // 컴포넌트
    PlayManager playManager;

    // 필드
    Sequence eventSeq;
    // 두더지 관련
    bool isTriggeredMole = false;
    // 거대 두더지 관련
    bool isTriggeredMoleKing = false;
    // 스톤러쉬 관련
    bool isTriggeredStoneRush = false;
    // 천연가스 관련
    bool isTriggeredNaturalGas = false;
    // 지진 관련
    const float EARTHQUAKE_DURATION = 0.85f;
    const float EARTHQUAKE_AMOUNT = 0.35f;
    bool isTriggeredEarthQuake = false;
    // 용암 관련
    readonly Vector3 LAVA_START_POS = new Vector3(0, -10f, 0);
    Vector3 LAVA_END_POS;
    const float LAVA_DURATION = 60f;
    const float LAVA_OFFSET = 2.25f; // 용암 위치 오프셋 값(세로 길이 / 2)

    void Awake()
    {
        DOTween.KillAll();
        TryGetComponent(out playManager);
        TryGetComponent(out mineralDataManager);

        LAVA_END_POS = new Vector3(0, playManager.goalTowerHeight - LAVA_OFFSET, 0);
    }
    void Start()
    {
        ResetEventBoolean();
        StartSurgeLava();
    }
    void Update()
    {
        if (!isTriggeredMole && playManager.GetElaspedTime() > 7f) // 10초 : 두더지 3마리
        {
            TriggerMoleEvent();
        }
        if (!isTriggeredEarthQuake && playManager.GetElaspedTime() > 17f) // 20초 : 지진
        {
            TriggerEarthQuakeEvent();
        }
        if (!isTriggeredStoneRush && playManager.GetElaspedTime() > 27f) // 30초 : 스톤러쉬
        {
            TriggerStoneRushEvent();
        }
        if (!isTriggeredMoleKing && playManager.GetElaspedTime() > 37f) // 40초 : 몰킹 1마리
        {
            TriggerMoleKingEvent();
        }
        if (!isTriggeredNaturalGas && playManager.GetElaspedTime() > 47f) // 50초 : 천연가스
        {
            TriggerNaturalGasEvent();
        }
    }

    void ResetEventBoolean() // 각종 이벤트 트리거 초기화
    {
        if (eventSeq != null && eventSeq.IsActive())
        {
            eventSeq.Kill(); // 진행 중인 Tween 종료
            eventSeq = null; // 참조 제거
        }

        isTriggeredMole = false;
        isTriggeredMoleKing = false;
        isTriggeredStoneRush = false;
        isTriggeredNaturalGas = false;
        isTriggeredEarthQuake = false;
    }
    void CommonSabotageEvent(string message, float displayTime, Action callback) // 방해 이벤트 메서드에 사용되는 공통 요소
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
                    {
                        float torque = UnityEngine.Random.value < 0.5f ? -10f : 10f;
                        rb.AddTorque(torque, ForceMode2D.Impulse);
                    }
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
                {
                    float torque = UnityEngine.Random.value < 0.5f ? -10f : 10f;
                    rb.AddTorque(torque, ForceMode2D.Impulse);
                }

                ShakeCamera(EARTHQUAKE_DURATION);
            });
        }
    }
    public void TriggerStoneRushEvent() // 스톤러쉬 이벤트
    {
        if (!isTriggeredStoneRush)
        {
            isTriggeredStoneRush = true;
            CommonSabotageEvent("돌 무더기가 떨어집니다!!", 2.5f, () =>
            {
                for (int i = -1; i < 2; i+=2)
                {
                    for (int j = 3; j < 6; j+=2)
                    {
                        GameObject go = Instantiate(prefab_BlockTopObject);
                        go.transform.position = blockController.GetBlockSpawnPoint() + new Vector3(i, j, 0);

                        AsyncOperationHandle<Sprite> spriteLoadHandle = Addressables.LoadAssetAsync<Sprite>($"Sprite_Stone_{UnityEngine.Random.Range(1, 5)}");
                        spriteLoadHandle.Completed += op =>
                        {
                            Sprite sprite = op.Result;
                            go.GetComponent<BlockOnlyTop>().InstantiateProxyObject(mineralDataManager.GetParentTopObject(), sprite, effectObjectPool, mineralDataManager);
                            go.GetComponent<SpriteOutlineCollider>().BuildCollider();

                            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                            if (rb != null)
                            {
                                rb.AddTorque(UnityEngine.Random.Range(-3f, 3f), ForceMode2D.Impulse);
                            }
                        };
                    }
                }
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
        float elapsed = 0f;
        int frameCounter = 0; // 5프레임마다 점수 차감을 위한 카운터

        while (elapsed < LAVA_DURATION)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / LAVA_DURATION);
            lava.transform.position = Vector3.Lerp(LAVA_START_POS, LAVA_END_POS, t);

            // 용암이 따라잡는지 체크
            if (lava.transform.position.y + LAVA_OFFSET > playManager.currentTowerHeight && playManager.currentTowerHeight > -3 && playManager.HasActiveBlock())
            {
                text_LavaAlarm.SetActive(true);

                // 3 프레임마다 2점 차감
                frameCounter++;
                if (frameCounter >= 3)
                {
                    gameManager.score -= 2;
                    frameCounter = 0;
                }
            }
            else
            {
                text_LavaAlarm.SetActive(false);

                frameCounter = 0; // 따라잡지 않았으면 카운터 초기화
            }

            yield return null;
        }

        lava.transform.position = LAVA_END_POS;
    }
    public void StartSurgeLava() // 용암 코루틴 시작
    {
        StartCoroutine(SurgeLavaCoroutine());
    }
}
