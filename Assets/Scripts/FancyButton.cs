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
    public Vector3 hoverRotation = new Vector3(0, 0, 5);
    public Vector3 pressRotation = new Vector3(0, 0, 0);

    [Header("Color Animation")]
    public bool enableColor = true;
    public Color hoverColor = Color.white;
    public Color pressColor = Color.gray;

    [Header("Position Bobble")]
    public bool enableBobble = false;
    public float bobbleAmount = 5f;
    public AxisConstraint bobbleAxis = AxisConstraint.Y;

    [Header("Sound")]
    public bool playSound = true;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private Vector3 originalScale;
    private Vector3 originalRotation;
    private Color originalColor;
    private Vector3 originalPosition;
    private Image targetImage;
    private AudioSource audioSource;
    private bool isAnimating = false;
    private bool isHovered = false;
    private Coroutine currentCoroutine;
    private string tweenId;
    private bool initialized = false;

    void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        if (initialized) return;

        tweenId = gameObject.GetInstanceID().ToString();

        originalScale = transform.localScale;
        originalRotation = transform.localEulerAngles;
        originalPosition = transform.localPosition;

        targetImage = GetComponent<Image>();
        if (targetImage == null) targetImage = GetComponentInChildren<Image>();
        if (targetImage != null) originalColor = targetImage.color;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        initialized = true;
    }

    void OnEnable()
    {
        Initialize();

        StopAllLocalCoroutines();
        DOTween.Kill(tweenId);

        isAnimating = false;
        isHovered = false;

        transform.localScale = originalScale;
        transform.localEulerAngles = originalRotation;
        transform.localPosition = originalPosition;
        if (targetImage != null) targetImage.color = originalColor;
    }

    void OnDisable()
    {
        StopAllLocalCoroutines();
        DOTween.Kill(tweenId);
        isAnimating = false;
        isHovered = false;
    }

    void OnDestroy()
    {
        DOTween.Kill(tweenId);
    }

    void StopAllLocalCoroutines()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        StopAllLocalCoroutines();
        currentCoroutine = StartCoroutine(HoverSequence());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        StopAllLocalCoroutines();
        currentCoroutine = StartCoroutine(ExitSequence());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StopAllLocalCoroutines();
        currentCoroutine = StartCoroutine(PressSequence());
        if (playSound && clickSound != null && audioSource)
            audioSource.PlayOneShot(clickSound);
    }

    public void OnPointerUp(PointerEventData eventData) { }

    IEnumerator HoverSequence()
    {
        isAnimating = true;
        DOTween.Kill(tweenId);

        // WaitForSecondsRealtime ignoriert timeScale (wichtig bei Pause!)
        yield return new WaitForSecondsRealtime(0.01f);

        Sequence seq = DOTween.Sequence()
            .SetId(tweenId)
            .SetUpdate(true); // Unscaled Time: läuft auch wenn timeScale = 0

        seq.Join(transform.DOScale(hoverScale, duration).SetEase(scaleEase));

        if (enableRotation)
            seq.Join(transform.DORotate(originalRotation + hoverRotation, duration).SetEase(Ease.OutBack));

        if (enableColor && targetImage != null)
            seq.Join(targetImage.DOColor(hoverColor, duration));

        if (enableBobble)
            seq.Join(transform.DOLocalMove(originalPosition + GetBobbleOffset(), duration).SetEase(Ease.OutBounce));

        if (playSound && hoverSound != null && audioSource)
            audioSource.PlayOneShot(hoverSound);

        yield return seq.WaitForCompletion();
        isAnimating = false;
        currentCoroutine = null;
    }

    IEnumerator ExitSequence()
    {
        isAnimating = true;
        DOTween.Kill(tweenId);

        yield return new WaitForSecondsRealtime(0.01f);

        Sequence seq = DOTween.Sequence()
            .SetId(tweenId)
            .SetUpdate(true); // Unscaled Time

        seq.Join(transform.DOScale(originalScale, duration).SetEase(Ease.OutBack));
        seq.Join(transform.DORotate(originalRotation, duration));

        if (enableColor && targetImage != null)
            seq.Join(targetImage.DOColor(originalColor, duration));

        if (enableBobble)
            seq.Join(transform.DOLocalMove(originalPosition, duration));

        yield return seq.WaitForCompletion();
        isAnimating = false;
        currentCoroutine = null;
    }

    IEnumerator PressSequence()
    {
        isAnimating = true;
        DOTween.Kill(tweenId);

        yield return new WaitForSecondsRealtime(0.01f);

        Sequence seq = DOTween.Sequence()
            .SetId(tweenId)
            .SetUpdate(true); // Unscaled Time

        seq.Join(transform.DOScale(pressScale, duration * 0.5f).SetEase(Ease.InBack));

        if (enableRotation)
            seq.Join(transform.DORotate(originalRotation + pressRotation, duration * 0.5f));

        if (enableColor && targetImage != null)
            seq.Join(targetImage.DOColor(pressColor, duration * 0.5f));

        yield return seq.WaitForCompletion();
        isAnimating = false;
        currentCoroutine = null;
    }

    Vector3 GetBobbleOffset()
    {
        Vector3 o = Vector3.zero;
        if (bobbleAxis == AxisConstraint.X) o.x = bobbleAmount * 0.01f;
        else if (bobbleAxis == AxisConstraint.Y) o.y = bobbleAmount * 0.01f;
        else o.z = bobbleAmount * 0.01f;
        return o;
    }
}