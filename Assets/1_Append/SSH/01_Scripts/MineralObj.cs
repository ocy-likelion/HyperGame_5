using System.Collections.Generic;
using UnityEngine;

public class MineralObj : MonoBehaviour
{
    [Header("컴포넌트")]
    Rigidbody2D rb;
    Collider2D col;
    PhysicsMaterial2D pM;

    [Header("주요 프로퍼티")]
    const float MAX_SPEED = 5f;
    const float MASS = 10;
    const float CRITICAL_POINT = -6f;
    [SerializeField] SO_Mineral so_Mineral;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        pM = col.sharedMaterial;
    }

    void Update()
    {
        if (transform.position.y < CRITICAL_POINT)
        {
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        Vector2 vel = rb.linearVelocity;

        if (vel.magnitude > MAX_SPEED)
        {
            rb.linearVelocity = vel.normalized * MAX_SPEED;
        }
    }
}
