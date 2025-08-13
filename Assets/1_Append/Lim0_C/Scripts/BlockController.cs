using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlockController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private const int CAMERA_OFFSET = 10;
    private const int HEIGHT_OFFSET = 3;
    
    [SerializeField]private Camera mainCamera;
    [SerializeField]private GameObject blockPrefab;
    [SerializeField]private GameObject predictionLinePrefab;

    private Vector3 _blockSpawnPosition;
    private GameObject _currentBlock; 
    private List<GameObject> _blockList = new List<GameObject>();
    private Transform _predictionLine;
    private bool _isPointerDown;
    private float _currentTowerHeight;
    private GameObject _highestBlock;

    public GameObject HighestBlock
    {
        get
        {
            return _highestBlock;
        }
    }

    private void Start()
    {
        Init();
    }

    //초기화
    private void Init()
    {
        UpdateBlockSpawnPosition();
        SpawnBlock();
        //예측선 초기화
        _predictionLine = Instantiate(predictionLinePrefab).GetComponent<Transform>();
        _predictionLine.gameObject.SetActive(false);
    }

    void UpdateBlockSpawnPosition()
    {
        // 화면 중상단 좌표
        Vector3 screenUpperCenter =
            new Vector3(Screen.width * 0.5f, Screen.height * 0.8f, CAMERA_OFFSET);
        // 화면 좌표 → 월드 좌표 변환
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenUpperCenter);
        worldPos.z = 0; // 2D는 z=0 맞추는 경우가 많음
        _blockSpawnPosition = worldPos;
    }

    #region temp
    //카메라 위치 조정
    void CheckCameraHeight()
    {   
        //화면 중하단 좌표
        Vector3 screenLowerCenter =
            new Vector3(Screen.width * 0.5f, Screen.height * 0.2f, CAMERA_OFFSET);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenLowerCenter);

        var height = _highestBlock.transform.position.y + HEIGHT_OFFSET;
        mainCamera.GetComponent<CameraController>().SetCameraHeight(height);

    }
    
    //가장 위에 있는 블록 저장
    void CheckHighestBlock()
    {
        // 타워 높이 갱신
        // 기믹 활용을 위한 최상단 블럭 갱신
        _currentTowerHeight = -10.0f;
        
        foreach (var block in _blockList)
        {
            float height = block.GetComponent<Collider2D>().bounds.max.y;

            if (height > _currentTowerHeight)
            {
                _currentTowerHeight = height;
                _highestBlock = block;
            }
            CheckCameraHeight();
        }
    }
    

    #endregion
    
    
    //돌 생성 기능
    private void SpawnBlock()
    {
        if (_currentBlock != null) return;
        var btnObj = Instantiate(blockPrefab);
        btnObj.GetComponent<Transform>().position = _blockSpawnPosition;
        btnObj.GetComponent<SpriteRenderer>().color = Color.yellow;
        _currentBlock = btnObj;
        _blockList.Add(btnObj);
        
        if (_isPointerDown)
        {
            _predictionLine.position = new Vector3(
                _blockSpawnPosition.x, _predictionLine.position.y, 0
            );
            _predictionLine.gameObject.SetActive(true);
            DrawPredictionLine();
        }
        
    }
    //터치 입력 시작 이벤트 핸들
    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;
        if (_currentBlock != null)
        {
            SetBlockPosition(eventData.position);
            _predictionLine.gameObject.SetActive(true);
            DrawPredictionLine();
        }
    }
    //터치 입력 끝 이벤트 핸들
    public void OnPointerUp(PointerEventData eventData)
    {
        _isPointerDown = false;
        if (_currentBlock == null) return;
        _currentBlock.GetComponent<Rigidbody2D>().simulated = true;
        _currentBlock = null;
        _predictionLine.gameObject.SetActive(false);
        StartCoroutine(RespawnBlock());
    }
    
    //일정 시간 뒤 블록 자동 생성
    IEnumerator RespawnBlock()
    {
        yield return new WaitForSeconds(2f);
        CheckHighestBlock();
        UpdateBlockSpawnPosition();
        SpawnBlock();
    }
    
    //드래그 입력 이벤트 핸들
    public void OnDrag(PointerEventData eventData)
    {
        if (_currentBlock == null) return;
        SetBlockPosition(eventData.position);
    }
    //입력 받은 위치 기반으로 블록위치 바꾸는 기능
    private void SetBlockPosition(Vector3 eventDataPos)
    {
        Vector3 viewportPos = mainCamera.ScreenToViewportPoint(eventDataPos);
        viewportPos.x = Mathf.Clamp(viewportPos.x, 0.1f, 0.9f);
        viewportPos.z = CAMERA_OFFSET;
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
        //-3 지면 높이
        var line2Y = hit.collider ? hit.point.y : -3;
        var linePoint2 = new Vector3(_currentBlock.transform.position.x, line2Y, 0);
        predictLineRender.SetPosition(1, linePoint2);
    }
}
