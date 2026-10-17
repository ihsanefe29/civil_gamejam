using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExitPortal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FireSceneManager fireSceneManager;
    [SerializeField] private Rigidbody2D playerBody;

    [Header("Disable On Enter")]
    [Tooltip("PlayerController, ShootEm ve oyuncunun interaction scripti")]
    [SerializeField] private MonoBehaviour[] playerControls;

    [Header("UI")]
    [SerializeField] private Font uiFont;

    [Header("Next Level")]
    [SerializeField] private UnityEvent onNextLevel;

    private bool entered;
    private GameObject resultCanvas;

    public void Enter()
    {
        Debug.Log(
    $"ExitPortal.Enter çaðrýldý | Obje: {name}\n" +
    System.Environment.StackTrace,
    this
);
        if (entered)
            return;

        if (fireSceneManager == null)
            fireSceneManager = FindFirstObjectByType<FireSceneManager>();

        if (fireSceneManager == null ||
            fireSceneManager.CurrentPhase != FirePhase.Completed)
            return;

        entered = true;

        foreach (MonoBehaviour control in playerControls)
        {
            if (control != null)
                control.enabled = false;
        }

        if (playerBody != null)
        {
            MonoBehaviour[] scripts =
                playerBody.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (MonoBehaviour script in scripts)
            {
                if (script != null)
                    script.enabled = false;
            }

            Animator playerAnimator =
    playerBody.GetComponentInChildren<Animator>();

            if (playerAnimator != null)
                playerAnimator.enabled = false;
        }

        CreateResultUI();

        // Hide the portal and its interaction prompt.
        // LevelTaskManager will also remove its arrow.
        gameObject.SetActive(false);
    }

    private void CreateResultUI()
    {
        if (uiFont == null)
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (EventSystem.current == null)
        {
            new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule)
            );
        }

        resultCanvas = new GameObject(
            "Level Result Canvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        Canvas canvas = resultCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;

        CanvasScaler scaler = resultCanvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Full-screen dark background.
        Image overlay = CreateImage(
            "Overlay",
            resultCanvas.transform,
            new Color(0f, 0f, 0f, 0.7f)
        );

        RectTransform overlayRect = overlay.rectTransform;
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image panel = CreateImage(
            "Result Panel",
            overlay.transform,
            new Color(0.08f, 0.06f, 0.04f, 0.98f)
        );

        SetRect(panel.rectTransform, Vector2.zero, new Vector2(620f, 400f));

        CreateText(
            "Title",
            panel.transform,
            "LEVEL COMPLETED",
            38,
            new Vector2(0f, 115f),
            new Vector2(560f, 70f)
        );

        CreateText(
            "Score",
            panel.transform,
            $"Total Score: {fireSceneManager.TotalScore}",
            32,
            new Vector2(0f, 15f),
            new Vector2(560f, 70f)
        );

        Image buttonImage = CreateImage(
            "Next Level Button",
            panel.transform,
            new Color(0.3f, 0.22f, 0.13f, 1f)
        );

        SetRect(
            buttonImage.rectTransform,
            new Vector2(0f, -110f),
            new Vector2(300f, 70f)
        );

        Button button = buttonImage.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(NextLevel);

        CreateText(
            "Button Text",
            buttonImage.transform,
            "Next Level",
            28,
            Vector2.zero,
            new Vector2(280f, 60f)
        );

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void NextLevel()
    {
        // Connect the scene transition later.
        Debug.Log("Next Level clicked!");
        onNextLevel?.Invoke();
    }

    private Image CreateImage(string objectName, Transform parent, Color color)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image)
        );

        obj.transform.SetParent(parent, false);

        Image image = obj.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private void CreateText(
        string objectName,
        Transform parent,
        string content,
        int fontSize,
        Vector2 position,
        Vector2 size)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Text)
        );

        obj.transform.SetParent(parent, false);
        SetRect(obj.GetComponent<RectTransform>(), position, size);

        Text text = obj.GetComponent<Text>();
        text.text = content;
        text.font = uiFont;
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 0.96f, 0.82f);
        text.raycastTarget = false;
    }

    private void SetRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private void OnDestroy()
    {
        if (resultCanvas != null)
            Destroy(resultCanvas);
    }
}