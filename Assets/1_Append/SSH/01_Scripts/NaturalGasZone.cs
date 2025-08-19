using System.Collections.Generic;
using UnityEngine;

public class NaturalGasZone : MonoBehaviour
{
    [SerializeField] Vector2 windDirection = Vector2.right; // 바람 방향
    [SerializeField] float windStrength = 2f; // 바람 세기 (유닛/초)
    [SerializeField] ParticleSystem particle; // 바람 세기 (유닛/초)

    List<Transform> affectedObjects = new List<Transform>();

    void OnEnable()
    {
        particle.Play();
    }

    void FixedUpdate()
    {
        Vector3 move = (Vector3)windDirection.normalized * windStrength * Time.fixedDeltaTime;

        foreach (var obj in affectedObjects)
        {
            if (obj != null)
                obj.position -= move;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Rigidbody가 없어도 Transform을 추가
        if (!affectedObjects.Contains(collision.transform))
        {
            affectedObjects.Add(collision.transform);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (affectedObjects.Contains(collision.transform))
        {
            affectedObjects.Remove(collision.transform);
        }
    }
}
