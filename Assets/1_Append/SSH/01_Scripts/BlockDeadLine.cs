using UnityEngine;

public class BlockDeadLine : MonoBehaviour
{
    [SerializeField] PlayManager playManager;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Block"))
        {
            Destroy(collision.gameObject);
        }
    }
}
