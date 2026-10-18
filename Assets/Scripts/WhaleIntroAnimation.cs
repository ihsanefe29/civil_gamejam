
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WhaleIntroAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Image animationImage;
    [SerializeField] private Sprite[] frames;

    [Header("Settings")]
    [SerializeField] private float frameDuration = 0.4f;

    public IEnumerator PlayAnimation()
    {
        gameObject.SetActive(true);

        if (animationImage == null || frames == null || frames.Length == 0)
        {
            Debug.LogWarning("Whale animation is not configured.");
            yield break;
        }

        foreach (Sprite frame in frames)
        {
            animationImage.sprite = frame;
            yield return new WaitForSecondsRealtime(frameDuration);
        }
    }
}
