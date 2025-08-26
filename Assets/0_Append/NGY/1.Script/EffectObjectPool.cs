using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EffectObjectPool : MonoBehaviour
{
    [SerializeField] GameObject effectGameObject;
    private Queue<GameObject> effectQueue = new Queue<GameObject>();
    private const int LOAD_COUNT = 15;

    private void Awake()
    {
        for (int i = 0; i < LOAD_COUNT; i++)
        {
            GameObject effectTempObject = Instantiate(effectGameObject);
            effectTempObject.transform.SetParent(this.gameObject.transform);
            effectQueue.Enqueue(effectTempObject);
            effectTempObject.SetActive(false);
            effectTempObject.GetComponent<EffectObject>().InitEffectObject(this);
        }
    }

    public void Get(Transform _transform)
    {
        GameObject effectObject = effectQueue.Dequeue();
        effectObject.SetActive(true);
        effectObject.transform.position = _transform.position;
        effectObject.GetComponent<EffectObject>().PlayEffect();
    }
    
    public void Return(GameObject _effectObject)
    {
        effectQueue.Enqueue(_effectObject);
        _effectObject.SetActive(false);
    }
}
