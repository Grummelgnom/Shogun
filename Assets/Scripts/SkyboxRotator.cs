using UnityEngine;
using System.Collections;

public class SkyboxRotator : MonoBehaviour
{
    public float rotationSpeed = 1f;

    [Tooltip("Das Skybox-Material dieser Szene zuweisen!")]
    public Material skyboxMaterial;

    private float rotation = 0f;

    IEnumerator Start()
    {
        // Warten bis Unity fertig geladen hat
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (skyboxMaterial == null)
        {
            Debug.LogError("[SkyboxRotator] Kein Material zugewiesen!");
            yield break;
        }

        RenderSettings.skybox = skyboxMaterial;
        rotation = 0f;
        Debug.Log($"[SkyboxRotator] Gestartet: {skyboxMaterial.name}");
    }

    void Update()
    {
        if (skyboxMaterial == null) return;
        if (RenderSettings.skybox != skyboxMaterial)
            RenderSettings.skybox = skyboxMaterial;

        rotation = (rotation + rotationSpeed * Time.deltaTime) % 360f;
        skyboxMaterial.SetFloat("_Rotation", rotation);
    }
}