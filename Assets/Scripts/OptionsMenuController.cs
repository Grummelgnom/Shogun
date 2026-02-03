using UnityEngine;

public class OptionsMenuController : MonoBehaviour
{
    public GameObject optionsPanel;

    [Header("UI References")]
    [SerializeField] private GameUIController gameUIController;

    void Start()
    {
        // Panel zu Beginn ausblenden
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

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

            // Hint Button ausblenden wenn Optionen offen, einblenden wenn geschlossen
            if (gameUIController != null)
                gameUIController.SetHintButtonVisible(!willBeActive);
        }
    }

    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        // Hint Button wieder einblenden beim Schließen
        if (gameUIController != null)
            gameUIController.SetHintButtonVisible(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) // 'O' für Optionen
            ToggleOptions();
    }
}
