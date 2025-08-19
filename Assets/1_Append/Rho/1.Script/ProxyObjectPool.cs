using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
public enum EJewelType
{
    easy,
    moderate_1,
    //moderate_2,
    difficult_1
    //difficult_2,
    //difficult_3,
    //hard_1,
    //hard_2,
    //hard_3,
    //hard_4
}

[Serializable]
public class SerializableKeyValue<EJewelType, GameObject>
{
    public EJewelType jewelType;
    public GameObject jewelGameObject;
}

public class ProxyObjectPool : MonoBehaviour
{
    [SerializeField] GameObject proxyGameObject;
    private Queue<GameObject> proxyQueue = new Queue<GameObject>();
    private const int LOAD_COUNT = 15;

    private void Awake()
    {
        for (int i = 0; i < LOAD_COUNT; i++)
        {
            GameObject proxyTempObject = Instantiate(proxyGameObject);
            proxyQueue.Enqueue(proxyTempObject);
            proxyTempObject.SetActive(false);
        }
    }

    public GameObject Get()
    {
        if (proxyQueue.Count == 0)
        {
            foreach (var jewel in proxyQueue)
            {
                GameObject proxyTempObject = Instantiate(proxyGameObject);
                proxyQueue.Enqueue(proxyTempObject);
                proxyTempObject.SetActive(false);
            }
        }

        GameObject jewelGameObject = proxyQueue.Dequeue();
        jewelGameObject.SetActive(true);
        return jewelGameObject;
    }
    public void Return(GameObject _gameObject)
    {
        proxyQueue.Enqueue(_gameObject);
        _gameObject.SetActive(false);
    }
}
