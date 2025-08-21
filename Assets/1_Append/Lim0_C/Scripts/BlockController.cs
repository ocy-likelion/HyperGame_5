using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlockController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField]private Camera mainCamera;
    [SerializeField]private GameObject blockPrefab;
    [SerializeField]private GameObject predictionLinePrefab;

    private Vector3 _blockSpawnPosition;
    private GameObject _currentBlock; 
    private Transform _predictionLineLeft;
    private Transform _predictionLineRight;
    private bool _isPointerDown;

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<GameObject>("SpawnBlock", SpawnBlock);
    }

    void OnDisable()
    {
        EventBus.Instance.Unsubscribe<GameObject>("SpawnBlock", SpawnBlock);
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        if (_isPointerDown)
        {
            DrawPredictionLine(); // 블록 좌우 예측선 그리기
        }
    }

    //초기화
    private void Init()
    {
        // 블록 좌우 예측선 초기화
        _predictionLineLeft = Instantiate(predictionLinePrefab).GetComponent<Transform>();
        _predictionLineLeft.gameObject.SetActive(false);
        _predictionLineRight = Instantiate(predictionLinePrefab).GetComponent<Transform>();
        _predictionLineRight.gameObject.SetActive(false);
    }

    //카메라 이동에 따른 블록 스폰 위치 업데이트 기능
    void UpdateBlockSpawnPosition()
    {
        // 화면 중상단 좌표
        Vector3 screenUpperCenter =
            new Vector3(Screen.width * 0.5f, Screen.height * 0.8f, Consts.CAMERA_OFFSET);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenUpperCenter);
        worldPos.z = 0; // 2D는 z=0 맞추는 경우가 많음
        _blockSpawnPosition = worldPos;
    }
    
    //블록 생성 기능
    private void SpawnBlock(GameObject newBlock)
    {
        if (_currentBlock is not null) return;
        UpdateBlockSpawnPosition();
        _currentBlock = newBlock;
        //_currentBlock.GetComponent<Rigidbody2D>().simulated = false;
        _currentBlock.GetComponent<Transform>().position = _blockSpawnPosition;
            
        if (_isPointerDown)
        {
            _predictionLineLeft.gameObject.SetActive(true);
            _predictionLineRight.gameObject.SetActive(true);
        }
    }

    //터치 입력 시작 이벤트 핸들
    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;
        if (_currentBlock is null) return;
        SetBlockPosition(eventData.position);
        _predictionLineLeft.gameObject.SetActive(true);
        _predictionLineRight.gameObject.SetActive(true);
    }

    //터치 입력 끝 이벤트 핸들
    public void OnPointerUp(PointerEventData eventData)
    {
        _isPointerDown = false;
        if (_currentBlock is null) return;
        //_currentBlock.GetComponent<Rigidbody2D>().simulated = true;
        _currentBlock.GetComponent<BlockDropProxy>().IsEnd = false;
        _currentBlock = null;
        _predictionLineLeft.gameObject.SetActive(false);
        _predictionLineRight.gameObject.SetActive(false);
        EventBus.Instance.Publish("RespawnBlock");
        RealSoundManager.Instance.PlayOneShot(Enums.SfxClips.DropBlock);
    }
    
    //터치 후 드래그 입력 이벤트 핸들
    public void OnDrag(PointerEventData eventData)
    {
        if (_currentBlock is null) return;
        SetBlockPosition(eventData.position);
    }
    
    //터치 입력 받은 위치 기반으로 블록 위치를 바꾸는 기능
    private void SetBlockPosition(Vector3 eventDataPos)
    {
        Vector3 viewportPos = mainCamera.ScreenToViewportPoint(eventDataPos);
        viewportPos.x = Mathf.Clamp(viewportPos.x, 0.1f, 0.9f);
        viewportPos.z = Consts.CAMERA_OFFSET;
        var clampedPos = mainCamera.ViewportToWorldPoint(viewportPos);
        _currentBlock.transform.position =  
            new Vector3(clampedPos.x, _currentBlock.transform.position.y, 0);
    }

    //예측선 그리는 기능
    private void DrawPredictionLine()
    {
        if (_currentBlock == null) return;
            
        Collider2D blockCollider = _currentBlock.GetComponent<Collider2D>(); // 블록 바닥 높이 구하기
        var blockBottom = _currentBlock.transform.position;
        blockBottom.y = blockCollider.bounds.min.y - 0.05f; // 바닥보다 조금 더 낮은 지점에서 predict line 출발
        
        var predictLineLeftRender = _predictionLineLeft.GetComponent<LineRenderer>(); // 블록 좌측 선
        var predictLineRightRender = _predictionLineRight.GetComponent<LineRenderer>(); // 블록 우측 선

        // left line
        var leftLineUpperPoint = new Vector3(0, 0, 0); // predict line 상단
        leftLineUpperPoint.y = blockCollider.bounds.min.y - 0.05f;
        leftLineUpperPoint.x = blockCollider.bounds.min.x;

        RaycastHit2D hit = Physics2D.Raycast(leftLineUpperPoint, Vector2.down, 10);
        var line2Y = Mathf.Max(hit.point.y, -3);   // -3 지면 높이
        var leftLineLowerPoint = new Vector3(leftLineUpperPoint.x, line2Y, 0); // predict line 하단

        predictLineLeftRender.SetPosition(0, leftLineUpperPoint);
        predictLineLeftRender.SetPosition(1, leftLineLowerPoint);

        // right line
        var rightLineUpperPoint = new Vector3(0, 0, 0); // predict line 상단
        rightLineUpperPoint.y = leftLineUpperPoint.y; // == blockCollider.bounds.min.y - 0.05f
        rightLineUpperPoint.x = blockCollider.bounds.max.x;

        hit = Physics2D.Raycast(rightLineUpperPoint, Vector2.down, 10);
        line2Y = Mathf.Max(hit.point.y, -3);   // -3 지면 높이
        var rightLineLowerPoint = new Vector3(rightLineUpperPoint.x, line2Y, 0); // predict line 하단

        predictLineRightRender.SetPosition(0, rightLineUpperPoint);
        predictLineRightRender.SetPosition(1, rightLineLowerPoint);
    }

    public Vector3 GetBlockSpawnPoint()
    {
        return _blockSpawnPosition;
    }
}
