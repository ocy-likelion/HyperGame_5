using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Logger : MonoBehaviour
{
    // 프리팹
    [Header("프리팹")]
    [SerializeField] private GameObject prefab_LogText;

    // private 필드(씬 오브젝트)
    [SerializeField] private Transform logContent;

    // 싱긑턴
    private static Logger instance;
    public static Logger Instance => instance;

    // 유니티 콜백
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

    // 메인
    public void SetLog(string log)
    {
        GameObject go = Instantiate(prefab_LogText, logContent);
        go.GetComponent<TMP_Text>().text = log;
        Destroy(go, 5);
    }
}
