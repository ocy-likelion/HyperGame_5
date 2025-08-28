using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SabotageEventManager : MonoBehaviour
{
    // 상수
    private const float EARTHQUAKE_DURATION = 0.85f; // 지진 지속 시간
    private const float EARTHQUAKE_AMOUNT = 0.55f; // 지진 세기
    private const float LAVA_DURATION = 60f; // 용암이 움직이는 시간
    private const float LAVA_OFFSET = 2.25f; // 용암 위치 오프셋 값

    // 프리팹
    [Header("프리팹")]
    [SerializeField] private GameObject prefab_Mole;
    [SerializeField] private GameObject prefab_TopBlock;

    // 씬 오브젝트
    [Header("씬 오브젝트")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private EffectObjectPool effectObjectPool;
    [SerializeField] private BlockController blockController;

    [SerializeField] private GameObject[] naturalGasObj;
    [SerializeField] private GameObject text_LavaAlarm;
    [SerializeField] private GameObject text_SabotageAlarm;
    [SerializeField] private GameObject backGround;
    [SerializeField] private GameObject ground;
    [SerializeField] private GameObject lava;

    // private 필드(컴포넌트)
    private PlayManager playManager;
    private SpawnBlockManager spawnBlockManager;

    // private 필드
    private Sequence eventSeq; // 방해 이벤트의 두트윈 시퀀스
    private readonly Vector3 LAVA_START_POS = new Vector3(0, -10f, 0);
    private Vector3 LAVA_END_POS;
    private List<Action> sabotageEvents;
    private readonly float[] eventTimes = { 7f, 17f, 27f, 37f, 47f }; // 이벤트 시작 시간

    // 유니티 콜백
    private void Awake()
    {
        DOTween.KillAll();
        TryGetComponent(out playManager);
        TryGetComponent(out spawnBlockManager);

        LAVA_END_POS = new Vector3(0, PlayManager.GOAL_HEIGHT - LAVA_OFFSET, 0);
    }
    private void Start()
    {
        if (eventSeq != null && eventSeq.IsActive())
        {
            eventSeq.Kill(); // 진행 중인 Tween 종료
            eventSeq = null; // 참조 제거
        }
        sabotageEvents = new() { TriggerMoleEvent, TriggerGreatMoleEvent, TriggerStoneRushEvent, TriggerNaturalGasEvent, TriggerEarthQuakeEvent };

        StartCoroutine(SurgeLavaCoroutine());
        StartCoroutine(EventSequenceCoroutine());
    }

    // 각종 이벤트
    private void CommonSabotageEvent(string message, float displayTime, Action callback) // 방해 이벤트 메서드에 사용되는 공통 요소
    {
        // 텍스트 세팅
        TMP_Text sabotageText = text_SabotageAlarm.GetComponent<TMP_Text>();
        sabotageText.text = message;
        text_SabotageAlarm.SetActive(true);

        // 두트윈 시퀀스를 통해 순차적으로 텍스트 알람 및 메서드 실행되도록 하기
        eventSeq = DOTween.Sequence();
        eventSeq.Append(sabotageText.DOFade(1f, 0.5f));
        eventSeq.AppendInterval(displayTime);
        eventSeq.Append(sabotageText.DOFade(0f, 0.5f)
            .OnComplete(() => text_SabotageAlarm.SetActive(false)));
        eventSeq.AppendCallback(new TweenCallback(() =>
        {
            callback?.Invoke(); // callback이 null이면 안전하게 무시
        }));
    }
    private void TriggerMoleEvent() // 두더지 이벤트
    {
        CommonSabotageEvent("두더지 떼가 몰려옵니다..", 2.5f, () =>
        {
            for (int i = -2; i < 3; i++)
            {
                // 두더지 생성
                GameObject go = Instantiate(prefab_Mole);
                go.transform.position = blockController.BlockSpawnPosition + new Vector3(i, UnityEngine.Random.Range(2, 7));
                go.GetComponent<SpriteOutlineCollider>().BuildCollider();

                // 두더지에 회전 힘 가하기
                Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    float torque = UnityEngine.Random.value < 0.5f ? -3f : 3f;
                    rb.AddTorque(torque, ForceMode2D.Impulse);
                }
            }
        });
    }
    private void TriggerGreatMoleEvent() // 거대 두더지 이벤트
    {
        CommonSabotageEvent("거대한 두더지가 내려옵니다!", 2.5f, () =>
        {
            // 두더지 생성
            GameObject go = Instantiate(prefab_Mole);
            go.transform.position = blockController.BlockSpawnPosition + new Vector3(0, 3, 0);
            go.transform.localScale = Vector3.one * 2.5f;
            go.GetComponent<SpriteOutlineCollider>().BuildCollider();

            // 두더지에 회전 힘 가하기
            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float torque = UnityEngine.Random.value < 0.5f ? -3f : 3f;
                rb.AddTorque(torque, ForceMode2D.Impulse);
            }

            ShakeCamera(EARTHQUAKE_DURATION); // 카메라도 흔들어주기
        });
    }
    private void TriggerStoneRushEvent() // 스톤러쉬 이벤트
    {
        CommonSabotageEvent("돌 무더기가 떨어집니다!!", 2.5f, () =>
        {
            for (int i = -1; i < 2; i += 2)
            {
                for (int j = 3; j < 6; j += 2)
                {
                    // 돌 생성
                    GameObject go = Instantiate(prefab_TopBlock);
                    go.transform.position = blockController.BlockSpawnPosition + new Vector3(i, j, 0);

                    // 비동기로 돌 스프라이트 할당
                    AsyncOperationHandle<Sprite> spriteLoadHandle = Addressables.LoadAssetAsync<Sprite>($"Sprite_Stone_{UnityEngine.Random.Range(1, 5)}");
                    spriteLoadHandle.Completed += op =>
                    {
                        Sprite sprite = op.Result;
                        go.GetComponent<TopBlockObject>().InitTopBlockObject(spawnBlockManager.TopBlockObjectParent, sprite, effectObjectPool); // 쌓이는 블럭 오브젝트 초기화
                        go.GetComponent<SpriteOutlineCollider>().BuildCollider(); // 콜라이더 생성

                        // 회전하는 힘 가해주기
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
    private void TriggerNaturalGasEvent() // 천연가스 이벤트
    {
        CommonSabotageEvent("어디선가 천연가스가\n새고 있습니다..!", 2.5f, () =>
        {
            if (naturalGasObj.Length == 0) return;

            // 무작위 천연가스 선택
            int randomIndex = UnityEngine.Random.Range(0, naturalGasObj.Length);

            // 3초간 천연가스 효과 활성화
            Sequence gasSeq = DOTween.Sequence();
            gasSeq.AppendCallback(() => naturalGasObj[randomIndex].SetActive(true));
            gasSeq.AppendInterval(3f);
            gasSeq.AppendCallback(() => naturalGasObj[randomIndex].GetComponent<NaturalGasObject>().TurnOffNaturalGas());
        });
    }
    private void TriggerEarthQuakeEvent() // 지진 이벤트
    {
        CommonSabotageEvent("곧 지진이 일어날 것\n같습니다..!!!", 2.5f, () =>
        {
            Vector3 originalPos = ground.transform.position;

            ground.transform.DOShakePosition(EARTHQUAKE_DURATION, EARTHQUAKE_AMOUNT, 10, 90, false, true).OnComplete(() => ground.transform.position = originalPos); // 두트윈으로 흔들기
            ShakeCamera(EARTHQUAKE_DURATION); // 카메라 흔들기

            // 여진
            float aftershockDuration = EARTHQUAKE_DURATION * 1.5f;
            float aftershockAmount = EARTHQUAKE_AMOUNT / 3f;

            // 본진 대기 후 여진 활성화
            eventSeq.AppendInterval(EARTHQUAKE_DURATION);
            eventSeq.AppendCallback(() =>
            {
                ground.transform.DOShakePosition(aftershockDuration, aftershockAmount, 10, 90, false, true)
                    .OnComplete(() => ground.transform.position = originalPos);
                ShakeCamera(aftershockDuration);
            });
        });
    }
    private IEnumerator EventSequenceCoroutine() // 방해 이벤트 시퀀스 코루틴
    {
        for (int i = 0; i < eventTimes.Length; i++)
        {
            float waitTime = eventTimes[i] - gameManager.GameElaspedTime;
            yield return new WaitForSeconds(waitTime);

            if (sabotageEvents.Count > 0)
            {
                // 남은 이벤트 중 랜덤 선택
                int randomIndex = UnityEngine.Random.Range(0, sabotageEvents.Count);
                sabotageEvents[randomIndex].Invoke();

                // 선택한 이벤트 제거
                sabotageEvents.RemoveAt(randomIndex);
            }
        }
    }

    // 용암
    private void ShakeCamera(float duration) // 카메라가 흔들리는 효과(배경 흔들기)
    {
        Vector3 originPos = backGround.transform.position;
        backGround.transform.DOKill();
        backGround.transform.DOShakePosition(duration, new Vector3(12f, 12f, 0f), 10, 90f)
            .OnComplete(() => backGround.transform.position =  originPos);
    }
    private IEnumerator SurgeLavaCoroutine() // 용암이 차오르는 코루틴
    {
        float elapsed = 0f;
        int frameCounter = 0; // 프레임마다 점수 차감을 위한 프레임 카운터
        const int FIXED_FRAME = 5;

        while (elapsed < LAVA_DURATION)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / LAVA_DURATION);
            lava.transform.position = Vector3.Lerp(LAVA_START_POS, LAVA_END_POS, t);

            // 용암이 따라잡는지 체크
            if (lava.transform.position.y + LAVA_OFFSET > playManager.CurrentTowerHeight && playManager.CurrentTowerHeight > -3 && playManager.IsExistTopBlock())
            {
                text_LavaAlarm.SetActive(true);

                // 점수 차감 로직
                frameCounter++;
                if (frameCounter >= FIXED_FRAME)
                {
                    gameManager.ModifyScore(-1);
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
}
