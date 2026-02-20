using UnityEngine;
using UnityEngine.SceneManagement;

public class SkyboxRotator : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 0.1f;
    public bool rotateY = true;
    public bool autoStart = true;

    Material skyMat;

    void OnEnable()
    {
        // Material bei jedem OnEnable neu holen (auch nach Szenenwechsel)
        FetchSkyboxMaterial();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Nach jedem Szenenwechsel Referenz erneuern
        FetchSkyboxMaterial();
    }

    void FetchSkyboxMaterial()
    {
        skyMat = RenderSettings.skybox;
        if (skyMat == null)
            Debug.LogError("Kein Skybox Material! Lighting > Environment zuweisen.");
    }

    void Update()
    {
        if (skyMat && autoStart)
        {
            float rot = Time.time * rotationSpeed;
            skyMat.SetFloat("_Rotation", rot);
        }
    }

    public void StartRotation() { autoStart = true; }
    public void StopRotation() { autoStart = false; }
}