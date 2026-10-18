
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FireSound : MonoBehaviour
{
    private AudioSource fireAudio;

    private void Awake()
    {
        fireAudio = GetComponent<AudioSource>();

        fireAudio.loop = true;
        fireAudio.playOnAwake = false;
        fireAudio.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        if (fireAudio == null)
            fireAudio = GetComponent<AudioSource>();

        if (fireAudio.clip != null && !fireAudio.isPlaying)
            fireAudio.Play();
    }

    private void OnDisable()
    {
        if (fireAudio != null)
            fireAudio.Stop();
    }
}
