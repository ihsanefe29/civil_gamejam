using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class FloodDeathUI : MonoBehaviour
{
    private GameObject canvasObject;
    private Text reasonText;
    private bool isShowing;

    private void Awake()
    {
        CreateUI();
        canvasObject.SetActive(false);
    }

    public void ShowDeath(string reason)
    {
        if (isShowing)
            return;

        isShowing = true;
        reasonText.text = reason;

        canvasObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    private void CreateUI()
    {
        // Sahnedeki mevcut EventSystem varsa onu kullan.
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObject =
                new GameObject("EventSystem");

            eventSystemObject.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            eventSystemObject.AddComponent<
                UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        canvasObject = new GameObject(
            "Flood Death Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject background = new GameObject(
            "Background",
            typeof(RectTransform),
            typeof(Image)
        );

        background.transform.SetParent(canvasObject.transform, false);

        RectTransform backgroundRect =
            background.GetComponent<RectTransform>();

        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        background.GetComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.85f);

        CreateText(
            background.transform,
            "ÖLDÜN",
            64,
            new Vector2(0f, 140f),
            new Vector2(800f, 100f)
        );

        reasonText = CreateText(
            background.transform,
            "",
            28,
            new Vector2(0f, 30f),
            new Vector2(900f, 100f)
        );

        GameObject buttonObject = new GameObject(
            "Play Again",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );

        buttonObject.transform.SetParent(background.transform, false);

        RectTransform buttonRect =
            buttonObject.GetComponent<RectTransform>();

        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(320f, 80f);
        buttonRect.anchoredPosition = new Vector2(0f, -110f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.25f, 0.16f, 0.09f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(PlayAgain);

        CreateText(
            buttonObject.transform,
            "Play Again",
            32,
            Vector2.zero,
            new Vector2(320f, 80f)
        );
    }

    private static Text CreateText(
        Transform parent,
        string label,
        int fontSize,
        Vector2 position,
        Vector2 size)
    {
        GameObject obj = new GameObject(
            "Text",
            typeof(RectTransform),
            typeof(Text)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = obj.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>(
            "LegacyRuntime.ttf"
        );
        text.text = label;
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 0.96f, 0.82f);
        text.raycastTarget = false;

        return text;
    }

    private void OnDestroy()
    {
        if (canvasObject != null)
            Destroy(canvasObject);
    }
}