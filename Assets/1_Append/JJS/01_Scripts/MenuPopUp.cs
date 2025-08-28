using DG.Tweening;
using UnityEngine;

public class MenuPopUp : MonoBehaviour
{
    [Header("Target")]
    public RectTransform target;        // 비워두면 자동 할당
    public CanvasGroup canvasGroup;     // 비워두면 자동 추가

    [Header("Open (Pop)")]
    [Range(0.5f, 1f)] public float startScale = 0.85f;
    public float popStep1 = 0.18f;      // 1단계(1.05까지)
    public float popStep2 = 0.10f;      // 2단계(1.0으로)
    public float fadeIn = 0.15f;

    [Header("Close")]
    public float closeStep1 = 0.08f;    // 1.00 -> 0.92
    public float closeStep2 = 0.12f;    // 0.92 -> 0.75
    public float fadeOut = 0.12f;

    [Header("Options")]
    public bool playOnEnable = false;   // 켜질 때 자동 팝업

    private Sequence _seq;

    // 유니티 콜백
    private void Reset() { target = GetComponent<RectTransform>(); }
    private void Awake()
    {
        if (!target) target = GetComponent<RectTransform>();
        if (!canvasGroup) canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
    private void OnEnable()
    {
        if (playOnEnable) Show();
    }

    // 메인
    public void Show()
    {
        KillSeq();

        gameObject.SetActive(true);
        target.localScale = Vector3.one * startScale;
        canvasGroup.alpha = 0f;

        // timeScale 영향 X : SetUpdate(true)
        _seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

        _seq.Append(canvasGroup.DOFade(1f, fadeIn))
            .Join(target.DOScale(1.05f, popStep1).SetEase(Ease.OutCubic))
            .Append(target.DOScale(1.00f, popStep2).SetEase(Ease.OutBack, 2.2f));
    }
    public void Hide()
    {
        KillSeq();

        _seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        _seq.Append(target.DOScale(0.92f, closeStep1).SetEase(Ease.InCubic))
            .Append(target.DOScale(0.75f, closeStep2).SetEase(Ease.InBack, 1.5f))
            .Join(canvasGroup.DOFade(0f, fadeOut))
            .OnComplete(() =>
            {
                target.localScale = Vector3.one;
                gameObject.SetActive(false);
            });
    }
    public void ShowInstant()
    {
        KillSeq();
        gameObject.SetActive(true);
        target.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
    }
    public void HideInstant()
    {
        KillSeq();
        gameObject.SetActive(false);
        target.localScale = Vector3.one;
        canvasGroup.alpha = 0f;
    }
    private void KillSeq()
    {
        if (_seq != null && _seq.IsActive()) _seq.Kill();

        _seq = null;
    }
}
