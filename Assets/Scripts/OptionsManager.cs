using UnityEngine;
using System.Collections.Generic; // NEU

public class OptionsManager : MonoBehaviour
{
    // Singleton-Pattern für globalen Zugriff
    public static OptionsManager Instance { get; private set; }

    [Header("Movement Settings")]
    [Range(1f, 20f)]
    public float globalMoveSpeed = 8f;

    [Range(0.1f, 2f)]
    public float globalHoverHeight = 0.3f;

    [Header("Gameplay Settings")]
    public bool highlightingEnabled = false; // Default aus

    public void SetHighlighting(bool enabled)
    {
        highlightingEnabled = enabled;
        Debug.Log($"Highlighting: {(enabled ? "An" : "Aus")}");

        // Sofort aktualisieren, wenn eine Figur ausgewählt ist
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            Piece selected = gm.GetSelectedPiece();
            if (selected != null)
            {
                if (enabled)
                {
                    // Highlighting einschalten: Zeige gültige Züge
                    List<ValidMove> moves = gm.CalculateValidMoves(selected);
                    gm.GetBoardRenderer().HighlightValidMoves(moves);
                }
                else
                {
                    // Highlighting ausschalten: Lösche alle Highlights
                    gm.GetBoardRenderer().ClearHighlights();
                }
            }
        }
    }

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
