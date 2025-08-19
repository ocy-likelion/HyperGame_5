using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class DraggableBlock : MonoBehaviour
{
    [Header("Follow (preview)")]
    public float followLerp = 20f;      // 프리뷰 이동 보정(부드럽게)
    public float previewZ = 0f;         // 프리뷰 시 Z 고정

    [Header("Drop physics")]
    public float dropGravity = 1f;      // 드랍 후 gravityScale
    public RigidbodySleepMode2D sleepMode = RigidbodySleepMode2D.StartAwake;

    public bool IsPreview { get; private set; }

    public event Action<DraggableBlock> OnDropped;   // 드랍 완료 콜백

    Camera _cam;
    Rigidbody2D _rb;
    Collider2D _col;
    SpriteRenderer _sr;

    void Awake()
    {
        _cam = Camera.main;
        _rb  = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _sr  = GetComponent<SpriteRenderer>();

        // 어떤 프리팹 설정이 와도 강제로 "프리뷰 상태"로 초기화
        EnterPreview();
    }

    void Update()
    {
        if (!IsPreview) return;

        // 마우스/터치 위치로 프리뷰 이동
        Vector3 mpos = Input.mousePosition;
        Vector3 world = _cam ? _cam.ScreenToWorldPoint(mpos) : new Vector3(0,0,0);
        world.z = previewZ;

        // Rigidbody2D는 MovePosition으로 이동(프리뷰 중 Kinematic)
        _rb.MovePosition(Vector2.Lerp(_rb.position, (Vector2)world, followLerp * Time.deltaTime));

        // 마우스/터치 릴리즈 시 드랍
        if (Input.GetMouseButtonUp(0) || ReleasedTouch())
        {
            Drop();
        }
    }

    bool ReleasedTouch()
    {
        // 간단 터치 릴리즈 감지(신규 입력 시스템이든 레거시든 대부분 호환)
        if (Input.touchCount == 0) return false;
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.touches[i].phase == TouchPhase.Ended) return true;
        }
        return false;
    }

    void EnterPreview()
    {
        IsPreview = true;

        // 프리뷰: 중력 X, Kinematic, 충돌 끔(탑과 간섭 방지)
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.sleepMode = sleepMode;

        if (_col) _col.enabled = false;

        // 혹시 투명으로 보였다면 알파 보정(원하는 색 있으면 프리팹에서 설정)
        if (_sr && _sr.color.a < 0.95f)
        {
            Color c = _sr.color;
            c.a = 1f;
            _sr.color = c;
        }
    }

    public void Drop()
    {
        if (!IsPreview) return;

        IsPreview = false;

        // 드랍: Dynamic + 중력 ON + 콜라이더 ON
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = dropGravity;
        if (_col) _col.enabled = true;

        // 드랍 알림 → 스포너가 다음 블록 생성 트리거
        OnDropped?.Invoke(this);
        // 더블 방지를 위해 이벤트 구독자 초기화(선택)
        OnDropped = null;
    }
}
