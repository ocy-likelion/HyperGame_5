using DG.Tweening;
using System.Collections;
using UnityEngine;

public class TopBlockObject : MonoBehaviour
{
    // private 필드(컴포넌트)
    private Rigidbody2D rb;

    // private 필드
    private Coroutine coroutine;

    // 유니티 콜백
    private void Awake()
    {
        TryGetComponent(out rb);
    }

    public void InitTopBlockObject(Transform parent, Sprite sprite, EffectObjectPool effectObjectPool) // 블럭 초기화
    {
        transform.SetParent(parent);
        GetComponent<SpriteRenderer>().sprite = sprite;
        GetComponent<SpriteOutlineCollider>().BuildCollider();
        effectObjectPool.Get(this.gameObject.transform);
    }
    IEnumerator TopBlockSettingCoroutine() // 블럭 안정화 작업 코루틴
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

    // 물리 콜백
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
}
