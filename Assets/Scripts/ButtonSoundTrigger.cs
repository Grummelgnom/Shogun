using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Füge dieses Script zu jedem Button hinzu für Hover + Click-Sound
/// </summary>
public class ButtonSoundTrigger : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private float lastHoverTime = 0f;
    private const float COOLDOWN = 0.1f; // 100ms Cooldown

    /// <summary>
    /// Hover-Sound
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Nur abspielen, wenn Cooldown vorbei
        if (Time.time - lastHoverTime >= COOLDOWN)
        {
            lastHoverTime = Time.time;

            if (UISoundManager.Instance != null)
            {
                UISoundManager.Instance.PlayButtonHover();
            }
        }
    }

    /// <summary>
    /// Click-Sound - NEU!
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayButtonClick();
        }
    }
}
