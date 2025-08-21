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
    [SerializeField] GameObject NaturalGasObj;
    [SerializeField] GameObject Text_SabotageAlarm;
    [SerializeField] BlockController blockController;
    [SerializeField] GameObject backGround;
    [SerializeField] GameObject ground;
    [SerializeField] GameObject lava;

    [Header("주요 프로퍼티")]
    // 두더지 관련
    readonly Vector2 MOLE_GEN_POS = new Vector2(0, 6f);
    readonly Vector2 FEATHER_GEN_POS = new Vector2(0, 6.5f);
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
    // 몰킹 관련
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
        isTriggeredMole = false;
        isTriggeredEarthQuake = false;
        isTriggeredMiniMole = false;
        isTriggeredMoleKing = false;
        isTriggeredNaturalGas = false;
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
                for (int i = -1; i < 2; i++)
                {
                    GameObject go = Instantiate(prefab_Mole);
                    go.transform.position = blockController.GetBlockSpawnPoint() + new Vector3(i, Random.Range(1f, 3f), 0);
                    go.GetComponent<SpriteOutlineCollider>().BuildCollider();

                    Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        float randomTorque = Random.Range(-10f, 10f); // 시계/반시계 랜덤 회전 힘
                        rb.AddTorque(randomTorque, ForceMode2D.Impulse);
                    }
                }
            });
        }
    }
    void TriggerEarthQuakeEvent() // 지진 이벤트 메서드
    {
        if (!isTriggeredEarthQuake)
        {
            isTriggeredEarthQuake = true;

            TMP_Text sabotageText = Text_SabotageAlarm.GetComponent<TMP_Text>();
            sabotageText.text = "곧 지진이 일어날 것 " + System.Environment.NewLine + "같습니다..!!!";
            Text_SabotageAlarm.SetActive(true);

            Vector3 originalPos = ground.transform.position;

            Sequence seq = DOTween.Sequence();
            seq.Append(sabotageText.DOFade(1f, 0.5f));
            seq.AppendInterval(2.5f);
            seq.Append(sabotageText.DOFade(0f, 0.5f).OnComplete(() => Text_SabotageAlarm.SetActive(false)));

            seq.AppendCallback(() =>
            {
                ground.transform.DOShakePosition(EARTHQUAKE_DURATION, EARTHQUAKE_AMOUNT, 10, 90, false, true)
                    .OnComplete(() => ground.transform.position = originalPos);
                ShakeCamera(EARTHQUAKE_DURATION);
            });
            seq.AppendInterval(EARTHQUAKE_DURATION);

            // 여진
            seq.AppendCallback(() =>
            {
                float aftershockDuration = EARTHQUAKE_DURATION * 1.5f;
                float aftershockAmount = EARTHQUAKE_AMOUNT / 3f;

                ground.transform.DOShakePosition(aftershockDuration, aftershockAmount, 10, 90, false, true)
                    .OnComplete(() => ground.transform.position = originalPos);
                ShakeCamera(aftershockDuration);
            });
        }
    }
    void TriggerMiniMoleEvent() // 미니 두더지 이벤트 메서드
    {
        if (!isTriggeredMiniMole)
        {
            isTriggeredMiniMole = true;

            TMP_Text sabotageText = Text_SabotageAlarm.GetComponent<TMP_Text>();
            sabotageText.text = "작은 두더지 떼가 몰려옵니다!";
            Text_SabotageAlarm.SetActive(true);

            Sequence seq = DOTween.Sequence();

            seq.Append(sabotageText.DOFade(1f, 0.5f));

            seq.AppendInterval(2.5f);

            seq.Append(sabotageText.DOFade(0f, 0.5f)
                .OnComplete(() => Text_SabotageAlarm.SetActive(false)));

            seq.AppendCallback(() =>
            {
                for (int i = -1; i < 2; i++)
                {
                    for (int j = 3; j < 5; j++)
                    {
                        GameObject go = Instantiate(prefab_Mole);
                        go.transform.position = blockController.GetBlockSpawnPoint() + new Vector3(i, j, 0);
                        go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                        go.GetComponent<SpriteOutlineCollider>().BuildCollider();

                        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            float randomTorque = Random.Range(-10f, 10f); // 시계/반시계 랜덤 회전 힘
                            rb.AddTorque(randomTorque, ForceMode2D.Impulse);
                        }
                    }
                }
            });
        }
    }
    void TriggerMoleKingEvent() // 몰킹 이벤트 메서드
    {
        if (!isTriggeredMoleKing)
        {
            isTriggeredMoleKing = true;

            TMP_Text sabotageText = Text_SabotageAlarm.GetComponent<TMP_Text>();
            sabotageText.text = "거대한 두더지가 내려옵니다!";
            Text_SabotageAlarm.SetActive(true);

            Sequence seq = DOTween.Sequence();

            seq.Append(sabotageText.DOFade(1f, 0.5f));

            seq.AppendInterval(2.5f);

            seq.Append(sabotageText.DOFade(0f, 0.5f)
                .OnComplete(() => Text_SabotageAlarm.SetActive(false)));

            seq.AppendCallback(() =>
            {
                GameObject go = Instantiate(prefab_Mole);
                go.transform.position = blockController.GetBlockSpawnPoint() + new Vector3(0, 3, 0);
                go.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
                go.GetComponent<SpriteOutlineCollider>().BuildCollider();

                Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    float randomTorque = Random.Range(-10f, 10f); // 시계/반시계 랜덤 회전 힘
                    rb.AddTorque(randomTorque, ForceMode2D.Impulse);
                }

                ShakeCamera(EARTHQUAKE_DURATION);
            });
        }
    }
    void TriggerNaturalGasEvent() // 천연가스 이벤트 메서드
    {
        if (!isTriggeredNaturalGas)
        {
            isTriggeredNaturalGas = true;

            TMP_Text sabotageText = Text_SabotageAlarm.GetComponent<TMP_Text>();
            sabotageText.text = "어디선가 천연가스가 새고 있습니다..!";
            Text_SabotageAlarm.SetActive(true);

            Sequence seq = DOTween.Sequence();

            seq.Append(sabotageText.DOFade(1f, 0.5f));

            seq.AppendInterval(2.5f);

            seq.Append(sabotageText.DOFade(0f, 0.5f)
                .OnComplete(() => Text_SabotageAlarm.SetActive(false)));

            seq.AppendCallback(() =>
            {
                NaturalGasObj.SetActive(true);
            });

            seq.AppendInterval(3f);
            seq.AppendCallback(() =>
            {
                NaturalGasObj.SetActive(false);
            });
        }
    }
    void ShakeCamera(float duration) // 카메라 쉐이킹 메서드
    {
        Vector3 originPos = backGround.transform.position;
        backGround.transform.DOKill();
        backGround.transform.DOShakePosition(duration, new Vector3(12f, 12f, 0f), 10, 90f)
            .OnComplete(() => backGround.transform.position =  originPos);
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
