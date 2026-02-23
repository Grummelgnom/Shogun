using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string mainSceneName = "Game";

    [Header("UI References")]
    [SerializeField] private GameUIController gameUIController;

    [Header("Visual Effects")]
    [SerializeField] private GameObject blurOverlay;

    [Header("Skybox")]
    [Tooltip("Das Skybox-Material des MainMenus hier zuweisen!")]
    [SerializeField] private Material mainMenuSkybox;

    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (blurOverlay != null) blurOverlay.SetActive(false);
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
        if (blurOverlay != null) blurOverlay.SetActive(true);
        Time.timeScale = 0f;

        if (gameUIController != null)
        {
            gameUIController.SetHintButtonVisible(false);
            gameUIController.SetPauseButtonVisible(false);
        }
    }

    public void Resume()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (blurOverlay != null) blurOverlay.SetActive(false);
        Time.timeScale = 1f;

        if (gameUIController != null)
        {
            gameUIController.SetHintButtonVisible(true);
            gameUIController.SetPauseButtonVisible(true);
        }
    }

    public void Restart()
    {
        CleanupBeforeLoad();
        SceneManager.LoadScene(mainSceneName);
    }

    public void BackToMenu()
    {
        // Skybox VOR dem Szenenwechsel auf MainMenu-Material setzen
        if (mainMenuSkybox != null)
            RenderSettings.skybox = mainMenuSkybox;

        CleanupBeforeLoad();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void CleanupBeforeLoad()
    {
        Time.timeScale = 1f;
        DOTween.KillAll();
        DOTween.ClearCachedTweens();
    }
}