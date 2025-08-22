using System;
using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [SerializeField] PlayManager playManager;

    const float followSpeed = 2f;
    bool isCompleteCameraMove = false;

    private void Update()
    {
        if (isCompleteCameraMove)
        {
            float targetY = playManager.CalculateSetCameraHeight();
            Vector3 pos = transform.position;

            pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * followSpeed);

            // x, y가 0 이하로 내려가지 않도록 제한
            pos.x = Mathf.Max(-0.32f, pos.x);
            pos.y = Mathf.Max(-0.32f, pos.y);

            transform.position = pos;
        }
    }
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
        isCompleteCameraMove = false;
        var pos = new Vector3(gameObject.transform.position.x, height, gameObject.transform.position.z);
        gameObject.transform.DOLocalMove(pos, 0.25f).SetEase(Ease.OutQuad).OnComplete(() => isCompleteCameraMove = true);
    }
}
