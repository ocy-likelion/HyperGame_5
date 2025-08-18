using UnityEngine;

public class BottomCollider : MonoBehaviour
{
    GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        int penalty = 0;

        if (collision.gameObject.CompareTag("Gold"))
        {
            penalty = 500;
        }
        else if (collision.gameObject.CompareTag("Silver"))
        {
            penalty = 300;
        }
        else if (collision.gameObject.CompareTag("Bronze"))
        {
            penalty = 200;
        }
        else if (collision.gameObject.CompareTag("Stone"))
        {
            penalty = 100;
        }

        gameManager.score = Mathf.Max(gameManager.score - penalty, 0);
        Destroy(collision.gameObject);
    }
}
