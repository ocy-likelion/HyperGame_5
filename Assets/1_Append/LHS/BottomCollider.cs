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

        if (collision.gameObject.CompareTag("Gold")) penalty = uiManager.GoldScore;
        else if (collision.gameObject.CompareTag("Silver")) penalty = uiManager.SilverScore;
        else if (collision.gameObject.CompareTag("Bronze")) penalty = uiManager.BronzeScore;
        else if (collision.gameObject.CompareTag("Stone")) penalty = uiManager.StoneScore;

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
