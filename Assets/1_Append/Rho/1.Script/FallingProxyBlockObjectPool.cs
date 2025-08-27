using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingProxyBlockObjectPool : MonoBehaviour
{
    // 상수
    private const int LOAD_COUNT = 15;

    // 프리팹
    [Header("프리팹")]
    [SerializeField] private GameObject prefab_FallingProxyBlock;

    // private 필드
    private Queue<GameObject> fallingProxyBlockObjectQueue = new Queue<GameObject>();

    // 유니티 콜백
    private void Awake()
    {
        for (int i = 0; i < LOAD_COUNT; i++)
        {
            CreateAndEnqueue();
        }
    }

    // 메인
    public GameObject Get()
    {
        if (fallingProxyBlockObjectQueue.Count == 0)
        {
            CreateAndEnqueue();
        }

        GameObject go = fallingProxyBlockObjectQueue.Dequeue();
        go.SetActive(true);
        return go;
    }
    public void Return(GameObject _gameObject)
    {
        fallingProxyBlockObjectQueue.Enqueue(_gameObject);
        _gameObject.SetActive(false);
    }
    private void CreateAndEnqueue() // 새 오브젝트 생성 후 큐에 저장
    {
        GameObject go = Instantiate(prefab_FallingProxyBlock);
        go.SetActive(false);
        fallingProxyBlockObjectQueue.Enqueue(go);
    }
}
