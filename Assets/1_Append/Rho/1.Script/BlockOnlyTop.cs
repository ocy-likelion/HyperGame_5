using UnityEngine;

public class BlockOnlyTop : MonoBehaviour
{
    private Rigidbody2D rb;
    private readonly float slideForce = -7f;
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
}
