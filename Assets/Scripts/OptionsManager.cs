using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    // Singleton-Pattern für globalen Zugriff
    public static OptionsManager Instance { get; private set; }

    [Header("Movement Settings")]
    [Range(1f, 20f)]
    public float globalMoveSpeed = 8f;

    [Range(0.1f, 2f)]
    public float globalHoverHeight = 0.3f;

    void Awake()
    {
        // Singleton: Nur eine Instanz erlauben
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Bleibt beim Scene-Wechsel erhalten
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Setter für UI-Slider
    public void SetMoveSpeed(float value)
    {
        globalMoveSpeed = value;
        Debug.Log($"Move Speed: {value:F1}");
    }

    public void SetHoverHeight(float value)
    {
        globalHoverHeight = value;
        Debug.Log($"Hover Height: {value:F2}");
    }
}
