using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class CharacterClimb : MonoBehaviour
{
    [Header("참조")]
    public PlayManager playManager;
    [SerializeField] private Camera cam;                 // 비워두면 자동으로 Camera.main 사용

    [Header("이동/등반 설정")]
    [SerializeField] private float climbSpeed = 2.8f;    // 타겟(최상단)으로 향하는 실제 이동 속도
    [SerializeField] private float riseSpeed = 3.0f;     // 표시 높이가 상승할 때 보간 속도
    [SerializeField] private float fallSpeed = 10f;      // 표시 높이가 하강할 때 보간 속도
    [SerializeField] private bool instantFall = true;    // 타워가 낮아졌을 때 즉시 따라갈지
    [SerializeField] private float hangOffsetY = 0.25f;  // 최상단에서 얼마나 위에 매달릴지
    [SerializeField] private float sideOffsetX = 0.0f;   // 좌우로 살짝 빗겨 매달리고 싶으면 사용

    [Header("리스폰 설정")]
    [SerializeField] private float offscreenMargin = 0.08f; // 화면 아래로 이만큼 더 나가면 리스폰
    [SerializeField] private float respawnDelay = 2.0f;      // 떨어진 후 재매달리기 지연

    // 내부 상태
    private float displayedHeight;   // 화면 연출용(부드러운 높이)
    private bool isRespawning = false;
    private Rigidbody2D rb;          // 있으면 MovePosition 사용, 없으면 transform 이동

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (playManager == null)
        {
            Debug.LogError("PlayManager 참조가 필요합니다 (CharacterClimb).");
            enabled = false;
            return;
        }

        // 시작 시 현재 타워 높이 기준으로 세팅
        displayedHeight = playManager.CurrentTowerHeight;
        // 캐릭터가 땅 밑에서 튀어나오는 걸 방지하려면 시작 위치도 살짝 맞춰준다(선택)
        Vector3 top = playManager.HighestTopPoint;
        if (top != Vector3.zero)
        {
            Vector3 start = new Vector3(top.x + sideOffsetX, top.y + hangOffsetY, transform.position.z);
            if (rb) rb.position = start;
            else transform.position = start;
        }
    }

    void Update()
    {
        if (playManager == null || cam == null) return;

        // 1) 목표 높이(타워 꼭대기 y) 계산
        float targetHeight = playManager.CurrentTowerHeight;
        SmoothDisplayedHeight(targetHeight);

        // 2) 타깃 앵커 위치(최상단 블록의 x, 표시높이 + 오프셋)
        Vector3 top = playManager.HighestTopPoint;
        float targetX = (top == Vector3.zero) ? transform.position.x : top.x + sideOffsetX;
        Vector3 anchor = new Vector3(targetX, displayedHeight + hangOffsetY, transform.position.z);

        // 3) 앵커를 향해 이동 (대각선 포함 → 블록이 비스듬히 쌓여도 그쪽으로 기어감)
        MoveTowards(anchor, climbSpeed);

        // 4) 화면 밖(아래)으로 떨어지면 리스폰 루틴 실행
        if (!isRespawning && IsBelowScreen(out Vector3 vp))
        {
            StartCoroutine(RespawnAtTopAfterDelay());
        }
    }

    // 표시 높이 보간
    private void SmoothDisplayedHeight(float targetHeight)
    {
        if (displayedHeight < targetHeight)
        {
            displayedHeight = Mathf.MoveTowards(displayedHeight, targetHeight, riseSpeed * Time.deltaTime);
        }
        else if (displayedHeight > targetHeight)
        {
            displayedHeight = instantFall
                ? targetHeight
                : Mathf.MoveTowards(displayedHeight, targetHeight, fallSpeed * Time.deltaTime);
        }
    }

    // 실제 이동(물리 있으면 MovePosition)
    private void MoveTowards(Vector3 target, float speed)
    {
        if (rb != null && rb.isKinematic == false)
        {
            Vector2 next = Vector2.MoveTowards(rb.position, (Vector2)target, speed * Time.deltaTime);
            rb.MovePosition(next);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }
    }

    // 화면 아래로 사라졌는지 체크
    private bool IsBelowScreen(out Vector3 viewportPos)
    {
        viewportPos = cam.WorldToViewportPoint(transform.position);
        // z < 0(카메라 뒤)도 배제
        return (viewportPos.z > 0f) && (viewportPos.y < -offscreenMargin);
    }

    // 2초 뒤 최상단으로 리스폰(매달리기)
    private IEnumerator RespawnAtTopAfterDelay()
    {
        isRespawning = true;

        // 보이는 요소 잠시 끄기(지우는 느낌). 오브젝트 자체를 비활성화하면 코루틴도 멈추니 주의.
        SetVisible(false);

        yield return new WaitForSeconds(respawnDelay);

        // 최신 최상단 위치 재확인 후 그 위에 매달리기
        Vector3 top = playManager.HighestTopPoint;
        Vector3 reattach = (top == Vector3.zero)
            ? transform.position
            : new Vector3(top.x + sideOffsetX, top.y + hangOffsetY, transform.position.z);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = reattach;
        }
        else
        {
            transform.position = reattach;
        }

        SetVisible(true);
        isRespawning = false;
    }

    private void SetVisible(bool on)
    {
        // 스프라이트/콜라이더만 꺼서 "삭제된 것처럼" 보이게
        var rends = GetComponentsInChildren<Renderer>(true);
        foreach (var r in rends) r.enabled = on;

        var cols = GetComponentsInChildren<Collider2D>(true);
        foreach (var c in cols) c.enabled = on;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (playManager == null) return;
        Gizmos.color = Color.cyan;
        Vector3 top = Application.isPlaying ? playManager.HighestTopPoint : Vector3.zero;
        if (top != Vector3.zero)
        {
            Vector3 anchor = new Vector3(top.x + sideOffsetX,
                                         (Application.isPlaying ? displayedHeight : top.y) + hangOffsetY,
                                         transform.position.z);
            Gizmos.DrawWireSphere(anchor, 0.08f);
            Gizmos.DrawLine(transform.position, anchor);
        }
    }
#endif
}
