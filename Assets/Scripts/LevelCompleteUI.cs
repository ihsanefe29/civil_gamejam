using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Reusable end-of-level score panel. It stays on screen until the player
/// chooses to continue, then contracts into the supplied portal position.
/// </summary>
public class LevelCompleteUI : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float portalPullDuration = 0.9f;

    private Canvas canvas;
    private CanvasGroup overlayGroup;
    private CanvasGroup panelGroup;
    private RectTransform panelRect;
    private Text statusText;
    private Button continueButton;
    private Text continueLabel;
    private Action onContinue;
    private bool exiting;

    public void Show(int totalScore, Action continueAction)
    {
        CreateUiIfNeeded();
        onContinue = continueAction;
        exiting = false;

        canvas.gameObject.SetActive(true);
        overlayGroup.alpha = 0.68f;
        panelGroup.alpha = 1f;
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.localScale = Vector3.one;
        statusText.text = "Ready to proceed to the next level.";
        statusText.color = new Color(0.86f, 0.82f, 0.68f, 1f);
        continueLabel.text = "PROCEED TO NEXT LEVEL";
        continueButton.interactable = true;

        Text scoreText = panelRect.Find("Score")?.GetComponent<Text>();
        if (scoreText != null)
            scoreText.text = $"Total Score: {totalScore}";
    }

    public void ShowUnavailableDestination()
    {
        if (statusText == null || continueButton == null)
            return;

        statusText.text = "The next level is not yet available.";
        statusText.color = new Color(1f, 0.76f, 0.32f, 1f);
        continueButton.interactable = true;
    }

    public void PullIntoPortal(Transform portal, Action onFinished)
    {
        if (exiting || canvas == null || portal == null)
            return;

        exiting = true;
        continueButton.interactable = false;
        StartCoroutine(PullRoutine(portal, onFinished));
    }

    private void HandleContinueButton()
    {
        if (!exiting)
            onContinue?.Invoke();
    }

    private IEnumerator PullRoutine(Transform portal, Action onFinished)
    {
        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector3 screenPoint = Camera.main != null
            ? Camera.main.WorldToScreenPoint(portal.position)
            : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPoint, null, out Vector2 targetPosition);

        Vector2 startPosition = panelRect.anchoredPosition;
        Vector3 startScale = panelRect.localScale;
        for (float elapsed = 0f; elapsed < portalPullDuration; elapsed += Time.unscaledDeltaTime)
        {
            float progress = Mathf.Clamp01(elapsed / portalPullDuration);
            float eased = 1f - Mathf.Pow(1f - progress, 3f);

            panelRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, eased);
            panelRect.localScale = Vector3.Lerp(startScale, Vector3.one * 0.05f, eased);
            panelGroup.alpha = 1f - eased;
            yield return null;
        }

        panelRect.anchoredPosition = targetPosition;
        panelRect.localScale = Vector3.one * 0.05f;
        panelGroup.alpha = 0f;
        onFinished?.Invoke();
    }

    private void CreateUiIfNeeded()
    {
        if (canvas != null)
            return;

        GameObject canvasObject = new GameObject(
            "Level Complete Canvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 2000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        EnsureEventSystem();

        GameObject overlay = CreateImage(canvas.transform, "Overlay", Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero, Color.black);
        overlayGroup = overlay.AddComponent<CanvasGroup>();

        GameObject panel = CreateImage(canvas.transform, "Level Complete Panel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(650f, 405f), Vector2.zero, new Color(0.16f, 0.09f, 0.045f, 0.98f));
        panelRect = panel.GetComponent<RectTransform>();
        panelGroup = panel.AddComponent<CanvasGroup>();

        CreateText(panel.transform, "Title", "LEVEL COMPLETE", 37,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(22f, -94f), new Vector2(-22f, -26f),
            TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.25f));

        CreateText(panel.transform, "Score", "Total Score: 0", 31,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(22f, -22f), new Vector2(-22f, 38f),
            TextAnchor.MiddleCenter, Color.white);

        statusText = CreateText(panel.transform, "Status", string.Empty, 19,
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(25f, 94f), new Vector2(-25f, 134f),
            TextAnchor.MiddleCenter, new Color(0.86f, 0.82f, 0.68f));

        GameObject buttonObject = CreateImage(panel.transform, "Continue Button",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(310f, 66f), new Vector2(0f, 42f), new Color(0.2f, 0.62f, 0.32f, 1f));
        Image buttonImage = buttonObject.GetComponent<Image>();
        continueButton = buttonObject.AddComponent<Button>();
        continueButton.targetGraphic = buttonImage;
        ColorBlock buttonColors = continueButton.colors;
        buttonColors.normalColor = Color.white;
        buttonColors.highlightedColor = new Color(1f, 0.92f, 0.66f, 1f);
        buttonColors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        buttonColors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.65f);
        continueButton.colors = buttonColors;
        continueButton.onClick.AddListener(HandleContinueButton);
        continueLabel = CreateText(buttonObject.transform, "Label", "GO TO NEXT LEVEL", 24,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            TextAnchor.MiddleCenter, Color.white);
    }

    private static GameObject CreateImage(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 size,
        Vector2 position,
        Color color)
    {
        GameObject result = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        result.transform.SetParent(parent, false);
        RectTransform rect = result.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        result.GetComponent<Image>().color = color;
        return result;
    }

    private static Text CreateText(
        Transform parent,
        string objectName,
        string value,
        int fontSize,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax,
        TextAnchor alignment,
        Color color)
    {
        GameObject result = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        result.transform.SetParent(parent, false);
        RectTransform rect = result.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        Text text = result.GetComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = alignment;
        text.color = color;
        return text;
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        new GameObject("Level Complete Event System", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}
