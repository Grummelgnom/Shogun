using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References (assign in Inspector)")]
    [SerializeField] private TMP_Dropdown strengthDropdown;
    [SerializeField] private TMP_Dropdown tempoDropdown;

    [Header("Scene Names")]
    public string mainSceneName = "Game";

    [Header("Scene Load Delay")]
    public float sceneLoadDelay = 0.6f; // Etwas länger als clickZoomDuration im FancyButton

    private void Start()
    {
        SetupDropdown(strengthDropdown);
        SetupDropdown(tempoDropdown);

        // Default selections reflect current GameSettings
        if (strengthDropdown != null) strengthDropdown.value = Mathf.Clamp(GameSettings.AIStrength, 1, 4) - 1;
        if (tempoDropdown != null) tempoDropdown.value = Mathf.Clamp(GameSettings.AITempo, 1, 4) - 1;

        ApplyDropdownsToSettings();
    }

    private void SetupDropdown(TMP_Dropdown dd)
    {
        if (dd == null) return;

        dd.ClearOptions();
        dd.AddOptions(new List<string> { "Easy", "Medium", "Hard" });
        dd.onValueChanged.AddListener(_ => ApplyDropdownsToSettings());
    }

    private void ApplyDropdownsToSettings()
    {
        if (strengthDropdown != null)
            GameSettings.AIStrength = GameSettings.Clamp1To4(strengthDropdown.value + 1);

        if (tempoDropdown != null)
            GameSettings.AITempo = GameSettings.Clamp1To4(tempoDropdown.value + 1);
    }

    // --- Buttons (wire these up in the Button OnClick) ---

    public void PlayTwoPlayer()
    {
        ApplyDropdownsToSettings();
        GameSettings.SelectedMode = GameMode.TwoPlayer;
        GameSettings.AIVsAI = false;
        StartCoroutine(LoadWithDelay(mainSceneName));
    }

    public void PlayVsAI()
    {
        ApplyDropdownsToSettings();
        GameSettings.SelectedMode = GameMode.VsAI;
        GameSettings.AIVsAI = false;
        StartCoroutine(LoadWithDelay(mainSceneName));
    }

    public void PlayAIVsAI()
    {
        ApplyDropdownsToSettings();
        GameSettings.SelectedMode = GameMode.VsAI;
        GameSettings.AIVsAI = true;
        StartCoroutine(LoadWithDelay(mainSceneName));
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator LoadWithDelay(string sceneName)
    {
        yield return new WaitForSecondsRealtime(sceneLoadDelay);
        SceneManager.LoadScene(sceneName);
    }
}