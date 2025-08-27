using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NaturalGasObject : MonoBehaviour
{
    // 상수
    private const float windStrength = 2f; // 바람 세기 (유닛/초)

    // 프리팹
    [Header("프리팹")]
    [SerializeField] private ParticleSystem prefab_NaturalGasParticle; // 천연가스 파티클

    // private 필드(인스펙터 노출)
    [SerializeField] private Vector2 windDirection = Vector2.right; // 바람 방향

    // private 필드
    private List<Transform> affectedObjects = new List<Transform>(); // 천연가스에 맞는 블럭들
    private Dictionary<Transform, Rigidbody2D> rigidbodyMap = new Dictionary<Transform, Rigidbody2D>(); // 천연가스에 맞는 블럭들(rigidbody가 있는)

    // 유니티 콜백
    private void OnEnable()
    {
        prefab_NaturalGasParticle.Play();
    }
    private void FixedUpdate()
    {
        Vector2 force = windDirection.normalized * windStrength;
        Vector3 move = (Vector3)(force * Time.fixedDeltaTime);

        foreach (var obj in affectedObjects)
        {
            if (obj == null) continue;

            if (rigidbodyMap.TryGetValue(obj, out Rigidbody2D rb) && rb != null) // Rigidbody가 있으면 AddForce로 밀기
            {
                rb.AddForce(force, ForceMode2D.Force);
            }
            else // Rigidbody가 없으면 Transform 이동
            {
                obj.position -= move;
            }
        }
    }

    // Etc
    public void TurnOffNaturalGas()
    {
        StartCoroutine(TurnOffNaturalGasCoroutine());
    }
    private IEnumerator TurnOffNaturalGasCoroutine()
    {
        if (prefab_NaturalGasParticle != null)
        {
            prefab_NaturalGasParticle.Stop();
        }

        // PolygonCollider2D 비활성화
        PolygonCollider2D polyCollider = GetComponent<PolygonCollider2D>();
        if (polyCollider != null)
        {
            polyCollider.enabled = false;
        }

        yield return new WaitForSeconds(2f);

        // 오브젝트 비활성화
        gameObject.SetActive(false);
    }

    // 물리 콜백
    private void OnTriggerEnter2D(Collider2D collision) // 리스트에 collision 오브젝트를 추가
    {
        if (!affectedObjects.Contains(collision.transform))
        {
            affectedObjects.Add(collision.transform);

            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            rigidbodyMap[collision.transform] = rb;
        }
    }
    private void OnTriggerExit2D(Collider2D collision) // 리스트에서 오브젝트를 제거
    {
        if (affectedObjects.Contains(collision.transform))
        {
            affectedObjects.Remove(collision.transform);
            
            if (rigidbodyMap.ContainsKey(collision.transform))
            {
                rigidbodyMap.Remove(collision.transform);
            }
        }
    }
}
