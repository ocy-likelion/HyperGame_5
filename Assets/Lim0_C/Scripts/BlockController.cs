using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlockController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private const int CAMERA_OFFSET = 10;
    
    [SerializeField]private Camera mainCamera;
    [SerializeField]private GameObject blockPrefab;
    [SerializeField]private GameObject predictionLinePrefab;

    private Vector3 _blockSpawnPosition;
    private GameObject _currentBlock;
    private GameObject _previousBlock;
    private Transform _predictionLine;
    private bool _isPointerDown;

    private void Start()
    {
        Init();
    }

    //초기화
    private void Init()
    {
        // 화면 중상단 좌표
        Vector3 screenUpperCenter =
            new Vector3(Screen.width * 0.5f, Screen.height * 0.8f, CAMERA_OFFSET);
        // 화면 좌표 → 월드 좌표 변환
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenUpperCenter);
        worldPos.z = 0; // 2D는 z=0 맞추는 경우가 많음
        _blockSpawnPosition = worldPos;
        SpawnBlock();
        //예측선 초기화
        _predictionLine = Instantiate(predictionLinePrefab).GetComponent<Transform>();
        var predicPos = _blockSpawnPosition;
        predicPos.y -= 4f;
        _predictionLine.position = predicPos;
        _predictionLine.gameObject.SetActive(false);
        

    }
    //돌 생성 기능
    private void SpawnBlock()
    {
        if (_currentBlock != null) return;
        var btnObj = Instantiate(blockPrefab);
        btnObj.GetComponent<Transform>().position = _blockSpawnPosition;
        btnObj.GetComponent<SpriteRenderer>().color = Color.yellow;
        _currentBlock = btnObj;

        if (_isPointerDown)
        {
            _predictionLine.position = new Vector3(
                _blockSpawnPosition.x, _predictionLine.position.y, 0
            );
            _predictionLine.gameObject.SetActive(true);
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
        }
    }
    //터치 입력 끝 이벤트 핸들
    public void OnPointerUp(PointerEventData eventData)
    {
        _isPointerDown = false;
        if (_currentBlock == null) return;
        _currentBlock.GetComponent<Rigidbody2D>().simulated = true;
        _previousBlock = _currentBlock;
        _currentBlock = null;
        _predictionLine.gameObject.SetActive(false);
        StartCoroutine(RespawnBlock());
    }

    IEnumerator RespawnBlock()
    {
        yield return new WaitForSeconds(2f);
        SpawnBlock();
    }
    
    //드래그 입력 이벤트 핸들
    public void OnDrag(PointerEventData eventData)
    {
        if (_currentBlock == null) return;
        SetBlockPosition(eventData.position);
    }

    private void SetBlockPosition(Vector3 eventDataPos)
    {
        Vector3 viewportPos = mainCamera.ScreenToViewportPoint(eventDataPos);
        viewportPos.x = Mathf.Clamp(viewportPos.x, 0.1f, 0.9f);
        viewportPos.z = CAMERA_OFFSET;
        var clampedPos = mainCamera.ViewportToWorldPoint(viewportPos);
        _currentBlock.transform.position =  
            new Vector3(clampedPos.x, _currentBlock.transform.position.y, 0);
        //예측선
        _predictionLine.position = 
            new Vector3(clampedPos.x, _predictionLine.position.y, 0);
    }
}
