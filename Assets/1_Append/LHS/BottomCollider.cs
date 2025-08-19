using UnityEngine;

public class BottomCollider : MonoBehaviour
{
    [SerializeField] UIManager uiManager;
    [SerializeField] PlayManager playManager;
    GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.FindFirstObjectByType<GameManager>();
        if (uiManager == null) uiManager = GameObject.FindFirstObjectByType<UIManager>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameManager == null || uiManager == null) return;


        if (collision.gameObject.CompareTag("Block"))
        {
            int from = gameManager.score;
            int to = from - uiManager.BlockDropScore;

            gameManager.score = to; // 점수 반영
            
            uiManager.AnimateScoreChange(from, to); // UI 효과
        }

        playManager.blockList.Remove(collision.gameObject); // 블럭 리스트에서 제거

        Destroy(collision.gameObject);
    }
}
