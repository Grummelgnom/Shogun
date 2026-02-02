using UnityEngine;

public class OptionsOpener : MonoBehaviour
{
    public void OpenOptions()
    {
        // Suche den OptionsMenuController zur Laufzeit
        OptionsMenuController controller = FindFirstObjectByType<OptionsMenuController>();


        if (controller != null)
        {
            controller.ToggleOptions();
        }
        else
        {
            Debug.LogWarning("OptionsMenuController nicht gefunden! Stelle sicher, dass er in der Scene existiert.");
        }
    }
}
