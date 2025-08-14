using UnityEngine;

public class BlockOnlyTop : MonoBehaviour
{
    private MineralTypeEnum mineralType;
    private Rigidbody2D rigid;
    private readonly float slideForce = -7f;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void InstantiateProxyObject(MineralTypeEnum _mineralType, Transform _parent)
    {
        mineralType = _mineralType;
        this.transform.SetParent(_parent);
    }

    public void ApplySlideMotion() // 크리스탈 기믹
    {
        rigid.linearVelocity = new Vector2(slideForce, slideForce);
    }
}
