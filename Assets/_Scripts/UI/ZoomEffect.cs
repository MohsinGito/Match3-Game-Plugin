using UnityEngine;
using DG.Tweening;

public class ZoomEffect : MonoBehaviour
{

    public enum ZoomType
    {
        SmoothStep,
        Linear,
        EaseInOut,
    }

    [Tooltip("The RectTransform to apply the zoom effect to.")]
    public RectTransform targetRectTransform;

    [Tooltip("The scale multiplier for the zoom effect.")]
    public float zoomScale = 1.1f;

    [Tooltip("The duration of the zoom in and out effect.")]
    public float zoomDuration = 0.5f;

    [Tooltip("Type of zooming effect.")]
    public ZoomType zoomType = ZoomType.SmoothStep;

    private Vector3 initialScale;
    private Sequence zoomSequence;

    private void Start()
    {
        if (targetRectTransform == null)
        {
            Debug.LogError("ZoomEffect requires a target RectTransform to be assigned.");
            return;
        }

        initialScale = targetRectTransform.localScale;
        StartEffect();
    }
    private void OnEnable() 
    {
        StartEffect();
    }

    private void OnDisable()
    {
        if (zoomSequence != null)
        {
            zoomSequence.Kill(complete: false);
            targetRectTransform.DOKill();
            targetRectTransform.localScale = initialScale;
        }
    }

    private void OnDestroy()
    {
        zoomSequence.Kill(complete: false);
        targetRectTransform.DOKill();
    }

    private void StartEffect()
    {
        zoomSequence = DOTween.Sequence();

        switch (zoomType)
        {
            case ZoomType.SmoothStep:
                zoomSequence.Append(targetRectTransform.DOScale(zoomScale, zoomDuration).SetEase(Ease.InOutSine));
                zoomSequence.Append(targetRectTransform.DOScale(1f, zoomDuration).SetEase(Ease.InOutSine));
                break;
            case ZoomType.Linear:
                zoomSequence.Append(targetRectTransform.DOScale(zoomScale, zoomDuration).SetEase(Ease.Linear));
                zoomSequence.Append(targetRectTransform.DOScale(1f, zoomDuration).SetEase(Ease.Linear));
                break;
            case ZoomType.EaseInOut:
                zoomSequence.Append(targetRectTransform.DOScale(zoomScale, zoomDuration).SetEase(Ease.InOutQuad));
                zoomSequence.Append(targetRectTransform.DOScale(1f, zoomDuration).SetEase(Ease.InOutQuad));
                break;
        }

        zoomSequence.SetLoops(-1, LoopType.Yoyo);
        zoomSequence.Play();
    }

}