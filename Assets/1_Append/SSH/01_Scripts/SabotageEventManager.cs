using DG.Tweening;
using UnityEngine;

public class SabotageEventManager : MonoBehaviour
{
    [Header("프리팹")]
    [SerializeField] GameObject prefab_Mole;
    [SerializeField] GameObject prefab_Feather;

    [Header("씬 오브젝트")]
    [SerializeField] GameObject ground;

    [Header("주요 프로퍼티")]
    readonly Vector2 SINKHOLE_POS = new Vector2(0, -1f);
    const float SINKHOLE_DURATION = 0.25f;
    readonly Vector2 MOLE_GEN_POS = new Vector2(0, 6f);
    readonly Vector2 FEATHER_GEN_POS = new Vector2(0, 6.5f);
    const float SHAKE_CAMERA_AMOUNT = 1.2f;

    public void EventCheckByMineralCount(int mineralCount)
    {
        if (mineralCount % 8 == 0)
        {
            TriggerMoleEvent();
        }
        if (mineralCount % 4 == 0)
        {
            TriggerSinkHoleEvent();
        }
        if (mineralCount % 6 == 0)
        {
            TriggerFeatherEvent();
        }
    }

    void TriggerMoleEvent()
    {
        GameObject go = Instantiate(prefab_Mole);
        go.transform.position = MOLE_GEN_POS + new Vector2(Random.Range(-3f, 3f), 0);
    }
    void TriggerFeatherEvent()
    {
        GameObject go = Instantiate(prefab_Feather);
        go.transform.position = FEATHER_GEN_POS + new Vector2(Random.Range(-3f, 3f), 0);
    }
    void TriggerSinkHoleEvent()
    {
        ground.transform.DOMove((Vector2)ground.transform.position + SINKHOLE_POS, SINKHOLE_DURATION);
        ShakeCamera(SINKHOLE_DURATION, SHAKE_CAMERA_AMOUNT);
    }

    void ShakeCamera(float duration, float strength)
    {
        Camera.main.transform.DOShakePosition(duration, strength, 10, 90f, false, true);
    }
}
