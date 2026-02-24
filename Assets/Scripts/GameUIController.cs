using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AudioSource applausAudio;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text resultText;

    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string mainSceneName = "MainScene";

    [Header("Game UI Elements")]
    [SerializeField] private GameObject hintButton;
    [SerializeField] private GameObject pauseButton; // NEU: Pause Button Referenz

    private bool shown = false;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Hint Button zu Spielbeginn einblenden
        SetHintButtonVisible(true);

        // Pause Button zu Spielbeginn einblenden
        SetPauseButtonVisible(true);
    }

    void Update()
    {
        if (shown || gameManager == null) return;

        int winner = gameManager.GetWinner();
        if (winner != 0)
        {
            shown = true;
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (resultText != null) resultText.text = $"Player {winner} wins!";

            // Applaus bei PvAI-Sieg (Player=1)
            if (winner == 1 && gameManager.mode == GameMode.VsAI)
            {
                if (applausAudio != null)
                {
                    applausAudio.Play();
                    Debug.Log("Applaus triggered bei Player-Sieg!");
                }
            }

            // Buttons beim GameOver ausblenden
            SetHintButtonVisible(false);
            SetPauseButtonVisible(false);
        }
    }

    /// <summary>
    /// Steuert die Sichtbarkeit des Hint Buttons
    /// </summary>
    public void SetHintButtonVisible(bool visible)
    {
        if (hintButton != null)
            hintButton.SetActive(visible);
    }

    /// <summary>
    /// Steuert die Sichtbarkeit des Pause Buttons
    /// </summary>
    public void SetPauseButtonVisible(bool visible)
    {
        if (pauseButton != null)
            pauseButton.SetActive(visible);
    }

    public void Restart()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}