using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Füge dieses Script zu jedem Button hinzu für Hover-Sound
/// </summary>
public class ButtonSoundTrigger : MonoBehaviour, IPointerEnterHandler
{
    /// <summary>
    /// Wird aufgerufen, wenn Maus über Button schwebt
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayButtonHover();
            Debug.Log($"Hover über {gameObject.name}");
        }
    }
}
