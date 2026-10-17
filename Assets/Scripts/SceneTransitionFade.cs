using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Keeps a black overlay alive across a scene load so level changes never
/// flash the next scene before the fade-in begins.
/// </summary>
public class SceneTransitionFade : MonoBehaviour
{
    private const float FadeOutDuration = 0.9f;
    private const float FadeInDuration = 0.85f;
    private static bool transitionInProgress;

    private CanvasGroup fadeGroup;

    public static void FadeOutAndLoad(string sceneName)
    {
        if (transitionInProgress || string.IsNullOrWhiteSpace(sceneName))
            return;

        transitionInProgress = true;

        GameObject transitionObject = new GameObject("Scene Transition Fade");
        DontDestroyOnLoad(transitionObject);

        SceneTransitionFade transition = transitionObject.AddComponent<SceneTransitionFade>();
        transition.StartCoroutine(transition.TransitionRoutine(sceneName));
    }

    private void Awake()
    {
        GameObject canvasObject = new GameObject(
            "Fade Canvas",
            typeof(Canvas),
            typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject blackout = new GameObject("Blackout", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        blackout.transform.SetParent(canvasObject.transform, false);

        RectTransform rect = blackout.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        blackout.GetComponent<Image>().color = Color.black;
        fadeGroup = blackout.GetComponent<CanvasGroup>();
        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = true;
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        yield return Fade(0f, 1f, FadeOutDuration);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        while (!loadOperation.isDone)
            yield return null;

        // Let the first rendered frame of the new level exist behind black.
        yield return null;
        yield return Fade(1f, 0f, FadeInDuration);

        transitionInProgress = false;
        Destroy(gameObject);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            fadeGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        fadeGroup.alpha = to;
    }
}
