using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EarthquakeTaskManager))]
public class EarthquakeTaskListUI : MonoBehaviour
{
    private EarthquakeTaskManager taskManager;
    private Text taskText;

    private void Start()
    {
        taskManager = GetComponent<EarthquakeTaskManager>();
        CreateHud();
        taskManager.StateChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (taskManager != null)
            taskManager.StateChanged -= Refresh;
    }

    private void CreateHud()
    {
        Canvas canvas = new GameObject("Earthquake Task Canvas", typeof(Canvas), typeof(CanvasScaler)).GetComponent<Canvas>();
        canvas.transform.SetParent(transform, false);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = new GameObject("Task List", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(32f, -32f);
        panelRect.sizeDelta = new Vector2(430f, 202f);
        panel.GetComponent<Image>().color = new Color(0.075f, 0.055f, 0.035f, 0.9f);

        GameObject textObject = new GameObject("Task Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(panel.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(22f, 15f);
        textRect.offsetMax = new Vector2(-18f, -15f);

        taskText = textObject.GetComponent<Text>();
        taskText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        taskText.fontSize = 25;
        taskText.fontStyle = FontStyle.Bold;
        taskText.alignment = TextAnchor.UpperLeft;
        taskText.horizontalOverflow = HorizontalWrapMode.Overflow;
        taskText.verticalOverflow = VerticalWrapMode.Overflow;
        taskText.color = new Color(1f, 0.95f, 0.78f);
    }

    private void Refresh()
    {
        if (taskText == null || taskManager == null) return;

        taskText.text = "EARTHQUAKE PREPAREDNESS\n\n" +
                        TaskLine(taskManager.InstalledLockCount >= EarthquakeTaskManager.RequiredLocks,
                            $"Lock cabinets ({taskManager.InstalledLockCount}/{EarthquakeTaskManager.RequiredLocks})") + "\n" +
                        TaskLine(taskManager.BagItemCount >= EarthquakeTaskManager.RequiredBagItems,
                            $"Pack emergency bag ({taskManager.BagItemCount}/{EarthquakeTaskManager.RequiredBagItems})") + "\n" +
                        TaskLine(taskManager.IsFridgeSecured, "Secure refrigerator");
    }

    private static string TaskLine(bool done, string label) => done ? $"✓  {label}" : $"☐  {label}";
}
