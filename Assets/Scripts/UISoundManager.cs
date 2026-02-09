using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance;

    [Header("UI Audio Source")]
    [SerializeField] private AudioSource uiAudioSource;

    [Header("UI Sound Effects")]
    [SerializeField] private AudioClip buttonHoverClip;
    [SerializeField] private AudioClip buttonClickClip;

    void Awake()
    {
        // Singleton mit DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // ← DIESE ZEILE NEU!
            Debug.Log("UISoundManager: Bleibt über Scenes hinweg bestehen");
        }
        else
        {
            Destroy(gameObject);  // Duplikate zerstören
            Debug.Log("UISoundManager: Duplikat entfernt");
        }
    }

    public void PlayButtonHover()
    {
        if (buttonHoverClip != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(buttonHoverClip, 0.3f);
        }
    }

    public void PlayButtonClick()
    {
        if (buttonClickClip != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(buttonClickClip, 0.5f);
        }
    }
}
