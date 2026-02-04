using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    public static OptionsManager Instance { get; private set; }

    [Header("Movement Settings")]
    [Range(1f, 20f)] public float globalMoveSpeed = 8f;
    [Range(0.1f, 2f)] public float globalHoverHeight = 0.3f;

    [Header("Gameplay Settings")]
    public bool highlightingEnabled = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMoveSpeed(float value)
    {
        globalMoveSpeed = value;
    }

    public void SetHoverHeight(float value)
    {
        globalHoverHeight = value;
    }

    public void SetHighlighting(bool enabled)
    {
        highlightingEnabled = enabled;

        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            if (enabled)
            {
                gm.ForceRefreshHighlights();
            }
            else
            {
                gm.GetBoardRenderer()?.ClearHighlights();
            }
        }
    }
}
