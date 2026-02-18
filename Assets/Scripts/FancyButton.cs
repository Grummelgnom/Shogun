using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class FancyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Animation")]
    public float hoverScale = 1.05f;
    public float pressScale = 0.95f;
    public float duration = 0.2f;
    public Ease scaleEase = Ease.OutBack;

    [Header("Rotation Animation")]
    public bool enableRotation = true;
    public Vector3 hoverRotation = new Vector3(0, 0, 360);  // Volle 360°
    public Vector3 pressRotation = new Vector3(0, 0, 0);

    [Header("Color Animation")]
    public bool enableColor = true;
    public Color hoverColor = Color.white;
    public Color pressColor = Color.gray;

    [Header("Position Bobble")]
    public bool enableBobble = true;
    public float bobbleAmount = 5f;
    public AxisConstraint bobbleAxis = AxisConstraint.Y;

    [Header("Sound")]
    public bool playSound = true;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Animation Control")]
    public bool waitForComplete = true;
    public bool resetOnExit = true;  // Zwingt Reset bei Exit

    private Vector3 originalScale;
    private Vector3 originalRotation;
    private Color originalColor;
    private Vector3 originalPosition;
    private Image targetImage;
    private AudioSource audioSource;
    private bool isAnimating = false;
    private bool isHovered = false;

    void Start()
    {
        originalScale = transform.localScale;
        originalRotation = transform.localEulerAngles;
        originalPosition = transform.localPosition;

        targetImage = GetComponent<Image>();
        if (targetImage == null) targetImage = GetComponentInChildren<Image>();
        if (targetImage != null) originalColor = targetImage.color;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (!isAnimating) StartCoroutine(HoverSequence());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (!isAnimating || resetOnExit) StartCoroutine(ExitSequence(true));  // Force Reset
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isAnimating) StartCoroutine(PressSequence());
        if (playSound && clickSound != null && audioSource) audioSource.PlayOneShot(clickSound);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Ignoriert während Animation
    }

    IEnumerator HoverSequence()
    {
        isAnimating = true;
        DOTween.Kill(transform);

        // PARALLELE Animationen starten
        Sequence hoverSeq = DOTween.Sequence();

        // Scale
        hoverSeq.Join(transform.DOScale(hoverScale, duration).SetEase(scaleEase));

        // Rotation KOMPLETT 360°
        if (enableRotation)
            hoverSeq.Join(transform.DORotate(originalRotation + hoverRotation, duration).SetEase(Ease.Linear));

        // Color
        if (enableColor && targetImage != null)
            hoverSeq.Join(targetImage.DOColor(hoverColor, duration));

        // Bobble
        if (enableBobble)
        {
            Vector3 bobbleOffset = GetBobbleOffset();
            hoverSeq.Join(transform.DOLocalMove(originalPosition + bobbleOffset, duration).SetEase(Ease.OutBounce));
        }

        if (playSound && hoverSound != null && audioSource)
            audioSource.PlayOneShot(hoverSound);

        yield return hoverSeq.WaitForCompletion();  // WARTET bis ALLES fertig
        isAnimating = false;
    }

    IEnumerator ExitSequence(bool forceReset = false)
    {
        if (!forceReset && isHovered) yield break;  // Noch gehovert? Abbrechen

        isAnimating = true;
        DOTween.Kill(transform);

        Sequence exitSeq = DOTween.Sequence();

        // Zurück zu Original
        exitSeq.Join(transform.DOScale(originalScale, duration).SetEase(Ease.OutBack));
        exitSeq.Join(transform.DORotate(originalRotation, duration));

        if (enableColor && targetImage != null)
            exitSeq.Join(targetImage.DOColor(originalColor, duration));

        if (enableBobble)
            exitSeq.Join(transform.DOLocalMove(originalPosition, duration));

        yield return exitSeq.WaitForCompletion();
        isAnimating = false;
        isHovered = false;
    }

    IEnumerator PressSequence()
    {
        isAnimating = true;
        DOTween.Kill(transform);

        Sequence pressSeq = DOTween.Sequence();
        pressSeq.Join(transform.DOScale(pressScale, duration * 0.5f).SetEase(Ease.InBack));

        if (enableRotation)
            pressSeq.Join(transform.DORotate(originalRotation + pressRotation, duration * 0.5f));

        if (enableColor && targetImage != null)
            pressSeq.Join(targetImage.DOColor(pressColor, duration * 0.5f));

        yield return pressSeq.WaitForCompletion();
        isAnimating = false;
    }

    Vector3 GetBobbleOffset()
    {
        Vector3 offset = Vector3.zero;
        if (bobbleAxis == AxisConstraint.X) offset.x = bobbleAmount * 0.01f;
        else if (bobbleAxis == AxisConstraint.Y) offset.y = bobbleAmount * 0.01f;
        else offset.z = bobbleAmount * 0.01f;
        return offset;
    }
}
