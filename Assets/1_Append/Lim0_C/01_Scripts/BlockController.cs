using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlockController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    // 상수
    private const float GROUND_Y = -3f; // 기반(라인의 마지노선)
    private const float RAY_DISTANCE = 10f; // 레이를 쏠 최대 거리
    private const float LINE_Y_OFFSET = 0.05f; // 라인의 오프셋 값

    // private 필드(인스펙터 노출)
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private GameObject predictionLinePrefab;

    // private 필드
    private Vector3 _blockSpawnPosition;
    private GameObject _currentBlock;
    private Transform _predictionLineLeft;
    private Transform _predictionLineRight;
    private bool _isPointerDown;

    // public Getter
    public Vector3 BlockSpawnPosition => _blockSpawnPosition;

    // 유니티 콜백
    private void OnEnable()
    {
        EventBus.Instance.Subscribe<GameObject>("SpawnBlock", InitBlockPosition);
    }
    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<GameObject>("SpawnBlock", InitBlockPosition);
    }
    private void Start()
    {
        InitPredictionLine();
    }
    private void Update()
    {
        if (_isPointerDown)
        {
            DrawPredictionLine(); // 블럭 좌우 예측선 그리기
        }
    }

    // 블럭
    private void InitBlockPosition(GameObject newBlock) // 카메라 위치에 따른 블럭의 위치 설정
    {
        if (_currentBlock != null) return; // 블록이 이미 존재하면 중복 생성 방지

        _currentBlock = newBlock;
        _currentBlock.transform.position = GetBlockSpawnPosition();

        if (_isPointerDown)
        {
            TogglePredictionLines(true);
        }
    }
    private Vector3 GetBlockSpawnPosition()
    {
        var screenPosition = new Vector3(Screen.width * 0.5f, Screen.height * 0.8f, Consts.CAMERA_OFFSET);
        var worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f; // 2D 오브젝트는 z=0

        _blockSpawnPosition = worldPosition;
        return worldPosition;
    }
    private void SetBlockPosition(Vector3 eventDataPos)
    {
        Vector3 viewportPos = mainCamera.ScreenToViewportPoint(eventDataPos);
        viewportPos.x = Mathf.Clamp(viewportPos.x, 0.1f, 0.9f);
        viewportPos.z = Consts.CAMERA_OFFSET;
        var clampedPos = mainCamera.ViewportToWorldPoint(viewportPos);
        _currentBlock.transform.position =
            new Vector3(clampedPos.x, _currentBlock.transform.position.y, 0);
    }

    // 예측선
    private void InitPredictionLine() // 블럭 좌우 예측선 초기화
    {
        _predictionLineLeft = Instantiate(predictionLinePrefab).GetComponent<Transform>();
        _predictionLineLeft.gameObject.SetActive(false);

        _predictionLineRight = Instantiate(predictionLinePrefab).GetComponent<Transform>();
        _predictionLineRight.gameObject.SetActive(false);
    }
    private void DrawPredictionLine()
    {
        if (_currentBlock == null) return;

        Collider2D blockCollider = _currentBlock.GetComponent<Collider2D>();
        
        UpdateLine(_predictionLineLeft, new Vector2(blockCollider.bounds.min.x, blockCollider.bounds.min.y - LINE_Y_OFFSET)); // 좌측 라인
        UpdateLine(_predictionLineRight, new Vector2(blockCollider.bounds.max.x, blockCollider.bounds.min.y - LINE_Y_OFFSET)); // 우측 라인
    }
    private void UpdateLine(Transform lineObj, Vector2 start)
    {
        LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();

        // 아래 방향으로 레이 발사
        RaycastHit2D hit = Physics2D.Raycast(start, Vector2.down, RAY_DISTANCE);

        float endY = GROUND_Y;

        if (hit.collider != null)
        {
            // 태그가 Platform 또는 Block일 때만 충돌 지점으로 라인 종료
            if (hit.collider.CompareTag("Platform") || hit.collider.CompareTag("Block"))
            {
                endY = hit.point.y;
            }
        }

        // 시작점과 끝점 업데이트
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, new Vector3(start.x, Mathf.Max(endY, GROUND_Y), 0));
    }
    private void TogglePredictionLines(bool isActive)
    {
        _predictionLineLeft.gameObject.SetActive(isActive);
        _predictionLineRight.gameObject.SetActive(isActive);
    }

    // 입력 핸들
    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;

        if (_currentBlock == null) return;

        SetBlockPosition(eventData.position);
        TogglePredictionLines(true);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        _isPointerDown = false;

        if (_currentBlock == null) return;

        _currentBlock.GetComponent<FallingProxyBlockObject>().StopFalling();
        _currentBlock = null;

        TogglePredictionLines(false);

        EventBus.Instance.Publish("RespawnBlock");
        RealSoundManager.Instance.PlayOneShot(Enums.SfxClips.DropBlock);
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (_currentBlock == null) return;

        SetBlockPosition(eventData.position);
    }
}
