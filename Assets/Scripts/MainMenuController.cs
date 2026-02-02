using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using static GameManager;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References (assign in Inspector)")]
    [SerializeField] private TMP_Dropdown strengthDropdown;
    [SerializeField] private TMP_Dropdown tempoDropdown;


    [Header("Scene Names")]
    public string mainSceneName = "Game";

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
        dd.AddOptions(new List<string> { "1", "2", "3" }); // Beispiel
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
        SceneManager.LoadScene(mainSceneName);
    }

    public void PlayVsAI()
    {
        ApplyDropdownsToSettings();
        GameSettings.SelectedMode = GameMode.VsAI;
        GameSettings.AIVsAI = false;
        SceneManager.LoadScene(mainSceneName);
    }

    public void PlayAIVsAI()
    {
        ApplyDropdownsToSettings();
        GameSettings.SelectedMode = GameMode.VsAI; // Option A: reuse VsAI
        GameSettings.AIVsAI = true;
        SceneManager.LoadScene(mainSceneName);
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
