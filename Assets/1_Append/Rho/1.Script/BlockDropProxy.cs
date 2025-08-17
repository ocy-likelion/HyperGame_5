using System.Collections;
using UnityEngine;

public class BlockDropProxy : MonoBehaviour
{
    private MineralDataManager mineralDataManager;
    private ProxyObjectPool proxyObjectPool;
    private EffectObjectPool effectObjectPool;
    private bool isEnd = false;
    [SerializeField] GameObject blockTopObject;
    
    public void InstantiateProxyObject(MineralDataManager _mineralDataManager, ProxyObjectPool _proxyObjectPool, EffectObjectPool _effectObjectPool)
    {
        isEnd = false;
        mineralDataManager = _mineralDataManager;
        proxyObjectPool = _proxyObjectPool;
        effectObjectPool = _effectObjectPool;
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
        if (isEnd) return;
        if (collision.gameObject.CompareTag("Block"))
        {
            effectObjectPool.Get(this.gameObject.transform);
            isEnd = true;

            GameObject _blockTopObject = Instantiate(blockTopObject, this.transform.position, Quaternion.Euler(Vector2.zero));
            _blockTopObject.GetComponent<BlockOnlyTop>().InstantiateProxyObject(mineralDataManager.GetParentTopObject(), GetComponent<SpriteRenderer>().sprite);
            proxyObjectPool.Return(gameObject);
            mineralDataManager.AddLastBlock(_blockTopObject);
        }
    }
}
