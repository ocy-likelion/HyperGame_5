using UnityEngine;

public class BottomCollider : MonoBehaviour
{
    // private 필드(인스펙터 노출)
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PlayManager playManager;
    [SerializeField] private GameManager gameManager;

    // 물리 콜백
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Block"))
        {
            int from = gameManager.Score;
            int to = Mathf.Max(0, from - uiManager.BlockScore);

            
            gameManager.SetScore(to); // 점수 반영

            // UI는 시각효과만
            uiManager.AnimateScoreChange(from, to);

            playManager.BlockList.Remove(collision.gameObject);
            Destroy(collision.gameObject);
        }
    }
}
