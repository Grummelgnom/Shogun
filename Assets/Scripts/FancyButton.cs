using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class FancyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Animation")]
    public float hoverScale = 1.05f;
    public float pressScale = 0.95f;
    public float rotateDegrees = 5f;
    public float duration = 0.2f;

    private Vector3 originalScale;
    private Vector3 originalEuler;

    void Start()
    {
        originalScale = transform.localScale;
        originalEuler = transform.localEulerAngles;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(hoverScale, duration).SetEase(Ease.OutBack);
        transform.DORotate(new Vector3(0, 0, rotateDegrees), duration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(originalScale, duration).SetEase(Ease.OutBack);
        transform.DORotate(originalEuler, duration);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.DOScale(pressScale, duration * 0.5f).SetEase(Ease.InBack);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOScale(hoverScale, duration).SetEase(Ease.OutBack);
    }
}
