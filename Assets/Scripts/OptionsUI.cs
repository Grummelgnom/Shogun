using UnityEngine;
using UnityEngine.UI;
using TMPro; // Falls du TextMeshPro nutzt

public class OptionsUI : MonoBehaviour
{
    [Header("Slider References")]
    public Slider sliderMoveSpeed;
    public Slider sliderHoverHeight;

    [Header("Label References")]
    public TextMeshProUGUI labelSpeed;  // Wenn TextMeshPro
    public TextMeshProUGUI labelHeight; // Wenn TextMeshPro

    // Falls du Legacy Text nutzt, kommentiere die oberen aus und nutze diese:
    // public Text labelSpeed;
    // public Text labelHeight;

    void Start()
    {
        // Initiale Werte setzen
        if (OptionsManager.Instance != null)
        {
            sliderMoveSpeed.value = OptionsManager.Instance.globalMoveSpeed;
            sliderHoverHeight.value = OptionsManager.Instance.globalHoverHeight;
        }

        // Listener für Slider-Änderungen
        sliderMoveSpeed.onValueChanged.AddListener(OnMoveSpeedChanged);
        sliderHoverHeight.onValueChanged.AddListener(OnHoverHeightChanged);

        // Initiale Labels setzen
        UpdateLabels();
    }

    void OnMoveSpeedChanged(float value)
    {
        if (OptionsManager.Instance != null)
        {
            OptionsManager.Instance.SetMoveSpeed(value);
            UpdateLabels();
        }
    }

    void OnHoverHeightChanged(float value)
    {
        if (OptionsManager.Instance != null)
        {
            OptionsManager.Instance.SetHoverHeight(value);
            UpdateLabels();
        }
    }

    void UpdateLabels()
    {
        if (labelSpeed != null)
            labelSpeed.text = $"Geschwindigkeit: {sliderMoveSpeed.value:F1}";

        if (labelHeight != null)
            labelHeight.text = $"Sprung-Höhe: {sliderHoverHeight.value:F2}";
    }
}
