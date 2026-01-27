using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text resultText;

    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string mainSceneName = "MainScene";

    private bool shown = false;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (shown || gameManager == null) return;

        int winner = gameManager.GetWinner(); // Getter existiert bei euch
        if (winner != 0)
        {
            shown = true;
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (resultText != null) resultText.text = $"Spieler {winner} gewinnt!";
        }
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
