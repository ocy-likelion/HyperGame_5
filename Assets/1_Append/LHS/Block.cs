using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Block : MonoBehaviour
{
    private TowerManager tower;

    private void Start()
    {
        // TowerManager 자동 탐색 후 등록
        tower = FindAnyObjectByType<TowerManager>();
        tower?.RegisterBlock(gameObject);
    }

    private void OnDestroy()
    {
        tower?.UnregisterBlock(gameObject);
    }
}