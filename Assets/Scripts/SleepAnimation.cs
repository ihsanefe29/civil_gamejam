
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SleepAnimation : MonoBehaviour
{
    [Header("Sleep Animation")]
    [SerializeField] private Image sleepImage;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 1.5f;

    [Header("Disaster Photos")]
    [SerializeField] private GameObject disasterPhotos;

    [Header("Scene Transition")]
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private string nextSceneName = "SampleScene";

    private bool isPlaying = false;

    public void PlayAnimation()
    {
        if (isPlaying) return;

        if (sleepImage == null || frames.Length == 0 ||
            disasterPhotos == null || fadeOverlay == null)
        {
            Debug.LogError("SleepAnimation: Missing references!");
            return;
        }

        isPlaying = true;

        sleepImage.gameObject.SetActive(true);
        disasterPhotos.SetActive(true);

        fadeOverlay.gameObject.SetActive(false);

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float frameDuration = 1f / framesPerSecond;

        foreach (Sprite frame in frames)
        {
            sleepImage.sprite = frame;
            yield return new WaitForSeconds(frameDuration);
        }

        fadeOverlay.gameObject.SetActive(true);

        Color color = Color.black;
        color.a = 0f;
        fadeOverlay.color = color;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadeOverlay.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeOverlay.color = color;

        SceneManager.LoadScene(nextSceneName);
    }
}
