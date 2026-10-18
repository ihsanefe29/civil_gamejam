
using UnityEngine;

public class Level2AudioManager : MonoBehaviour
{
    public static Level2AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource footstepSource;

    [Header("Volume Settings")]
    [SerializeField, Range(0f, 30f)]
    private float sfxVolumeMultiplier = 15f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip portalSound;
    [SerializeField] private AudioClip objectPickUp;
    [SerializeField] private AudioClip monsterSound;
    [SerializeField] private AudioClip waterFillingSound;
    [SerializeField] private AudioClip fireSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            enabled = false;
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip, sfxVolumeMultiplier);
    }

    // BUTTON CLICK
    public void PlayButtonClick()
    {
        PlaySFX(buttonClick);
    }

    // PORTAL
    public void PlayPortal()
    {
        PlaySFX(portalSound);
    }

    // OBJECT PICK UP
    public void PlayObjectPickUp()
    {
        PlaySFX(objectPickUp);
    }

    // MONSTER
    public void PlayMonster()
    {
        PlaySFX(monsterSound);
    }

    // WATER FILLING
    public void PlayWaterFilling()
    {
        PlaySFX(waterFillingSound);
    }

    // FIRE
    public void PlayFire()
    {
        PlaySFX(fireSound);
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

    // BACKGROUND MUSIC
    public void PlayMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
            musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }
}
