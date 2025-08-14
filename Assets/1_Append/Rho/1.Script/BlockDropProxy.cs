using System.Collections;
using UnityEngine;

public class BlockDropProxy : MonoBehaviour
{
    private EJewelType jewelType;
    private JewelDataManager jewelDataManager;
    private ProxyObjectPool proxyObjectPool;
    [SerializeField] GameObject blockTopObject;
    private bool isEnd = false;

    public void InstantiateProxyObject(EJewelType _jewelType, JewelDataManager _jewelDataManager, ProxyObjectPool _proxyObjectPool)
    {
        isEnd = false;
        jewelType = _jewelType;
        jewelDataManager = _jewelDataManager;
        proxyObjectPool = _proxyObjectPool;
        this.transform.SetParent(proxyObjectPool.transform);
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
            _blockTopObject.GetComponent<BlockOnlyTop>().InstantiateProxyObject(jewelType, jewelDataManager.GetParentTopObject(), GetComponent<SpriteRenderer>().sprite);
            jewelDataManager.AddLastBlock(_blockTopObject);
            gameObject.SetActive(false);
        }
    }
}
