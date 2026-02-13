using System.Collections;
using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance;

    [Header("UI Audio Source")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioSource musicSource;

    [Header("UI Sound Effects")]
    [SerializeField] private AudioClip buttonHoverClip;
    [SerializeField] private AudioClip buttonClickClip;

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;

    void Awake()
    {
        // Singleton mit DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("UISoundManager: Bleibt über Scenes hinweg bestehen");

            // Background-Musik automatisch starten
            StartBackgroundMusic();
        }
        else
        {
            Destroy(gameObject);  // Duplikate zerstören
            Debug.Log("UISoundManager: Duplikat entfernt");
        }
    }

    /// <summary>
    /// Startet die Background-Musik als Loop
    /// </summary>
    private void StartBackgroundMusic()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.volume = 0.3f;  // Leiser als UI-Sounds
            musicSource.Play();
            Debug.Log("Background-Musik gestartet");
        }
        else
        {
            Debug.LogWarning("musicSource oder backgroundMusic fehlt!");
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

    /// <summary>
    /// Setzt Musik-Lautstärke (0-1)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }

    /// <summary>
    /// Setzt SFX-Lautstärke (0-1)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        if (uiAudioSource != null)
        {
            uiAudioSource.volume = Mathf.Clamp01(volume);
        }
    }

    /// <summary>
    /// Stoppt die Musik
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    /// Startet die Musik neu
    
    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    [Header("Game Sounds")]
    [SerializeField] private AudioClip pieceMoveClip; 
    [SerializeField] private AudioClip pieceCaptureClip;
    [SerializeField] private AudioClip checkWarningClip;

    public void PlayCheckWarning(float volume = 0.7f)
    {
        if (checkWarningClip != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(checkWarningClip, volume);
        }
    }

    /// Spielt Bewegungs-Sound mit dynamischem Pitch (Tonhöhe)

    public void PlayPieceMoveSound(float pitch = 1f, float volume = 0.4f)
    {
        if (pieceMoveClip != null)
        {
            // Erstelle temporäre AudioSource NUR für diesen Sound
            GameObject tempGO = new GameObject("TempMoveSound");
            AudioSource tempSource = tempGO.AddComponent<AudioSource>();

            // Konfiguriere
            tempSource.clip = pieceMoveClip;
            tempSource.volume = volume;
            tempSource.pitch = pitch;
            tempSource.spatialBlend = 0f; // 2D Sound

            // Abspielen
            tempSource.Play();

            // Automatisch aufräumen nach Sound-Ende
            Destroy(tempGO, pieceMoveClip.length / pitch);
        }
    }

    public void PlayCaptureSound(float volume = 0.6f)
    {
        if (pieceCaptureClip != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(pieceCaptureClip, volume);
        }
    }

    

}
