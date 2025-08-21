using System.Collections.Generic;
using UnityEngine;

public class MineralObj : MonoBehaviour
{
    [Header("컴포넌트")]
    protected Rigidbody2D rb;
    protected Collider2D col;

    [Header("주요 프로퍼티")]
    protected const float TERMINAL_SPEED = 5f;
    protected const float MASS = 10;
    protected const float DEAD_LINE_POS = -6f;
    protected const float HIT_FORCE = 5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

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