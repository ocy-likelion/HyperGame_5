using System.Collections.Generic;
using UnityEngine;

public class NaturalGasZone : MonoBehaviour
{
    [SerializeField] Vector2 windDirection = Vector2.right; // 바람 방향
    [SerializeField] float windStrength = 5f; // 바람 세기

    List<Rigidbody2D> affectedRbs = new List<Rigidbody2D>();

    void FixedUpdate()
    {
        foreach (var rb in affectedRbs)
        {
            rb.AddForce(windDirection.normalized * windStrength, ForceMode2D.Force); // 질량에 상관없이 밀고 싶다면 추후 rb.mass를 곱할 것
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.attachedRigidbody;
        if (rb != null && !affectedRbs.Contains(rb))
        {
            affectedRbs.Add(rb);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.attachedRigidbody;
        if (rb != null && affectedRbs.Contains(rb))
        {
            affectedRbs.Remove(rb);
        }
    }
}
