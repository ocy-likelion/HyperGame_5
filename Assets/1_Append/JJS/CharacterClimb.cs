using UnityEngine;

public class CharacterClimb : MonoBehaviour
{
    [Header("타워 표시선(라인)")]
   
    [SerializeField] private float riseSpeed = 1.5f;  // 올라갈 때 속도
    [SerializeField] private float fallSpeed = 10f;   // 내려갈 때 속도
    [SerializeField] private bool instantFall = true; // 무너질 때 즉시 따라갈지 여부

    public PlayManager playManager;
    private float displayedHeight;

    private void Start()
    {
        if (playManager == null)
        {
            Debug.LogError("PlayManager를 찾을 수 없습니다!");
        }

        displayedHeight = 0f;
    }

    private void Update()
    {
        if (playManager == null) return;

        float targetHeight = playManager.currentTowerHeight; // 직접 읽어오기
        UpdateLine(targetHeight);
        Debug.Log("현재 높이 : " + targetHeight + ", 표시된 높이: " + displayedHeight);
    }

    private void UpdateLine(float targetHeight)
    {
        // 올라가는 경우
        if (displayedHeight < targetHeight)
        {
            displayedHeight = Mathf.MoveTowards(displayedHeight, targetHeight, riseSpeed * Time.deltaTime);
        }
        // 내려가는 경우
        else if (displayedHeight > targetHeight)
        {
            if (instantFall)
            {
                displayedHeight = targetHeight;
            }
            else
            {
                displayedHeight = Mathf.MoveTowards(displayedHeight, targetHeight, fallSpeed * Time.deltaTime);
            }
        }

     
    }
}
