using UnityEngine;

public class MoleBlockObject : SabotageEventBlockObject
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Block"))
        {
            Vector2 dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            rb.AddForce(dir.normalized * HIT_FORCE, ForceMode2D.Impulse);
        }
    }
}
