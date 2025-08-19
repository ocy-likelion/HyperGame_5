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
    private Transform _predictionLine;
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

    //초기화
    private void Init()
    {
        //예측선 초기화
        _predictionLine = Instantiate(predictionLinePrefab).GetComponent<Transform>();
        _predictionLine.gameObject.SetActive(false);
    }
    //블록 스폰 위치 업데이트 기능
    void UpdateBlockSpawnPosition()
    {
        // 화면 중상단 좌표
        Vector3 screenUpperCenter =
            new Vector3(Screen.width * 0.5f, Screen.height * 0.8f, Consts.CAMERA_OFFSET);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenUpperCenter);
        worldPos.z = 0; // 2D는 z=0 맞추는 경우가 많음
        _blockSpawnPosition = worldPos;
    }
    
    //돌 생성 기능
    private void SpawnBlock(GameObject newBlock)
    {
        if (_currentBlock is not null) return;
        UpdateBlockSpawnPosition();
        _currentBlock = newBlock;
        //_currentBlock.GetComponent<Rigidbody2D>().simulated = false;
        _currentBlock.GetComponent<Transform>().position = _blockSpawnPosition;
            
        if (_isPointerDown)
        {
            _predictionLine.gameObject.SetActive(true);
            DrawPredictionLine();
        }
        
    }
    //터치 입력 시작 이벤트 핸들
    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;
        if (_currentBlock is null) return;
        SetBlockPosition(eventData.position);
        _predictionLine.gameObject.SetActive(true);
        DrawPredictionLine();
    }
    //터치 입력 끝 이벤트 핸들
    public void OnPointerUp(PointerEventData eventData)
    {
        _isPointerDown = false;
        if (_currentBlock is null) return;
        //_currentBlock.GetComponent<Rigidbody2D>().simulated = true;
        _currentBlock.GetComponent<BlockDropProxy>().IsEnd = false;
        _currentBlock = null;
        _predictionLine.gameObject.SetActive(false);
        EventBus.Instance.Publish("RespawnBlock");
        RealSoundManager.Instance.PlayOneShot(Enums.SfxClips.DropBlock);
    }
    
    //드래그 입력 이벤트 핸들
    public void OnDrag(PointerEventData eventData)
    {
        if (_currentBlock is null) return;
        SetBlockPosition(eventData.position);
    }
    
    
    //입력 받은 위치 기반으로 블록위치 바꾸는 기능
    private void SetBlockPosition(Vector3 eventDataPos)
    {
        Vector3 viewportPos = mainCamera.ScreenToViewportPoint(eventDataPos);
        viewportPos.x = Mathf.Clamp(viewportPos.x, 0.1f, 0.9f);
        viewportPos.z = Consts.CAMERA_OFFSET;
        var clampedPos = mainCamera.ViewportToWorldPoint(viewportPos);
        _currentBlock.transform.position =  
            new Vector3(clampedPos.x, _currentBlock.transform.position.y, 0);
        //예측선
        DrawPredictionLine();
    }
    //예측선 그리는 기능
    private void DrawPredictionLine()
    {
        RaycastHit2D hit = Physics2D.Raycast(_currentBlock.transform.position, Vector2.down, 10);
        var predictLineRender = _predictionLine.GetComponent<LineRenderer>();
        predictLineRender.SetPosition(0, _currentBlock.transform.position);
        var line2Y = hit.collider ? hit.point.y : -3;   //-3 지면 높이
        var linePoint2 = new Vector3(_currentBlock.transform.position.x, line2Y, 0);
        predictLineRender.SetPosition(1, linePoint2);
    }
}
