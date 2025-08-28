using System;
using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    // 상수
    private const float FOLLOW_SPEED = 2f;

    // private 필드(인스펙터 노출)
    [SerializeField] private PlayManager playManager;

    // 유니티 콜백
    private void Update()
    {
        float targetY = playManager.GetCameraHeight();
        Vector3 pos = transform.position;

        pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * FOLLOW_SPEED);

        // x, y가 0 이하로 내려가지 않도록 제한
        pos.x = Mathf.Max(-0.32f, pos.x);
        pos.y = Mathf.Max(-0.32f, pos.y);

        transform.position = pos;
    }
    private void OnEnable()
    {
        EventBus.Instance.Subscribe<float>("SetCameraHeight", SetCameraHeight);
    }
    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<float>("SetCameraHeight", SetCameraHeight);
    }

    // LEGACY
    public void SetCameraHeight(float height)
    {
        //isCompleteCameraMove = false;
        //var pos = new Vector3(gameObject.transform.position.x, height, gameObject.transform.position.z);
        //gameObject.transform.DOLocalMove(pos, 0.25f).SetEase(Ease.OutQuad).OnComplete(() => isCompleteCameraMove = true);
    }
}
