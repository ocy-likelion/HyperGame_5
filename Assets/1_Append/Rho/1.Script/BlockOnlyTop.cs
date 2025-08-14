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

    public void InstantiateProxyObject(EJewelType _jewelType, Transform _parent)
    {
        jewelType = _jewelType;
        this.transform.SetParent(_parent);
    }

    public void ApplySlideMotion() // Å©¸®½ºÅ» ±â¹Í
    {
        rigid.linearVelocity = new Vector2(slideForce, slideForce);
    }
}
