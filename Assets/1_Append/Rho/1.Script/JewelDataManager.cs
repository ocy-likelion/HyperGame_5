using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JewelDataManager : MonoBehaviour
{
    public float x = 0;
    [SerializeField] Transform proxyParent;
    [SerializeField] Transform topParent;

    [SerializeField] private List<SerializableKeyValue<EJewelType, GameObject>> jewelTypePrefab;
    [SerializeField] ProxyObjectPool blockDropProxyPool;
    public List<GameObject> ingameBlockList { get; private set; } = new List<GameObject>();
    private Dictionary<EJewelType, GameObject> queueGameObject = new Dictionary<EJewelType, GameObject>();

    private void Awake()
    {
        foreach (var jewel in jewelTypePrefab)
        {
            queueGameObject.Add(jewel.jewelType, jewel.jewelGameObject);
        }
    }

    public void CalledWhenDropBlock(GameObject go)
    {

    }
    public void InitializeWithJewelToEasy()
    {
        GameObject blockTopObject = queueGameObject[EJewelType.easy];
        GameObject blockProxyObject = blockDropProxyPool.Get(EJewelType.easy);
        float maxY = 0;
        for (int i = 0; i < ingameBlockList.Count; i++)
        {
            float temp = ingameBlockList[i].transform.position.y;
            maxY = temp > maxY ? temp : maxY;
        }

        float tempY = maxY + 10f;
        blockProxyObject.transform.position = new Vector3(x, tempY, 0);
        blockProxyObject.GetComponent<BlockDropProxy>().InstantiateProxyObject(EJewelType.easy, this, blockDropProxyPool, blockTopObject, GetComponent<SpriteRenderer>().sprite);
    }

    public void InitializeWithJewelToModerate_1()
    {
        GameObject blockTopObject = queueGameObject[EJewelType.moderate_1];
        GameObject blockProxyObject = blockDropProxyPool.Get(EJewelType.moderate_1);
        float maxY = 0;
        for (int i = 0; i < ingameBlockList.Count; i++)
        {
            float temp = ingameBlockList[i].transform.position.y;
            maxY = temp > maxY ? temp : maxY;
        }

        float tempY = maxY + 10f;
        blockProxyObject.transform.position = new Vector3(x, tempY, 0);
        blockProxyObject.GetComponent<BlockDropProxy>().InstantiateProxyObject(EJewelType.moderate_1, this, blockDropProxyPool, blockTopObject);
    }

    public void InitializeWithJewelToDifficult()
    {
        GameObject blockTopObject = queueGameObject[EJewelType.difficult_1];
        GameObject blockProxyObject = blockDropProxyPool.Get(EJewelType.difficult_1);
        float maxY = 0;
        for (int i = 0; i < ingameBlockList.Count; i++)
        {
            float temp = ingameBlockList[i].transform.position.y;
            maxY = temp > maxY ? temp : maxY;
        }

        float tempY = maxY + 10f;
        blockProxyObject.transform.position = new Vector3(x, tempY, 0);
        blockProxyObject.GetComponent<BlockDropProxy>().InstantiateProxyObject(EJewelType.difficult_1, this, blockDropProxyPool, blockTopObject);
    }

    public void StartSlidingToObject()
    {
        if (ingameBlockList.Count > 0)
        {
            ingameBlockList[ingameBlockList.Count - 1].GetComponent<BlockOnlyTop>().ApplySlideMotion();
        }
    }

    public void AddLastBlock(GameObject _object)
    {
        ingameBlockList.Add(_object);
    }

    public void RightArrowButton()
    {
        ++x;
    }

    public void LeftArrowButton()
    {
        --x;
    }

    public Transform GetParentTopObject()
    {
        return topParent;
    }
}

