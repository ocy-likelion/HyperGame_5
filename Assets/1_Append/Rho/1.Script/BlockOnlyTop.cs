using DG.Tweening;
using System.Collections;
using UnityEngine;

public class BlockOnlyTop : MonoBehaviour
{
    private MineralDataManager mineralDataManager;
    private Rigidbody2D rb;
    private readonly float slideForce = -7f;
    private Coroutine coroutine;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void InstantiateProxyObject(Transform _parent, Sprite sprite, EffectObjectPool _effectObjectPool, MineralDataManager _mineralDataManager)
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

            rb.gravityScale = 0f;
            coroutine = StartCoroutine(TopBlockSettingCoroutine());
        }
    }

    IEnumerator TopBlockSettingCoroutine()
    {
        yield return null;

        rb.gravityScale = 1f;
    }
}
