using System.Collections;
using UnityEngine;
using DG.Tweening;

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
        StartCoroutine(BlockOpacityCoroutine());
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
            GameObject backGroundObject = GameObject.FindWithTag("BackGround");
            backGroundObject.transform.DOShakePosition(1f, new Vector3(5f, 5f, 0f), 10, 90f);

            IsEnd = true;

            blockTopInstance.SetActive(true);
            blockTopInstance.transform.position = transform.position;
            effectObjectPool.Get(blockTopInstance.gameObject.transform);

            proxyObjectPool.Return(gameObject);
            mineralDataManager.AddLastBlock(blockTopInstance);
            EventBus.Instance.Publish(Consts.BLOCK_LANDED); // 블럭이 떨어졌음을 알리기
        }
    }

    IEnumerator BlockOpacityCoroutine()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        float time = 0f;
        float fixTime = 1f;
        bool isUpper = false;
        bool isLower = true;
        Color originColor = spriteRenderer.color;

        while (IsEnd)
        {
            if (isLower)
            {
                time += Time.deltaTime;
                float percent = Mathf.Clamp01(time / fixTime);

                Color tempcolor = spriteRenderer.color;
                tempcolor.a = percent;
                spriteRenderer.color = tempcolor;

                if (time >= fixTime)
                {
                    isLower = false;
                    isUpper = true;
                }
            }
            else if (isUpper)
            {
                time -= Time.deltaTime;
                float percent = Mathf.Clamp01(time / fixTime);

                var c = spriteRenderer.color;
                c.a = percent;
                spriteRenderer.color = c;

                if (time <= 0f)
                {
                    isLower = true;
                    isUpper = false;
                }
            }

            yield return null;
        }

        spriteRenderer.color = originColor;
    }
}
