using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    public void SetCameraHeight(float height)
    {
        var pos = new Vector3(gameObject.transform.position.x, height, gameObject.transform.position.z);
        gameObject.transform.DOLocalMove(pos, 1f);
    }
}
