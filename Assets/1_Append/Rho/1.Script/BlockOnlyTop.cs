using DG.Tweening;
using System.Collections;
using UnityEngine;

public class BlockOnlyTop : MonoBehaviour
{
    private Rigidbody2D rb;
    private readonly float slideForce = -7f;
    private Coroutine coroutine;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void InstantiateProxyObject(Transform _parent, Sprite sprite, EffectObjectPool _effectObjectPool)
    {
        this.transform.SetParent(_parent);
        GetComponent<SpriteRenderer>().sprite = sprite;
        GetComponent<SpriteOutlineCollider>().BuildCollider();
        _effectObjectPool.Get(this.gameObject.transform);
    }

    public void ApplySlideMotion() // 크리스탈 기믹
    {
        rb.linearVelocity = new Vector2(slideForce, slideForce);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Block") || collision.gameObject.CompareTag("Platform"))
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            coroutine = StartCoroutine(TopBlockSettingCoroutine());
        }
    }

    IEnumerator TopBlockSettingCoroutine()
    {
        Rigidbody2D rb = this.gameObject.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        yield return new WaitForSeconds(0.1f);

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

}
