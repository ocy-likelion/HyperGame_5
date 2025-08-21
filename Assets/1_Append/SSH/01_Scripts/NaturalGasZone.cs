using System.Collections.Generic;
using UnityEngine;

public class NaturalGasZone : MonoBehaviour
{
    [SerializeField] Vector2 windDirection = Vector2.right; // 바람 방향
    [SerializeField] float windStrength = 2f; // 바람 세기 (유닛/초)
    [SerializeField] ParticleSystem particle;

    private List<Transform> affectedObjects = new List<Transform>();
    private Dictionary<Transform, Rigidbody2D> rigidbodyMap = new Dictionary<Transform, Rigidbody2D>();

    void OnEnable()
    {
        particle.Play();
    }

    void FixedUpdate()
    {
        Vector2 force = windDirection.normalized * windStrength;
        Vector3 move = (Vector3)(force * Time.fixedDeltaTime);

        foreach (var obj in affectedObjects)
        {
            if (obj == null) continue;

            if (rigidbodyMap.TryGetValue(obj, out Rigidbody2D rb) && rb != null)
            {
                // Rigidbody가 있으면 AddForce로 밀기
                rb.AddForce(force, ForceMode2D.Force);
                Debug.Log("리지드바디 미는중");
            }
            else
            {
                // Rigidbody가 없으면 Transform 이동
                obj.position -= move;
                Debug.Log("프록시 미는중");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!affectedObjects.Contains(collision.transform))
        {
            affectedObjects.Add(collision.transform);

            // Rigidbody2D가 있으면 매핑
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            rigidbodyMap[collision.transform] = rb;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (affectedObjects.Contains(collision.transform))
        {
            affectedObjects.Remove(collision.transform);

            // Rigidbody 정보도 삭제
            if (rigidbodyMap.ContainsKey(collision.transform))
                rigidbodyMap.Remove(collision.transform);
        }
    }
}
