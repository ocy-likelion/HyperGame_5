using System.Collections;
using UnityEngine;

//public enum MineralTypeEnum
//{
//    Stone,
//    Copper,
//    Silver,
//    Gold
//}

public class BlockDropProxy : MonoBehaviour
{
    private MineralTypeEnum mineralType;
    private MineralDataManager mineralDataManager;
    private ProxyObjectPool proxyObjectPool;
    private GameObject blockTopObject;
    private bool isEnd = false;
    public void InstantiateProxyObject(MineralTypeEnum _mineralType, MineralDataManager _mineralDataManager, ProxyObjectPool _proxyObjectPool, GameObject _blockTopObject)
    {
        isEnd = false;
        mineralType = _mineralType;
        mineralDataManager = _mineralDataManager;
        proxyObjectPool = _proxyObjectPool;
        this.transform.SetParent(proxyObjectPool.transform);
        blockTopObject = _blockTopObject;
        blockTopObject.SetActive(false);
    }

    void FixedUpdate()
    {
        if (!isEnd)
        {
            Vector3 targetPosition = transform.position;
            transform.position = targetPosition + Vector3.down * Time.fixedDeltaTime * 10f;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Block"))
        {
            isEnd = true;
            var _blockTopObject = Instantiate(blockTopObject, this.transform.position, Quaternion.Euler(Vector2.zero));
            _blockTopObject.SetActive(true);
            _blockTopObject.GetComponent<BlockOnlyTop>().InstantiateProxyObject(mineralType, mineralDataManager.GetParentTopObject());
            proxyObjectPool.Return(mineralType, this.gameObject);
            mineralDataManager.AddLastBlock(_blockTopObject);
        }
    }
}
