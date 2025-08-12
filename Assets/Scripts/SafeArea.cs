using UnityEngine;
public class SafeArea : MonoBehaviour
{
    public Rect safeArea;
    public Vector2 minAnchor;
    public Vector2 maxAnchor;

    private void Awake()
    {
        RectTransform rect = this.GetComponent<RectTransform>();

        safeArea = Screen.safeArea;

        minAnchor = safeArea.min;
        maxAnchor = safeArea.max;

        minAnchor.x /= Screen.width;
        minAnchor.y /= Screen.height;
        maxAnchor.x /= Screen.width;
        maxAnchor.y /= Screen.height;

        rect.anchorMin = minAnchor;
        rect.anchorMax = maxAnchor;
    }

}