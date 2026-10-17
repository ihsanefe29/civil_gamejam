using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Portal-only guidance UI. It intentionally does not share the player's
/// generic interaction prompt, so other nearby objects cannot hide it.
/// </summary>
[DisallowMultipleComponent]
public class PortalPromptUI : MonoBehaviour
{
    [SerializeField, Min(0f)] private float worldOffset = 0.7f;
    [SerializeField, Min(0.01f)] private float fadeSpeed = 10f;

    private CanvasGroup canvasGroup;
    private RectTransform promptRect;
    private bool shouldShow;

    private void Awake()
    {
        CreateUi();
    }

    public void SetVisible(bool value)
    {
        shouldShow = value;
    }

    private void LateUpdate()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = Mathf.MoveTowards(
            canvasGroup.alpha,
            shouldShow ? 1f : 0f,
            fadeSpeed * Time.unscaledDeltaTime);

        if (!shouldShow || Camera.main == null)
            return;

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * worldOffset);
        if (screenPosition.z <= 0f)
        {
            canvasGroup.alpha = 0f;
            return;
        }

        promptRect.position = screenPosition;
    }

    private void CreateUi()
    {
        GameObject canvasObject = new GameObject("Portal Prompt Canvas", typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 160;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = new GameObject(
            "Portal Interaction Prompt",
            typeof(RectTransform),
            typeof(Image),
            typeof(CanvasGroup));
        panel.transform.SetParent(canvasObject.transform, false);

        promptRect = panel.GetComponent<RectTransform>();
        promptRect.pivot = new Vector2(0.5f, 0f);
        promptRect.sizeDelta = new Vector2(238f, 46f);

        panel.GetComponent<Image>().color = new Color(0.06f, 0.18f, 0.11f, 0.76f);
        canvasGroup = panel.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        GameObject label = new GameObject("Label", typeof(RectTransform), typeof(Text));
        label.transform.SetParent(panel.transform, false);

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8f, 3f);
        labelRect.offsetMax = new Vector2(-8f, -3f);

        Text text = label.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 19;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.88f, 1f, 0.84f, 1f);
        text.text = "E - Portaldan Geç";
    }
}
