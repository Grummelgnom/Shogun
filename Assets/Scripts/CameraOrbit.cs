using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Header("Setup")]
    public Transform target;     // Brett-Center (GameManager draggen)
    public float rotationSpeed = 10f;  // Sanft: 5-20
    public bool autoRotate = true;     // Auto-Dreh ein/aus

    [Header("Advanced")]
    public float distance = 10f;       // Abstand zur Kamera
    public float height = 5f;          // Höhe über Brett

    void Start()
    {
        if (target == null)
            target = GameObject.Find("GameManager").transform; // Auto-Find
    }

    void LateUpdate()
    {
        if (autoRotate && target)
        {
            // Sanfte Rotation um Target
            transform.RotateAround(target.position, Vector3.up, rotationSpeed * Time.deltaTime);

            // Perspektive immer auf Brett zentriert
            Vector3 pos = target.position;
            pos.y += height;
            transform.position = pos + (transform.position - target.position).normalized * distance;
            transform.LookAt(target.position + Vector3.up * height / 2);
        }
    }
}
