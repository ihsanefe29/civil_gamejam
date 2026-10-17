using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A single, screen-space prompt reused by every nearby interactable.
/// </summary>
public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField, Min(0f)] private float worldOffset = 0.55f;
    [SerializeField, Min(0.01f)] private float fadeSpeed = 9f;

    private CanvasGroup canvasGroup;
    private RectTransform promptRect;
    private Text promptText;
    private Transform target;
    private string activeText;

    private void Awake()
    {
        CreateUi();
    }

    private void LateUpdate()
    {
        bool shouldShow = target != null && !string.IsNullOrWhiteSpace(activeText);
        float desiredAlpha = shouldShow ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, desiredAlpha, fadeSpeed * Time.unscaledDeltaTime);
        canvasGroup.blocksRaycasts = false;

        if (!shouldShow || Camera.main == null)
            return;

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(target.position + Vector3.up * worldOffset);
        if (screenPosition.z <= 0f)
        {
            canvasGroup.alpha = 0f;
            return;
        }

        promptRect.position = screenPosition;
    }

    public void Show(Transform newTarget, string newText)
    {
        if (target == newTarget && activeText == newText)
            return;

        target = newTarget;
        activeText = newText;
        promptText.text = newText;
    }

    public void Hide()
    {
        target = null;
        activeText = null;
    }

    private void CreateUi()
    {
        Canvas canvas = new GameObject("Interaction Prompt Canvas", typeof(Canvas), typeof(CanvasScaler))
            .GetComponent<Canvas>();
        canvas.transform.SetParent(transform, false);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject promptObject = new GameObject(
            "Interaction Prompt",
            typeof(RectTransform),
            typeof(Image),
            typeof(CanvasGroup));
        promptObject.transform.SetParent(canvas.transform, false);

        promptRect = promptObject.GetComponent<RectTransform>();
        promptRect.pivot = new Vector2(0.5f, 0f);
        promptRect.sizeDelta = new Vector2(190f, 44f);

        Image panel = promptObject.GetComponent<Image>();
        panel.color = new Color(0.08f, 0.06f, 0.04f, 0.58f);

        canvasGroup = promptObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        GameObject textObject = new GameObject("Prompt Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(promptObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 3f);
        textRect.offsetMax = new Vector2(-8f, -3f);

        promptText = textObject.GetComponent<Text>();
        promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        promptText.fontSize = 19;
        promptText.fontStyle = FontStyle.Bold;
        promptText.alignment = TextAnchor.MiddleCenter;
        promptText.color = new Color(1f, 0.96f, 0.82f, 0.92f);
    }
}
