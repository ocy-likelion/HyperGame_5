using UnityEngine;

public class BlockOnlyTop : MonoBehaviour
{
    private EJewelType jewelType;
    private Rigidbody2D rigid;
    private readonly float slideForce = -7f;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void InstantiateProxyObject(EJewelType _jewelType, Transform _parent, Sprite sprite)
    {
        jewelType = _jewelType;
        this.transform.SetParent(_parent);
        GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public void ApplySlideMotion() // 크리스탈 기믹
    {
        rigid.linearVelocity = new Vector2(slideForce, slideForce);
    }
}
