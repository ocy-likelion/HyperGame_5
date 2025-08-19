using UnityEngine;

public class BottomCollider : MonoBehaviour
{
    [SerializeField] UIManager uiManager; // 인스펙터에서 연결 권장
    GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.FindFirstObjectByType<GameManager>();
        if (uiManager == null) uiManager = GameObject.FindFirstObjectByType<UIManager>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameManager == null || uiManager == null) return;

        int penalty = 0;

        if (collision.gameObject.CompareTag("Block")) penalty = uiManager.BlockScore;
     
        if (penalty > 0)
        {
            int from = gameManager.score;
            int to = Mathf.Max(0, from - penalty);

            // 점수 반영은 GameManager
            gameManager.score = to;

            // UI는 시각효과만
            uiManager.AnimateScoreChange(from, to);
        }

        Destroy(collision.gameObject);
    }
}
