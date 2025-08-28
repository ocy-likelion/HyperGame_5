using System.Collections.Generic;
using UnityEngine;

public class SabotageEventBlockObject : MonoBehaviour
{
    // 상수
    protected const float TERMINAL_SPEED = 5f;
    protected const float MASS = 10;
    protected const float DEAD_LINE_POS = -6f;
    protected const float HIT_FORCE = 30f;

    // protected 필드(컴포넌트)
    protected Rigidbody2D rb;
    protected Collider2D col;

    // 유니티 콜백
    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out col);

        rb.linearVelocity = Vector2.down * TERMINAL_SPEED; // 시작하자마자 종단 속도로 움직이기
    }
    private void Update()
    {
        if (transform.position.y < DEAD_LINE_POS)
        {
            Destroy(gameObject);
        }
    }
    private void FixedUpdate()
    {
        Vector2 vel = rb.linearVelocity;

        if (vel.magnitude > TERMINAL_SPEED)
        {
            rb.linearVelocity = vel.normalized * TERMINAL_SPEED;
        }
    }
}
