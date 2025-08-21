using System.Runtime.InteropServices;
using UnityEngine;

public class Bridge : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void ExecuteJavaScriptMethod(string method);

    public static void OpenLeaderBoard()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ExecuteJavaScriptMethod("openLeaderBoard()");
#else
        Debug.Log("리더보드 오픈");
#endif
    }

    public static void SubmitScore(int score)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ExecuteJavaScriptMethod($"submitScore({score})");
#else
        Debug.Log("점수 제출");
#endif
    }
}
