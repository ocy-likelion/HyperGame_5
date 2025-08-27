using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FallingProxyBlockObject : MonoBehaviour
{
    // 상수
    const float BLOCK_DROP_SPEED = 9.5f;

    [Header("프리팹")]
    // 프리팹
    [SerializeField] private GameObject prefab_TopBlock;

    // private 필드
    private SpawnBlockManager mineralDataManager;
    private FallingProxyBlockObjectPool fallingProxyBlockObjectPool;
    private EffectObjectPool effectObjectPool;
    private bool isFalling = true;
    private GameObject topBlockObjectInstance; // 쌓이는 블럭 오브젝트 인스턴스

    // 블럭 초기화
    public void InitFallingProxyBlock(SpawnBlockManager mineralDataManager, FallingProxyBlockObjectPool fallingProxyBlockObjectPool, EffectObjectPool effectObjectPool)
    {
        isFalling = true;
        this.mineralDataManager = mineralDataManager;
        this.fallingProxyBlockObjectPool = fallingProxyBlockObjectPool;
        this.effectObjectPool = effectObjectPool;
        StartCoroutine(BlockOpacityCoroutine());
    }
    public GameObject InstantiateTopBlock() // 쌓이는 블럭 오브젝트를 생성(호출 시점 : 떨어뜨릴 블럭 오브젝트 생성과 동시에 호출)
    {
        topBlockObjectInstance = Instantiate(prefab_TopBlock, new Vector3(-10, -10, 0), Quaternion.Euler(Vector2.zero));
        topBlockObjectInstance.GetComponent<TopBlockObject>().InstantiateProxyObject(mineralDataManager.TopBlockObjectParent, GetComponent<SpriteRenderer>().sprite, effectObjectPool, mineralDataManager);
        topBlockObjectInstance.SetActive(false);
        return topBlockObjectInstance;
    }

    void FixedUpdate()
    {
        if (!isFalling)
        {
            Vector3 targetPosition = transform.position;
            transform.position = targetPosition + Vector3.down * Time.fixedDeltaTime * BLOCK_DROP_SPEED;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (isFalling) return;
        if (collision.gameObject.CompareTag("Block") || collision.gameObject.CompareTag("Platform"))
        {
            RealSoundManager.Instance.PlayOneShot(Enums.SfxClips.HitBlock);
            GameObject backGroundObject = GameObject.FindWithTag("BackGround");
            backGroundObject.transform.DOShakePosition(1f, new Vector3(5f, 5f, 0f), 10, 90f);

            isFalling = true;

            topBlockObjectInstance.SetActive(true);
            topBlockObjectInstance.transform.position = transform.position;
            topBlockObjectInstance.transform.rotation = transform.rotation;

            Rigidbody2D rb = topBlockObjectInstance.GetComponent<Rigidbody2D>();
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            fallingProxyBlockObjectPool.Return(gameObject);
            EventBus.Instance.Publish(Consts.BLOCK_LANDED); // 블럭이 떨어졌음을 알리기
            topBlockObjectInstance = null;

            effectObjectPool.Get(this.gameObject.transform); // 효과

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

        while (isFalling)
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

    public void StopFalling()
    {
        isFalling = false;
    }
}
