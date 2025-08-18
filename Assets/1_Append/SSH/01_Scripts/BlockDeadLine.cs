using UnityEngine;

public class BlockDeadLine : MonoBehaviour
{
    [SerializeField] PlayManager playManager;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Block"))
        {
            playManager.blockList.Remove(collision.gameObject); // 블럭 리스트에서 제거
            Destroy(collision.gameObject);
        }
    }
}
