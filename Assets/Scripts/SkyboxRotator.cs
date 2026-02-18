using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 0.1f;  // Sanft: 0.05-0.5
    public bool rotateY = true;          // Y-Achse (Standard)
    public bool autoStart = true;

    Material skyMat;

    void Start()
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

    // Manuell stoppen/starten (z.B. per Button)
    public void StartRotation() { autoStart = true; }
    public void StopRotation() { autoStart = false; }
}
