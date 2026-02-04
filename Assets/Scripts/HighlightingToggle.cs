using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class HighlightingToggle : MonoBehaviour
{
    private Toggle toggle;

    void Start()
    {
        toggle = GetComponent<Toggle>();

        // Synchronisiere mit OptionsManager
        if (OptionsManager.Instance != null)
        {
            toggle.isOn = OptionsManager.Instance.highlightingEnabled;
        }

        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnToggleChanged(bool value)
    {
        if (OptionsManager.Instance != null)
        {
            OptionsManager.Instance.SetHighlighting(value);
        }
    }

    void OnDestroy()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
    }
}
