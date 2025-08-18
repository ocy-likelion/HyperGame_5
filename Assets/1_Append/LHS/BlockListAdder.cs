using UnityEngine;

public class BlockListAdder : MonoBehaviour
{
    // PlayManager의 블럭 리스트에 추가하기 위한 일회용 컴포넌트

    private void OnCollisionEnter2D(Collision2D collision)
    {
        EventBus.Instance.Publish(Consts.BLOCK_LANDED);

        BlockListAdder adder = GetComponent<BlockListAdder>();
        Destroy(adder); // 이 일회용 컴포넌트 삭제
    }
}
