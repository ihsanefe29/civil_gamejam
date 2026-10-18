using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Brief confirmation shown when all preparation tasks finish before time runs out.
/// </summary>
public class EarlyEarthquakeNoticeUI : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private float hideAt;

    private void Awake()
    {
        CreateUi();
    }

    private void Update()
    {
        bool visible = Time.unscaledTime < hideAt;
        float targetAlpha = visible ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * 8f);
    }

    public void Show(float duration)
    {
        hideAt = Time.unscaledTime + duration;
    }

    private void CreateUi()
    {
        Canvas canvas = new GameObject("Early Earthquake Notice Canvas", typeof(Canvas), typeof(CanvasScaler))
            .GetComponent<Canvas>();
        canvas.transform.SetParent(transform, false);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 160;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panelObject = new GameObject(
            "Preparation Complete Notice",
            typeof(RectTransform),
            typeof(Image),
            typeof(CanvasGroup));
        panelObject.transform.SetParent(canvas.transform, false);

        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(620f, 150f);

        panelObject.GetComponent<Image>().color = new Color(0.11f, 0.07f, 0.035f, 0.9f);
        canvasGroup = panelObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        CreateText(panelObject.transform, "Title", "PREPARATION COMPLETE!", 31,
            new Vector2(0f, 0.25f), new Vector2(1f, 1f), new Color(1f, 0.81f, 0.2f));
        CreateText(panelObject.transform, "Subtitle", "EARTHQUAKE STARTING...", 25,
            new Vector2(0f, 0f), new Vector2(1f, 0.48f), new Color(1f, 0.95f, 0.82f));
    }

    private static void CreateText(
        Transform parent,
        string objectName,
        string value,
        int fontSize,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = anchorMin;
        textRect.anchorMax = anchorMax;
        textRect.offsetMin = new Vector2(18f, 4f);
        textRect.offsetMax = new Vector2(-18f, -4f);

        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
    }
}
