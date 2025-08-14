using System.Collections;
using UnityEngine;

public class BlockDropProxy : MonoBehaviour
{
    private EJewelType jewelType;
    private JewelDataManager jewelDataManager;
    private ProxyObjectPool proxyObjectPool;
    private GameObject blockTopObject;
    private bool isEnd = false;
    public void InstantiateProxyObject(EJewelType _jewelType, JewelDataManager _jewelDataManager, ProxyObjectPool _proxyObjectPool, GameObject _blockTopObject)
    {
        isEnd = false;
        jewelType = _jewelType;
        jewelDataManager = _jewelDataManager;
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
            _blockTopObject.GetComponent<BlockOnlyTop>().InstantiateProxyObject(jewelType, jewelDataManager.GetParentTopObject());
            proxyObjectPool.Return(jewelType, this.gameObject);
            jewelDataManager.AddLastBlock(_blockTopObject);
        }
    }
}
