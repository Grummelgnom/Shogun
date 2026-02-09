using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AudioSource applausAudio;  // NEU: Drag GameManager's Applaus-Source rein
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text resultText;

    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string mainSceneName = "MainScene";

    [Header("Game UI Elements")]
    [SerializeField] private GameObject hintButton; // NEU: Hint Button Referenz

    private bool shown = false;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Hint Button zu Spielbeginn einblenden
        SetHintButtonVisible(true);
    }

    void Update()
    {
        if (shown || gameManager == null) return;

        int winner = gameManager.GetWinner();
        if (winner != 0)
        {
            shown = true;
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (resultText != null) resultText.text = $"Spieler {winner} gewinnt!";
            
            // NEU: Applaus bei PvAI-Sieg (Player=1)
            if (winner == 1 && gameManager.mode == GameMode.VsAI)
            {  // Passe GameMode an dein Enum an
                if (applausAudio != null)
                {
                    applausAudio.Play();
                    Debug.Log("Applaus triggered bei Player-Sieg!");
                }
            }

            SetHintButtonVisible(false);
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

    public void Restart()
    {
        SceneManager.LoadScene(mainSceneName);
        
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
        
    }
}
