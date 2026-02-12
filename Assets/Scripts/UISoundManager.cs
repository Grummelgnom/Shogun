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

    /// <summary>
    /// Startet die Musik neu
    /// </summary>
    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    [Header("Game Sounds")]
    [SerializeField] private AudioClip pieceMoveClip; // NEU

    /// <summary>
    /// Spielt Bewegungs-Sound mit dynamischem Pitch (Tonhöhe)
    /// </summary>
    public void PlayPieceMoveSound(float pitch = 1f, float volume = 0.4f)
    {
        if (pieceMoveClip != null && uiAudioSource != null)
        {
            // Pitch temporär ändern
            float originalPitch = uiAudioSource.pitch;
            uiAudioSource.pitch = pitch;

            // Sound abspielen
            uiAudioSource.PlayOneShot(pieceMoveClip, volume);

            // Pitch zurücksetzen (nach Sound-Länge)
            StartCoroutine(ResetPitchAfterDelay(originalPitch, pieceMoveClip.length));
        }
    }

    private IEnumerator ResetPitchAfterDelay(float originalPitch, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (uiAudioSource != null)
        {
            uiAudioSource.pitch = originalPitch;
        }
    }

}
