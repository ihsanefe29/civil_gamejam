using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FloodSceneManager))]
public class FloodTaskUI : MonoBehaviour
{
    [SerializeField] private FloodTaskManager taskManager;

    private FloodSceneManager sceneManager;

    private GameObject canvasRoot;
    private GameObject countdownRoot;
    private GameObject taskRoot;
    private GameObject resultRoot;
    private bool resultsHidden = false;

    private Text countdownText;
    private Text taskText;
    private Text resultText;

    private bool endingTriggered = false;

    [SerializeField] private EndingManager endingManager;

    private void Awake()
    {
        sceneManager = GetComponent<FloodSceneManager>();

        if (taskManager == null)
            taskManager = GetComponent<FloodTaskManager>();

        CreateHud();

        countdownRoot.SetActive(false);
        taskRoot.SetActive(false);
        resultRoot.SetActive(false);
    }

    private void LateUpdate()
    {
        bool completed = sceneManager.CurrentPhase == FloodPhase.Completed;
        bool intro = sceneManager.CurrentPhase == FloodPhase.Intro;

        resultRoot.SetActive(completed && !resultsHidden);
        if (completed && !endingTriggered)
        {
            endingTriggered = true;

            if (endingManager == null)
                endingManager = FindFirstObjectByType<EndingManager>();

            if (endingManager != null)
                endingManager.StartEnding();
            else
                Debug.LogError("EndingManager bulunamadý!");
        }
        taskRoot.SetActive(!intro && !completed);
        countdownRoot.SetActive(!intro && !completed);

        if (taskManager == null)
            return;

        if (completed)
        {
            resultText.text =
                "BAÞARDIN!\n\n" +
                "Düdük balýklarýný kullanarak\n" +
                "dikkat çekmeyi baþardýn\n" +
                "ve rüyandan kurtarýldýn!\n\n" +
                $"Toplanan düdük balýðý: " +
                $"{taskManager.SpecialItemCount}/" +
                $"{taskManager.RequiredSpecialItems}";

            return;
        }

        if (intro)
            return;

        bool preparing = sceneManager.CurrentPhase == FloodPhase.Preparation;

        float remaining = preparing
            ? sceneManager.RemainingPreparationTime
            : sceneManager.RemainingFloodTime;

        int seconds = Mathf.CeilToInt(remaining);

        countdownText.text = preparing
            ? $"SELE KALAN SÜRE\n{seconds:00} sn"
            : $"SELÝN BÝTMESÝNE\n{seconds:00} sn";

        if (preparing)
        {
            taskText.text =
                "SEL HAZIRLIÐI\n\n" +
                TaskLine(
                    taskManager.FishInteractionCount >=
                    taskManager.RequiredFishInteractions,
                    $"Balýklar: {taskManager.FishInteractionCount}" +
                    $"/{taskManager.RequiredFishInteractions}"
                ) + "\n" +
                TaskLine(
                    taskManager.HasLadder || taskManager.LadderDelivered,
                    "Merdiveni al"
                ) + "\n" +
                TaskLine(
                    taskManager.LadderDelivered,
                    "Merdiveni kayýða taþý"
                ) +
                $"\n\nToplam coin: {taskManager.SpecialItemCount}/3" +
                "\n\n" + ObjectiveText();
        }
        else
        {
            taskText.text =
                "SEL PARKURU\n\n" +
                $"Toplam düdük balýðý: {taskManager.SpecialItemCount}/3\n\n" +
                ObjectiveText() +
                "\n\nSuya düþme!";
        }
    }

    private string ObjectiveText()
    {
        switch (taskManager.CurrentStage)
        {
            case FloodObjectiveStage.InteractWithFish:
                return "Þimdi: Balýklara yaklaþ ve E'ye bas.";

            case FloodObjectiveStage.TakeLadder:
                return "Þimdi: Merdivene yaklaþ ve E'ye bas.";

            case FloodObjectiveStage.CarryLadderToBoat:
                return "Þimdi: Kayýða yaklaþ ve E'ye bas.";

            case FloodObjectiveStage.WaitForWater:
                return "Su yükseliyor...";

            case FloodObjectiveStage.CollectSpecialItems:
                return "Varillerden atla.\nE - Düdük Balýðýný Topla";

            case FloodObjectiveStage.WaitForFloodEnd:
                return "Düdük balýðý toplandý!";

            default:
                return "";
        }
    }

 

    private static string TaskLine(bool done, string label)
    {
        return done ? $"[X]  {label}" : $"[ ]  {label}";
    }

    private void CreateHud()
    {
        canvasRoot = new GameObject(
            "Flood UI Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler)
        );

        canvasRoot.transform.SetParent(transform, false);

        Canvas canvas = canvasRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        countdownRoot = CreatePanel(
            "Countdown",
            canvasRoot.transform,
            new Vector2(0.5f, 1f),
            new Vector2(0f, -32f),
            new Vector2(420f, 110f),
            new Color(0.04f, 0.1f, 0.16f, 0.9f)
        );

        countdownText = CreateText(
            countdownRoot.transform, 32,
            TextAnchor.MiddleCenter, new Vector2(10f, 8f)
        );

        taskRoot = CreatePanel(
            "Tasks",
            canvasRoot.transform,
            new Vector2(0f, 1f),
            new Vector2(32f, -32f),
            new Vector2(490f, 400f),
            new Color(0.035f, 0.075f, 0.12f, 0.92f)
        );

        taskText = CreateText(
            taskRoot.transform, 25,
            TextAnchor.UpperLeft, new Vector2(22f, 18f)
        );

        resultRoot = CreatePanel(
            "Results Background",
            canvasRoot.transform,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            new Color(0f, 0f, 0f, 0.85f)
        );

        RectTransform resultRect = resultRoot.GetComponent<RectTransform>();
        resultRect.anchorMin = Vector2.zero;
        resultRect.anchorMax = Vector2.one;
        resultRect.offsetMin = Vector2.zero;
        resultRect.offsetMax = Vector2.zero;

        GameObject card = CreatePanel(
            "Results Card",
            resultRoot.transform,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(760f, 620f),
            new Color(0.035f, 0.09f, 0.15f, 1f)
        );

        resultText = CreateText(
            card.transform, 30,
            TextAnchor.MiddleCenter, new Vector2(30f, 25f)
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
            name, typeof(RectTransform), typeof(Image)
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
        Transform parent,
        int fontSize,
        TextAnchor alignment,
        Vector2 padding)
    {
        GameObject obj = new GameObject(
            "Text", typeof(RectTransform), typeof(Text)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = padding;
        rect.offsetMax = -padding;

        Text text = obj.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        return text;
    }
    public void HideResults()
    {
        resultsHidden = true;
        resultRoot.SetActive(false);
    }
    private void OnDestroy()
    {
        if (canvasRoot != null)
            Destroy(canvasRoot);
    }
}