using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string mainSceneName = "MainScene";

    [Header("UI References")]
    [SerializeField] private GameUIController gameUIController; // NEU

    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;

        // Falls nicht zugewiesen, automatisch finden
        if (gameUIController == null)
            gameUIController = FindFirstObjectByType<GameUIController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;

        // Hint Button ausblenden
        if (gameUIController != null)
            gameUIController.SetHintButtonVisible(false);
    }

    public void Resume()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;

        // Hint Button wieder einblenden
        if (gameUIController != null)
            gameUIController.SetHintButtonVisible(true);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainSceneName);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
