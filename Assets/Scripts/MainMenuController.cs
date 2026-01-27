using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Genau so in Unity geschrieben: "MainScene"
    [SerializeField] private string gameSceneName = "MainScene";

    public void StartTwoPlayer()
    {
        GameSettings.SelectedMode = GameManager.GameMode.TwoPlayer;
        SceneManager.LoadScene(gameSceneName);
    }

    public void StartVsAI()
    {
        GameSettings.SelectedMode = GameManager.GameMode.VsAI;
        SceneManager.LoadScene(gameSceneName);
    }
    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
    Application.Quit();
    #endif
    }
}
