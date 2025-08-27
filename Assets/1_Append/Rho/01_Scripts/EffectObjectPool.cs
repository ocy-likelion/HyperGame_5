using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EffectObjectPool : MonoBehaviour
{
    // 상수
    private const int LOAD_COUNT = 15;

    // 프리팹
    [Header("프리팹")]
    [SerializeField] private GameObject prefab_Effect;

    // private 필드
    private Queue<GameObject> effectObjectQueue = new Queue<GameObject>();

    // 유니티 롤백
    private void Awake()
    {
        for (int i = 0; i < LOAD_COUNT; i++)
        {
            GameObject effectTempObject = Instantiate(prefab_Effect);
            effectTempObject.transform.SetParent(this.gameObject.transform);
            effectObjectQueue.Enqueue(effectTempObject);
            effectTempObject.SetActive(false);
            effectTempObject.GetComponent<EffectObject>().InitEffectObject(this);
        }
    }

    public void Get(Transform _transform)
    {
        GameObject effectObject = effectObjectQueue.Dequeue();
        effectObject.SetActive(true);
        effectObject.transform.position = _transform.position;
        effectObject.GetComponent<EffectObject>().PlayEffect();
    }
    public void Return(GameObject _effectObject)
    {
        effectObjectQueue.Enqueue(_effectObject);
        _effectObject.SetActive(false);
    }
}
