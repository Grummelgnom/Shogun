using UnityEngine;

public class OptionsMenuController : MonoBehaviour
{
    public GameObject optionsPanel;

    [Header("UI References")]
    [SerializeField] private GameUIController gameUIController;
    [SerializeField] private GameObject pausePanel; // Referenz zum PausePanel

    [Header("Visual Effects")]
    [SerializeField] private GameObject blurOverlay;

    void Start()
    {
        // Panel zu Beginn ausblenden
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (blurOverlay != null)
            blurOverlay.SetActive(false);

        // Falls nicht zugewiesen, automatisch finden
        if (gameUIController == null)
            gameUIController = FindFirstObjectByType<GameUIController>();
    }

    public void ToggleOptions()
    {
        if (optionsPanel != null)
        {
            bool willBeActive = !optionsPanel.activeSelf;
            optionsPanel.SetActive(willBeActive);

            if (blurOverlay != null)
                blurOverlay.SetActive(willBeActive);

            // Hint + Pause Button ausblenden wenn Optionen offen, einblenden wenn geschlossen
            if (gameUIController != null)
            {
                gameUIController.SetHintButtonVisible(!willBeActive);
                gameUIController.SetPauseButtonVisible(!willBeActive);
            }
        }
    }

    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (blurOverlay != null)
            blurOverlay.SetActive(false);

        // Nur einblenden wenn PauseMenu NICHT offen ist
        bool pauseIsOpen = pausePanel != null && pausePanel.activeSelf;
        if (gameUIController != null)
        {
            gameUIController.SetHintButtonVisible(!pauseIsOpen);
            gameUIController.SetPauseButtonVisible(!pauseIsOpen);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) // 'O' für Optionen
            ToggleOptions();
    }
}