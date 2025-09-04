using System.Runtime.InteropServices;
using UnityEngine;

public class Bridge : MonoBehaviour
{
    #region 유니티에서 자바스크립트로
    [DllImport("__Internal")]
    private static extern void ExecuteJavaScriptMethod(string method); // 단순 메서드 실행용(반환값 없음)
    [DllImport("__Internal")]
    private static extern string ExecuteJavaScriptReturn(string method); // 반환값이 있는 메서드 실행용(단 무조건 string으로 반환됨)

    public static void OpenLeaderBoard() // 리더보드 열기
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ExecuteJavaScriptMethod("openLeaderBoard()");
#else
        Debug.Log("리더보드 오픈");
#endif
    }
    public static void SubmitScore(int score) // 토스에 점수 보내기
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ExecuteJavaScriptMethod($"submitScore({score})");
#else
        Debug.Log("점수 제출");
#endif
    }
    public static void LoadAd() // 광고 불러오기
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ExecuteJavaScriptMethod($"loadInterstitialAd()");
#else
        Debug.Log("광고 불러오기");
#endif
    }
    public static void ShowAd() // 광고 보여주기
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ExecuteJavaScriptMethod($"showInterstitialAd()");
#else
        Debug.Log("광고 보여주기");
#endif
    }
    public static AdLoadStatus GetAdStatus() // 현재 광고 상태 불러오기
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