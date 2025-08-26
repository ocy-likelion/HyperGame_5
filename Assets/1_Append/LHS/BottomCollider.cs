using UnityEngine;

public class BottomCollider : MonoBehaviour
{
    [SerializeField] UIManager uiManager; // 인스펙터에서 연결 권장
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
            int to = Mathf.Max(0, from - uiManager.BlockScore);

            // 점수 반영은 GameManager
            gameManager.score = to;

            // UI는 시각효과만
            uiManager.AnimateScoreChange(from, to);

            playManager.BlockList.Remove(collision.gameObject);
            Destroy(collision.gameObject);
        }
    }
}
