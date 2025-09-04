using System;
using System.Threading.Tasks;
using UnityEngine;

public static class ShowAd
{
    /// <param name="timeout">광고 로드 최대 대기 시간 (초)</param>
    /// <param name="checkInterval">상태 체크 간격 (초)</param>
    public static async Task LoadAndShowAdAsync(float timeout = 15f, float checkInterval = 0.1f) // 전면 광고를 로드하고 준비될 때까지 대기한 뒤 보여줍니다.
    {
//#if UNITY_WEBGL && !UNITY_EDITOR
//        // 광고 로드 시작
//        Bridge.LoadAd();

//        float elapsed = 0f;

//        // 광고 로드될 때까지 대기
//        while (true)
//        {
//            AdLoadStatus status = Bridge.GetAdStatus();

//            if (status == AdLoadStatus.Loaded)
//            {
//                Debug.Log("광고 준비 완료. 표시합니다.");
//                break;
//            }
//            else if (status == AdLoadStatus.Failed)
//            {
//                Debug.LogWarning("광고 로드 실패");
//                return;
//            }
//            else if (status == AdLoadStatus.Not_Loaded)
//            {
//                Debug.Log("아직 불러오는 중");
//            }

//            await Task.Delay(TimeSpan.FromSeconds(checkInterval));
//            elapsed += checkInterval;

//            if (elapsed >= timeout)
//            {
//                Debug.LogWarning("광고 로드 타임아웃");
//                return;
//            }
//        }

//        // 광고 표시
//        Bridge.ShowAd();
//#else
//        Debug.Log("에디터 또는 WebGL 미지원 환경: 광고 호출 스킵");
//#endif
    }
}
