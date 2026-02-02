using UnityEngine;

public class OptionsMenuController : MonoBehaviour
{
    public GameObject optionsPanel;

    void Start()
    {
        // Panel zu Beginn ausblenden
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void ToggleOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) // 'O' für Optionen
            ToggleOptions();
    }

}

