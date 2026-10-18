
using UnityEngine;

public class ElectricFish : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Proximity Sound")]
    [SerializeField] private AudioSource electricHumSource;
    [SerializeField, Min(0.1f)] private float hearingDistance = 3f;
    [SerializeField, Min(0f)] private float maxVolumeDistance = 0.5f;
    [SerializeField, Range(0f, 1f)] private float maxVolume = 0.4f;

    [Header("Touch Detection")]
    [SerializeField] private bool playOnTouch = true;

    private bool hasPlayedTouchSound;

    private void Awake()
    {
        if (electricHumSource != null)
        {
            electricHumSource.playOnAwake = false;
            electricHumSource.loop = true;
            electricHumSource.spatialBlend = 0f;
            electricHumSource.volume = 0f;
            electricHumSource.Stop();
        }
    }

    private void Update()
    {
        if (player == null || electricHumSource == null)
            return;

        // Measure distance between player and fish
        float distance = Vector2.Distance(
            player.position,
            transform.position
        );

        // Stop sound completely when out of range
        if (distance >= hearingDistance)
        {
            StopElectricSound();
            return;
        }

        // Calculate volume based on distance
        float volume = Mathf.InverseLerp(
            hearingDistance,
            maxVolumeDistance,
            distance
        ) * maxVolume;

        electricHumSource.volume = Mathf.Clamp01(volume);

        if (electricHumSource.volume > 0.001f)
        {
            if (!electricHumSource.isPlaying)
                electricHumSource.Play();
        }
        else
        {
            StopElectricSound();
        }
    }

    private void StopElectricSound()
    {
        if (electricHumSource == null)
            return;

        electricHumSource.volume = 0f;

        if (electricHumSource.isPlaying)
            electricHumSource.Stop();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!playOnTouch || hasPlayedTouchSound || player == null)
            return;

        if (other.transform != player &&
            !other.transform.IsChildOf(player))
            return;

        hasPlayedTouchSound = true;

        Level3AudioManager.Instance?.PlayElectricShock();
    }

    private void OnDisable()
    {
        StopElectricSound();
    }
}
