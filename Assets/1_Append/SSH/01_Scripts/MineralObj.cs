using System.Collections.Generic;
using UnityEngine;

public class MineralObj : MonoBehaviour
{
    [Header("컴포넌트")]
    Rigidbody2D rb;
    Collider2D col;
    PhysicsMaterial2D pM;

    [Header("주요 프로퍼티")]
    const float TERMINAL_SPEED = 5f;
    const float MASS = 10;
    const float DEAD_LINE_POS = -6f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        pM = col.sharedMaterial;

        rb.linearVelocity = Vector2.down * TERMINAL_SPEED;
    }
    void Update()
    {
        if (transform.position.y < DEAD_LINE_POS)
        {
            Destroy(gameObject);
        }
    }
    void FixedUpdate()
    {
        Vector2 vel = rb.linearVelocity;

        if (vel.magnitude > TERMINAL_SPEED)
        {
            rb.linearVelocity = vel.normalized * TERMINAL_SPEED;
        }
    }
}
