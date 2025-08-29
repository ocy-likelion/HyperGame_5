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

            uiManager.AnimateScoreChange(from, to); // UI 변경

            DamagePostEffect.Instance.PlayDamageEffect(); // 포스트 프로세싱

            playManager.BlockList.Remove(collision.gameObject);
            Destroy(collision.gameObject);
        }
    }
}
