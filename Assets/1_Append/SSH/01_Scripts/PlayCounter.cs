using UnityEngine;

public static class PlayCounter
{
    // 상수
    private const string PLAY_COUNT_KEY = "PlayCount";

    // 메인
    public static int GetPlayCount()
    {
        return PlayerPrefs.GetInt(PLAY_COUNT_KEY, 0); // 기본값 0
    }
    public static void IncrementPlayCount() // 플레이 횟수 증가
    {
        int count = GetPlayCount();
        count++;
        PlayerPrefs.SetInt(PLAY_COUNT_KEY, count);
        PlayerPrefs.Save();
    }
    public static void ResetPlayCount()
    {
        PlayerPrefs.SetInt(PLAY_COUNT_KEY, 0);
        PlayerPrefs.Save();
    }
}
