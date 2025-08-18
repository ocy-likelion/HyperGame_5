using System;
using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    private void OnEnable()
    {
        EventBus.Instance.Subscribe<float>("SetCameraHeight", SetCameraHeight);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<float>("SetCameraHeight", SetCameraHeight);
    }

    public void SetCameraHeight(float height)
    {
        var pos = new Vector3(gameObject.transform.position.x, height, gameObject.transform.position.z);
        gameObject.transform.DOLocalMove(pos, 1f);
    }
}
