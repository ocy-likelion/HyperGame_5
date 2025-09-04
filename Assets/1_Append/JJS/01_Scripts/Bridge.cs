using System.Runtime.InteropServices;
using UnityEngine;

public class Bridge : MonoBehaviour
{
    #region 유니티에서 자바스크립트로
    [DllImport("__Internal")]
    private static extern void ExecuteJavaScriptMethod(string method); // 단순 메서드 실행용(반환값 없음)
    [DllImport("__Internal")]
    private static extern string ExecuteJavaScriptReturn(string method); // 반환값이 있는 메서드 실행용(단 무조건 string으로 반환됨)

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
    public static void LoadAd()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ExecuteJavaScriptMethod($"loadInterstitialAd()");
#else
        Debug.Log("광고 불러오기");
#endif
    }
    public static void ShowAd()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ExecuteJavaScriptMethod($"showInterstitialAd()");
#else
        Debug.Log("광고 보여주기");
#endif
    }
    public static AdLoadStatus GetAdStatus()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string status = ExecuteJavaScriptReturn("GetAdLoadStatus()");
        switch (status)
        {
            AudioListener.pause = true;
            Time.timeScale = 0f;
        }
        else
        {
            AudioListener.pause = false;
            Time.timeScale = 1f;
        }
    }
}

public enum AdLoadStatus
{
    Not_Loaded,
    Loaded,
    Failed,
    Closed
}