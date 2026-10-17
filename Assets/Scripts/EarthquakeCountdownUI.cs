using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EarthquakeLevelManager))]
public class EarthquakeCountdownUI : MonoBehaviour
{
    private EarthquakeLevelManager levelManager;
    private GameObject countdownRoot;
    private Text countdownText;

    private void Awake()
    {
        levelManager = GetComponent<EarthquakeLevelManager>();
        CreateCountdown();
    }

    private void Update()
    {
        if (levelManager == null || countdownRoot == null)
            return;

        bool isPreparing = levelManager.CurrentPhase == EarthquakePhase.Preparation;
        if (countdownRoot.activeSelf != isPreparing)
            countdownRoot.SetActive(isPreparing);

        if (!isPreparing)
            return;

        int remainingSeconds = Mathf.CeilToInt(levelManager.RemainingPreparationTime);
        countdownText.text = $"DEPREME KALAN SÜRE\n{remainingSeconds:00} sn";
    }

    private void CreateCountdown()
    {
        Canvas canvas = new GameObject("Earthquake Countdown Canvas", typeof(Canvas), typeof(CanvasScaler)).GetComponent<Canvas>();
        canvas.transform.SetParent(transform, false);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        countdownRoot = new GameObject("Earthquake Countdown", typeof(RectTransform), typeof(Image));
        countdownRoot.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = countdownRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -32f);
        panelRect.sizeDelta = new Vector2(420f, 96f);

        Image panel = countdownRoot.GetComponent<Image>();
        panel.color = new Color(0.12f, 0.07f, 0.05f, 0.82f);

        GameObject textObject = new GameObject("Countdown Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(countdownRoot.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        countdownText = textObject.GetComponent<Text>();
        countdownText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        countdownText.fontSize = 32;
        countdownText.fontStyle = FontStyle.Bold;
        countdownText.alignment = TextAnchor.MiddleCenter;
        countdownText.color = Color.white;
    }
}
