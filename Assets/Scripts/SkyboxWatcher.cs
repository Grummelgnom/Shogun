using UnityEngine;

/// Dieses Script NUR in der MainMenu-Szene auf ein leeres GameObject legen.
/// Es beobachtet jeden Frame was mit RenderSettings.skybox passiert.
public class SkyboxWatcher : MonoBehaviour
{
    private Material lastMaterial = null;
    private float lastRotation = -1f;
    private int frameCount = 0;

    void Update()
    {
        frameCount++;
        Material current = RenderSettings.skybox;

        // Loggen wenn sich das Material ändert
        if (current != lastMaterial)
        {
            Debug.Log($"[SkyboxWatcher] Frame {frameCount}: Material GEWECHSELT von '{lastMaterial?.name ?? "NULL"}' zu '{current?.name ?? "NULL"}'");
            lastMaterial = current;
        }

        // Loggen wenn sich _Rotation NICHT ändert (alle 120 Frames)
        if (frameCount % 120 == 0 && current != null)
        {
            float rot = current.GetFloat("_Rotation");
            if (rot == lastRotation)
                Debug.LogWarning($"[SkyboxWatcher] Frame {frameCount}: _Rotation BEWEGT SICH NICHT! Wert: {rot}");
            else
                Debug.Log($"[SkyboxWatcher] Frame {frameCount}: _Rotation läuft: {rot:F3}");
            lastRotation = rot;
        }
    }
}