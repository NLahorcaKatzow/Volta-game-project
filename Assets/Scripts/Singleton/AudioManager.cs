using UnityEngine;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    [Header("Volume Settings")]
    [SerializeField][Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.7f;
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 1f;
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("Default Audio")]
    [SerializeField] private AudioClip defaultBackgroundMusic;
    [SerializeField] private bool playMusicOnStart = true;
    
    [Header("Fade Settings")]
    [SerializeField] private float musicFadeDuration = 1f;
    [SerializeField] private bool debugMode = true;
    
    public static AudioManager Instance;
    
    void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Setup audio sources
            SetupAudioSources();
            
            if (debugMode)
                Debug.Log("AudioManager singleton created and set to DontDestroyOnLoad");
        }
        else if (Instance != this)
        {
            if (debugMode)
                Debug.Log("Duplicate AudioManager found, destroying...");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Apply volume settings
        UpdateVolumes();
        
        // Play default background music if specified
        if (playMusicOnStart && defaultBackgroundMusic != null)
        {
            PlayBackgroundMusic(defaultBackgroundMusic);
        }
    }
    
    private void SetupAudioSources()
    {
        // Create music source if not assigned
        if (musicSource == null)
        {
            GameObject musicObject = new GameObject("MusicSource");
            musicObject.transform.SetParent(transform);
            musicSource = musicObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            
            if (debugMode)
                Debug.Log("Created music AudioSource");
        }
        
        // Create SFX source if not assigned
        if (sfxSource == null)
        {
            GameObject sfxObject = new GameObject("SFXSource");
            sfxObject.transform.SetParent(transform);
            sfxSource = sfxObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            
            if (debugMode)
                Debug.Log("Created SFX AudioSource");
        }
        
        // Configure sources
        musicSource.loop = true;
        sfxSource.loop = false;
    }
    
    private void UpdateVolumes()
    {
        if (musicSource != null)
            musicSource.volume = masterVolume * musicVolume;
        
        if (sfxSource != null)
            sfxSource.volume = masterVolume * sfxVolume;
    }
    
    // Background Music Methods
    public void PlayBackgroundMusic(AudioClip musicClip, bool fadeIn = true)
    {
        if (musicClip == null || musicSource == null) return;
        
        if (musicSource.isPlaying && fadeIn)
        {
            // Fade out current music, then play new music
            FadeOutMusic(() => {
                musicSource.clip = musicClip;
                FadeInMusic();
            });
        }
        else
        {
            musicSource.clip = musicClip;
            if (fadeIn)
                FadeInMusic();
            else
                musicSource.Play();
        }
        
        if (debugMode)
            Debug.Log($"Playing background music: {musicClip.name}");
    }
    
    public void StopBackgroundMusic(bool fadeOut = true)
    {
        if (musicSource == null) return;
        
        if (fadeOut)
        {
            FadeOutMusic(() => musicSource.Stop());
        }
        else
        {
            musicSource.Stop();
        }
        
        if (debugMode)
            Debug.Log("Stopped background music");
    }
    
    public void PauseBackgroundMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
            if (debugMode)
                Debug.Log("Paused background music");
        }
    }
    
    public void ResumeBackgroundMusic()
    {
        if (musicSource != null && !musicSource.isPlaying && musicSource.clip != null)
        {
            musicSource.UnPause();
            if (debugMode)
                Debug.Log("Resumed background music");
        }
    }
    
    // Sound Effects Methods
    public void PlaySoundEffect(AudioClip sfxClip)
    {
        if (sfxClip == null || sfxSource == null) return;
        
        sfxSource.PlayOneShot(sfxClip);
        
        if (debugMode)
            Debug.Log($"Playing sound effect: {sfxClip.name}");
    }
    
    public void PlaySoundEffect(AudioClip sfxClip, float volumeScale)
    {
        if (sfxClip == null || sfxSource == null) return;
        
        sfxSource.PlayOneShot(sfxClip, volumeScale);
        
        if (debugMode)
            Debug.Log($"Playing sound effect: {sfxClip.name} at volume scale: {volumeScale}");
    }
    
    // Fade Methods
    private void FadeInMusic()
    {
        if (musicSource == null) return;
        
        float targetVolume = masterVolume * musicVolume;
        musicSource.volume = 0f;
        musicSource.Play();
        
        musicSource.DOFade(targetVolume, musicFadeDuration).SetEase(Ease.InOutQuad);
        
        if (debugMode)
            Debug.Log("Fading in music");
    }
    
    private void FadeOutMusic(System.Action onComplete = null)
    {
        if (musicSource == null) return;
        
        musicSource.DOFade(0f, musicFadeDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                onComplete?.Invoke();
                if (debugMode)
                    Debug.Log("Music fade out completed");
            });
    }
    
    // Volume Control Methods
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        
        if (debugMode)
            Debug.Log($"Master volume set to: {masterVolume}");
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        
        if (debugMode)
            Debug.Log($"Music volume set to: {musicVolume}");
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        
        if (debugMode)
            Debug.Log($"SFX volume set to: {sfxVolume}");
    }
    
    // Getters
    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
    
    public bool IsMusicPlaying() => musicSource != null && musicSource.isPlaying;
    public bool IsMusicPaused() => musicSource != null && musicSource.clip != null && !musicSource.isPlaying;
    
    public AudioClip GetCurrentMusic() => musicSource?.clip;
    
    // Utility Methods
    public void MuteMusic(bool mute)
    {
        if (musicSource != null)
        {
            musicSource.mute = mute;
            if (debugMode)
                Debug.Log($"Music muted: {mute}");
        }
    }
    
    public void MuteSFX(bool mute)
    {
        if (sfxSource != null)
        {
            sfxSource.mute = mute;
            if (debugMode)
                Debug.Log($"SFX muted: {mute}");
        }
    }
    
    public void MuteAll(bool mute)
    {
        MuteMusic(mute);
        MuteSFX(mute);
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
