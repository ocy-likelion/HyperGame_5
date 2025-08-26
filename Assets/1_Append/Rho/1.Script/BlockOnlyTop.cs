using DG.Tweening;
using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class BlockOnlyTop : MonoBehaviour
{
    private SpawnBlockManager mineralDataManager;
    private Rigidbody2D rb;
    private readonly float slideForce = -7f;
    private Coroutine coroutine;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void InstantiateProxyObject(Transform _parent, Sprite sprite, EffectObjectPool _effectObjectPool, SpawnBlockManager _mineralDataManager)
    {
        this.transform.SetParent(_parent);
        GetComponent<SpriteRenderer>().sprite = sprite;
        GetComponent<SpriteOutlineCollider>().BuildCollider();
        _effectObjectPool.Get(this.gameObject.transform);
        mineralDataManager = _mineralDataManager;
    }

    public void ApplySlideMotion() // 크리스탈 기믹
    {
        rb.linearVelocity = new Vector2(slideForce, slideForce);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Block") || collision.gameObject.CompareTag("Platform"))
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            StartCoroutine(TopBlockSettingCoroutine());
        }
    }

    IEnumerator TopBlockSettingCoroutine()
    {
        // 중력을 0에서 1로 점진적으로 증가
        float duration = 0.5f; // 1초
        float elapsed = 0f;
        float startGravity = 0f;
        float targetGravity = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime; // 매 프레임 시간 누적
            float t = elapsed / duration; // 0 ~ 1 비율
            rb.gravityScale = Mathf.Lerp(startGravity, targetGravity, t);
            yield return null; // 다음 프레임까지 대기
        }

        // 마지막에 정확히 1로 고정
        rb.gravityScale = targetGravity;

        // 코루틴 종료
        coroutine = null;
    }
}
