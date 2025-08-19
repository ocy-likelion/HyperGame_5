using UnityEngine;

public class GimmickDebugUI : MonoBehaviour
{
    public GimmickManager gimmickManager;
    public float autoInterval = 15f;
    private bool autoOn = false;

    void OnGUI()
    {
        if (gimmickManager == null)
        {
            GUI.Label(new Rect(10,10,300,30), "Assign GimmickManager in Inspector");
            return;
        }

        int x = 10, y = 10, w = 200, h = 30, gap = 5;

        if (GUI.Button(new Rect(x, y, w, h), "Base Shake (강하게)"))
        {
            gimmickManager.TriggerBaseShake(1.0f, 1.5f); // 강도↑, 지속시간↑
        }
        y += h + gap;

        if (GUI.Button(new Rect(x, y, w, h), "Top Hit (최상단 타격)"))
        {
            gimmickManager.TriggerTopHit(3, 8f); // 개수=3, 힘↑
        }
        y += h + gap;

        if (GUI.Button(new Rect(x, y, w, h), "Wind (강풍 2초)"))
        {
            gimmickManager.TriggerWind(8f, 2f); // 바람힘↑, 2초
        }
        y += h + gap;

        var label = autoOn ? $"Auto: ON ({autoInterval}s)" : "Auto: OFF";
        if (GUI.Button(new Rect(x, y, w, h), label))
        {
            autoOn = !autoOn;
            if (autoOn) gimmickManager.StartAutoGimmicks(autoInterval);
            else gimmickManager.StopAutoGimmicks();
        }
    }
}
