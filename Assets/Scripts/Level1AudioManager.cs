
using UnityEngine;

public class Level1AudioManager : MonoBehaviour
{
    public static Level1AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource footstepSource;

    [Header("Volume Settings")]
    [SerializeField, Range(0f, 30f)]
    private float sfxVolumeMultiplier = 15f;

    [SerializeField, Range(0f, 30f)]
    private float footstepVolumeMultiplier = 15f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip hammerHit;
    [SerializeField] private AudioClip hammerSuccess;
    [SerializeField] private AudioClip hammerFail;
    [SerializeField] private AudioClip lockSound;
    [SerializeField] private AudioClip earthquakeSound;
    [SerializeField] private AudioClip portalSound;
    [SerializeField] private AudioClip objectPickUp;
    [SerializeField] private AudioClip objectPutDown;

    private float originalFootstepVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate Level1AudioManager found.");
            enabled = false;
            return;
        }

        Instance = this;

        if (footstepSource != null)
        {
            originalFootstepVolume = footstepSource.volume;
        }
    }

    private void Start()
    {
        if (footstepSource != null)
        {
            footstepSource.volume = Mathf.Clamp01(
                originalFootstepVolume * footstepVolumeMultiplier
            );
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, sfxVolumeMultiplier);
    }

    // BUTTONS
    public void PlayButtonClick()
    {
        PlaySFX(buttonClick);
    }

    // HAMMER MINIGAME
    public void PlayHammerHit()
    {
        PlaySFX(hammerHit);
    }

    public void PlayHammerSuccess()
    {
        PlaySFX(hammerSuccess);
    }

    public void PlayHammerFail()
    {
        PlaySFX(hammerFail);
    }

    // CABINET LOCK
    public void PlayLock()
    {
        PlaySFX(lockSound);
    }

    // EARTHQUAKE
    public void PlayEarthquake()
    {
        PlaySFX(earthquakeSound);
    }

    // PORTAL
    public void PlayPortal()
    {
        PlaySFX(portalSound);
    }

    // OBJECT INTERACTIONS
    public void PlayObjectPickUp()
    {
        PlaySFX(objectPickUp);
    }

    public void PlayObjectPutDown()
    {
        PlaySFX(objectPutDown);
    }

    // WALKING
    public void SetWalking(bool walking)
    {
        if (footstepSource == null)
            return;

        if (walking)
        {
            if (!footstepSource.isPlaying)
                footstepSource.Play();
        }
        else
        {
            if (footstepSource.isPlaying)
                footstepSource.Stop();
        }
    }

    // MUSIC
    public void PlayMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
            musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Stop();
    }
}
