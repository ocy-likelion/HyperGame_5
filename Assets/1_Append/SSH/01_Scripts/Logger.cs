using TMPro;
using UnityEngine;

public class Logger : MonoBehaviour
{
    [Header("프리팹")]
    [SerializeField] private GameObject prefab_LogText;

    [SerializeField] private Transform logContent;

    private static Logger instance;
    public static Logger Instance => instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 기존 메서드 그대로
    public void SetLog(string log)
    {
        // null 체크 + 비활성화 체크
        if (instance == null || !gameObject.activeInHierarchy || prefab_LogText == null || logContent == null)
            return;

        GameObject go = Instantiate(prefab_LogText, logContent);
        TMP_Text tmp = go.GetComponent<TMP_Text>();
        if (tmp != null)
            tmp.text = log;

        DontDestroyOnLoad(go);
        Destroy(go, 5f);
    }

    // 기존 JS 호출용
    public void LogFromJS(string message)
    {
        SetLog("[JS] " + message);
    }
}
