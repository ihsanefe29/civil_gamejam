
using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource clickAudioSource;
    [SerializeField] private AudioClip clickSound;

    public void PlayClick()
    {
        clickAudioSource.PlayOneShot(clickSound, 10f);
    }
}
