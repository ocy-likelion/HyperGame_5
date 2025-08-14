using System;
using System.Collections.Generic;
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
    [SerializeField] private List<SerializableKeyValue<EJewelType, GameObject>> jewelTypePrefab;
    private Dictionary<EJewelType, GameObject> queueGameObject = new Dictionary<EJewelType, GameObject>();

    private void Awake()
    {
        foreach (var jewel in jewelTypePrefab)
        {
            GameObject _jewelGameObject = Instantiate(jewel.jewelGameObject, this.transform);
            queueGameObject.Add(jewel.jewelType, _jewelGameObject);
            _jewelGameObject.SetActive(false);
        }
    }

    public GameObject Get(EJewelType jewelType)
    {
        if (jewelTypePrefab.Count == 0)
        {
            foreach (var jewel in jewelTypePrefab)
            {
                GameObject _jewelGameObject = Instantiate(jewel.jewelGameObject, this.transform);
                queueGameObject.Add(jewel.jewelType, _jewelGameObject);
                _jewelGameObject.SetActive(false);
            }
        }

        GameObject jewelGameObject = queueGameObject[jewelType];
        jewelGameObject.SetActive(true);
        queueGameObject.Remove(jewelType);
        return jewelGameObject;
    }

    public void Return(EJewelType jewelType, GameObject _gameObject)
    {
        queueGameObject.Add(jewelType, _gameObject);
        _gameObject.SetActive(false);
    }
}
