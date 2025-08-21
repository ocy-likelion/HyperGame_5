using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LavaZone2D : MonoBehaviour
{
    [Header("What counts as a block?")]
    [Tooltip("이 레이어에 있는 물체만 블록으로 취급 (없으면 Rigidbody2D 있으면 허용)")]
    [SerializeField] private LayerMask blockLayers = ~0; // 기본: 모두 허용

    [Header("Splash FX")]
    [SerializeField] private GameObject splashPrefab; // SimpleSplashFX 프리팹
    [SerializeField] private AudioClip splashSfx;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.8f;

    [Header("Behavior")]
    [Tooltip("용암에 닿으면 블록을 제거할지 여부")]
    [SerializeField] private bool destroyOnContact = true;
    [SerializeField] private float destroyDelay = 0.15f;

    [Header("Intensity (by fall speed)")]
    [Tooltip("이 속도 이상이면 큰 스플래시")]
    [SerializeField] private float bigSplashSpeed = 5f; 
    [Tooltip("이 속도 이하면 아주 작은 스플래시")]
    [SerializeField] private float tinySplashSpeed = 0.5f;

    // 같은 블록이 중복으로 여러 번 트리거되는 것 방지
    private readonly HashSet<int> _entered = new HashSet<int>();

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnValidate()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // 레이어 체크
        bool layerOK = (blockLayers.value & (1 << other.gameObject.layer)) != 0;
        var rb = other.attachedRigidbody;

        // 최소 조건: 지정 레이어거나 Rigidbody2D가 있어야 "블록"으로 간주
        if (!layerOK && rb == null) return;

        int id = other.gameObject.GetInstanceID();
        if (_entered.Contains(id)) return; // 중복 방지
        _entered.Add(id);

        // 스피드 기반 강도 계산
        float fallSpeed = rb != null ? Mathf.Abs(rb.linearVelocity.y) : 0f;
        float t = Mathf.InverseLerp(tinySplashSpeed, bigSplashSpeed, fallSpeed); // 0~1

        // 스플래시 생성 위치: 용암 표면 y에 other의 x 맞추기
        Vector3 pos = other.bounds.center;
        pos.y = transform.position.y; // 용암 표면 y(= 이 오브젝트의 y)

        if (splashPrefab != null)
        {
            var go = Instantiate(splashPrefab, pos, Quaternion.identity);
            var splash = go.GetComponent<SimpleSplashFX>();
            if (splash != null) splash.Play(t);
        }

        if (splashSfx != null)
        {
            // 카메라 기준으로 재생 (볼륨은 속도 t 반영)
            var camPos = Camera.main != null ? Camera.main.transform.position : pos;
            AudioSource.PlayClipAtPoint(splashSfx, camPos, sfxVolume * Mathf.Lerp(0.4f, 1f, t));
        }

        if (destroyOnContact)
        {
            // 살짝 지연 후 제거 (스플래시가 보이게)
            Destroy(other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject, destroyDelay);
        }
    }
}
