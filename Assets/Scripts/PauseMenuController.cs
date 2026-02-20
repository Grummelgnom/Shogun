using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string mainSceneName = "MainScene";

    [Header("UI References")]
    [SerializeField] private GameUIController gameUIController;

    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;

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

        if (gameUIController != null)
            gameUIController.SetHintButtonVisible(false);
    }

    public void Resume()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;

        if (gameUIController != null)
            gameUIController.SetHintButtonVisible(true);
    }

    public void Restart()
    {
        CleanupBeforeLoad();
        SceneManager.LoadScene(mainSceneName);
    }

    public void BackToMenu()
    {
        CleanupBeforeLoad();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Vor jedem Szenenwechsel: timeScale zurücksetzen und DOTween sauber beenden.
    /// Ohne das frieren DOTween-Animationen in der Zielszene ein, weil
    /// timeScale noch 0 war als die neuen Tweens gestartet wurden.
    /// </summary>
    private void CleanupBeforeLoad()
    {
        Time.timeScale = 1f;
        DOTween.KillAll();       // Alle laufenden Tweens sauber beenden
        DOTween.ClearCachedTweens(); // Tween-Pool leeren für sauberen Start
    }
}