using System.Collections;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class BlockDropProxy : MonoBehaviour
{
    private MineralDataManager mineralDataManager;
    private ProxyObjectPool proxyObjectPool;
    private EffectObjectPool effectObjectPool;
    public bool IsEnd = true;
    [SerializeField] GameObject blockTopObject;

    GameObject blockTopInstance; // 프록시 생성 시 만들어지는 인스턴스
    
    public void InstantiateProxyObject(MineralDataManager _mineralDataManager, ProxyObjectPool _proxyObjectPool, EffectObjectPool _effectObjectPool)
    {
        IsEnd = true;
        mineralDataManager = _mineralDataManager;
        proxyObjectPool = _proxyObjectPool;
        effectObjectPool = _effectObjectPool;
    }
    public GameObject InstantiateTopObject() // 프록시 생성 시 탑 오브젝트도 미리 생성하는 메서드
    {
        blockTopInstance = Instantiate(blockTopObject, new Vector3(-10, -10, 0), Quaternion.Euler(Vector2.zero));
        blockTopInstance.GetComponent<BlockOnlyTop>().InstantiateProxyObject(mineralDataManager.GetParentTopObject(), GetComponent<SpriteRenderer>().sprite, effectObjectPool);
        blockTopInstance.SetActive(false); // TODO : false로 수정 요

        return blockTopInstance;
    }

    void FixedUpdate()
    {
        if (!IsEnd)
        {
            Vector3 targetPosition = transform.position;
            transform.position = targetPosition + Vector3.down * Time.fixedDeltaTime * 10f;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsEnd) return;
        if (collision.gameObject.CompareTag("Block") || collision.gameObject.CompareTag("Platform"))
        {
            IsEnd = true;

            blockTopInstance.SetActive(true);
            blockTopInstance.transform.position = transform.position;
            effectObjectPool.Get(blockTopInstance.gameObject.transform);

            proxyObjectPool.Return(gameObject);
            mineralDataManager.AddLastBlock(blockTopInstance);
        }
    }
}
