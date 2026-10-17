using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FireSceneManager))]
public class FireSceneUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelTaskManager taskManager;

    private FireSceneManager fireSceneManager;

    private GameObject countdownRoot;
    private Text countdownText;
    private Text taskText;

    private void Awake()
    {
        fireSceneManager = GetComponent<FireSceneManager>();

        // Automatically find it if attached to the same object.
        if (taskManager == null)
            taskManager = GetComponent<LevelTaskManager>();

        CreateHud();

        countdownRoot.SetActive(false);
        taskText.text = "";
    }

    private void LateUpdate()
    {
        RefreshCountdown();
        RefreshTasks();
    }

    private void RefreshCountdown()
    {
        bool isPreparing =
            fireSceneManager.CurrentPhase == FirePhase.Preparation;

        if (countdownRoot.activeSelf != isPreparing)
            countdownRoot.SetActive(isPreparing);

        if (!isPreparing)
            return;

        int seconds = Mathf.CeilToInt(
            fireSceneManager.RemainingPreparationTime
        );

        string label = $"YANGINA KALAN SÜRE\n{seconds:00} sn";

        if (countdownText.text != label)
            countdownText.text = label;
    }

    private void RefreshTasks()
    {
        if (taskManager == null)
        {
            SetTaskText("LevelTaskManager referansýný ata.");
            return;
        }

        string label;

        switch (fireSceneManager.CurrentPhase)
        {
            case FirePhase.Preparation:
                {
                    int glassCount = taskManager.RemainingGlassCount;
                    int emberCount = taskManager.RemainingEmberCount;

                    label =
                        "YANGIN HAZIRLIÐI\n\n" +
                        TaskLine(
                            glassCount == 0,
                            $"Camlarý topla (Kalan: {glassCount})"
                        ) + "\n" +
                        TaskLine(
                            taskManager.HasWaterForTask,
                            "Su al"
                        ) + "\n" +
                        TaskLine(
                            emberCount == 0,
                            $"Közleri söndür (Kalan: {emberCount})"
                        );

                    // Show the current step in the same order as the arrows.
                    label += "\n\n" + CurrentObjectiveText();
                    break;
                }

            case FirePhase.Fire:
                {
                    int kills = Mathf.Min(
                        taskManager.EnemyKillCount,
                        taskManager.RequiredEnemyKills
                    );

                    label =
                        "YANGINLA MÜCADELE\n\n" +
                        TaskLine(
                            taskManager.HasWaterForTask,
                            "Su al"
                        ) + "\n" +
                        TaskLine(
                            kills >= taskManager.RequiredEnemyKills,
                            $"Düþmanlarý yen " +
                            $"({kills}/{taskManager.RequiredEnemyKills})"
                        );

                    label += "\n\n" + CurrentObjectiveText();
                    break;
                }

            default:
                // Completed is also used by the current manager on death,
                // so do not assume that this phase means victory.
                label =
                    "BÖLÜM SONA ERDÝ\n\n" +
                    $"Toplam puan: {fireSceneManager.TotalScore}";
                break;
        }

        SetTaskText(label);
    }

    private string CurrentObjectiveText()
    {
        switch (taskManager.CurrentStage)
        {
            case ObjectiveStage.CollectGlass:
                return "Þimdi: Camlarý topla";

            case ObjectiveStage.GetWater:
                return "Þimdi: Su al";

            case ObjectiveStage.ExtinguishEmbers:
                return "Þimdi: Közleri söndür";

            case ObjectiveStage.DefeatEnemies:
                return "Þimdi: Düþmanlarý yen";

            default:
                return "Görevler tamamlandý";
        }
    }

    private void SetTaskText(string label)
    {
        if (taskText.text != label)
            taskText.text = label;
    }

    private static string TaskLine(bool done, string label)
    {
        // ASCII symbols work reliably with the built-in font.
        return done ? $"[X]  {label}" : $"[ ]  {label}";
    }

    private void CreateHud()
    {
        Canvas canvas = new GameObject(
            "Fire UI Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler)
        ).GetComponent<Canvas>();

        canvas.transform.SetParent(transform, false);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Top-center countdown.
        countdownRoot = CreatePanel(
            "Fire Countdown",
            canvas.transform,
            new Vector2(0.5f, 1f),
            new Vector2(0f, -32f),
            new Vector2(420f, 96f),
            new Color(0.12f, 0.07f, 0.05f, 0.82f)
        );

        countdownText = CreateText(
            "Countdown Text",
            countdownRoot.transform,
            32,
            TextAnchor.MiddleCenter,
            Color.white,
            new Vector2(10f, 8f)
        );

        // Top-left task list.
        GameObject taskPanel = CreatePanel(
            "Fire Task List",
            canvas.transform,
            new Vector2(0f, 1f),
            new Vector2(32f, -32f),
            new Vector2(460f, 290f),
            new Color(0.075f, 0.055f, 0.035f, 0.9f)
        );

        taskText = CreateText(
            "Task Text",
            taskPanel.transform,
            25,
            TextAnchor.UpperLeft,
            new Color(1f, 0.95f, 0.78f),
            new Vector2(22f, 15f)
        );
    }

    private static GameObject CreatePanel(
        string name,
        Transform parent,
        Vector2 anchor,
        Vector2 position,
        Vector2 size,
        Color color)
    {
        GameObject panel = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image)
        );

        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = panel.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        return panel;
    }

    private static Text CreateText(
        string name,
        Transform parent,
        int fontSize,
        TextAnchor alignment,
        Color color,
        Vector2 padding)
    {
        GameObject textObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Text)
        );

        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = padding;
        rect.offsetMax = -padding;

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>(
            "LegacyRuntime.ttf"
        );
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        return text;
    }
}