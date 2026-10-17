using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Two successful timing clicks advance one nail through its three art states.
public class HammerNailMinigame : MonoBehaviour
{
    private const int RequiredHits = 2;
    // The hammer's head must line up with the nail on this horizontal guide.
    private const float TargetY = -42f;
    private const float BottomY = -165f;
    private const float TopY = 165f;
    private const float Speed = 235f;
    private const float Tolerance = 23f;
    private const float HammerHomeX = -185f;
    private const float HammerStrikeX = -8f;
    private const float StrikeSpeed = 1500f;
    private const float FinalStatePreviewSeconds = 1.35f;

    private enum HammerState { Swinging, SlidingRight, SlidingLeft, Finished }

    private FridgeMountTask task;
    private Canvas canvas;
    private RectTransform board;
    private RectTransform hammer;
    private Image nail;
    private RectTransform fallbackNail;
    private readonly List<Sprite> nailStates = new();
    private Text status;
    private Image feedback;
    private Button stopButton;
    private int hits;
    private float strikeY;
    private HammerState hammerState;
    private GameObject createdEventSystem;
    private PlayerController controller;
    private PlayerInteraction interaction;
    private bool controllerWasEnabled;
    private bool interactionWasEnabled;

    public void Show(FridgeMountTask mountTask)
    {
        if (task != null || mountTask == null) return;
        CancelInvoke();
        hits = 0;
        hammerState = HammerState.Swinging;
        task = mountTask;
        DisablePlayerControls();
        CreateInterface();
        SetStatus("Çekiç sarı çizgideyken STOP düğmesine bas.");
    }

    private void Update()
    {
        if (task == null || hammer == null) return;
        if (Input.GetKeyDown(KeyCode.Escape)) { Close(); return; }

        switch (hammerState)
        {
            case HammerState.Swinging:
                float y = BottomY + Mathf.PingPong(Time.unscaledTime * Speed, TopY - BottomY);
                hammer.anchoredPosition = new Vector2(HammerHomeX, y);
                break;
            case HammerState.SlidingRight:
                SlideHammer(new Vector2(HammerStrikeX, strikeY), ResolveStrike);
                break;
            case HammerState.SlidingLeft:
                SlideHammer(new Vector2(HammerHomeX, strikeY), ResumeSwinging);
                break;
        }
    }

    private void PressStopButton()
    {
        if (hammerState != HammerState.Swinging) return;

        // Freeze the selected Y position first, then slide the hammer across
        // to the nail. The result is judged when the hammer actually reaches it.
        strikeY = hammer.anchoredPosition.y;
        hammerState = HammerState.SlidingRight;
        stopButton.interactable = false;
        SetStatus("Çekiç çiviye gidiyor...");
    }

    private void SlideHammer(Vector2 destination, UnityEngine.Events.UnityAction onReached)
    {
        hammer.anchoredPosition = Vector2.MoveTowards(
            hammer.anchoredPosition,
            destination,
            StrikeSpeed * Time.unscaledDeltaTime);

        if (hammer.anchoredPosition == destination)
            onReached.Invoke();
    }

    private void ResolveStrike()
    {
        if (Mathf.Abs(strikeY - TargetY) > Tolerance)
        {
            Flash(new Color(0.9f, 0.18f, 0.16f, 0.32f));
            SetStatus("Iskaladın — çekiç sarı çizgiyle hizalanmalı.");
            ReturnHammer();
            return;
        }

        hits++;
        SetNailState(hits);
        Flash(new Color(0.35f, 1f, 0.45f, 0.34f));
        if (hits < RequiredHits)
        {
            SetStatus("İsabet! Çivi biraz daha duvara girdi. Bir kez daha vur.");
            ReturnHammer();
            return;
        }

        SetStatus("Buzdolabı güvenle duvara sabitlendi!");
        task.CompleteMounting();
        hammerState = HammerState.Finished;
        // Keep the final nail artwork visible long enough for the player to
        // see that the second hit completed the task.
        Invoke(nameof(Close), FinalStatePreviewSeconds);
    }

    private void ReturnHammer() => hammerState = HammerState.SlidingLeft;

    private void ResumeSwinging()
    {
        hammerState = HammerState.Swinging;
        stopButton.interactable = true;
        SetStatus("Çekiç sarı çizgideyken STOP düğmesine bas.");
    }

    private void CreateInterface()
    {
        canvas = new GameObject("Fridge Mount Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        EnsureEventSystem();
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        CreateImage(canvas.transform, "Overlay", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, .7f));
        GameObject panel = CreateImage(canvas.transform, "Mount Panel", new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(760f, 640f), Vector2.zero, new Color(.13f, .09f, .055f, .98f));
        CreateText(panel.transform, "Title", "BUZDOLABINI SABİTLE", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(-25f, -82f), new Vector2(25f, -12f), 36, TextAnchor.MiddleCenter, new Color(1f, .85f, .3f));
        board = CreateImage(panel.transform, "Hit Board", new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(620f, 390f), new Vector2(0f, -20f), new Color(.22f, .16f, .1f, 1f)).GetComponent<RectTransform>();
        CreateImage(board, "Wall", new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(180f, 310f), Vector2.zero, new Color(.57f, .5f, .4f, 1f));
        CreateNail();
        CreateHammer();
        feedback = CreateImage(board, "Strike Feedback", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear).GetComponent<Image>();
        status = CreateText(panel.transform, "Status", string.Empty, Vector2.zero, new Vector2(1f, 0f), new Vector2(22f, 20f), new Vector2(-22f, 92f), 24, TextAnchor.MiddleCenter, Color.white);
        CreateStopButton(panel.transform);
        CreateText(panel.transform, "Cancel", "Vazgeçmek için ESC", Vector2.zero, new Vector2(1f, 0f), new Vector2(22f, 2f), new Vector2(-22f, 30f), 16, TextAnchor.MiddleCenter, new Color(.75f, .7f, .62f));
    }

    private void CreateStopButton(Transform parent)
    {
        GameObject buttonObject = CreateImage(parent, "Stop Button", new Vector2(.5f, 0f), new Vector2(.5f, 0f), new Vector2(210f, 54f), new Vector2(0f, 112f), new Color(.75f, .18f, .12f, 1f));
        Image image = buttonObject.GetComponent<Image>();
        stopButton = buttonObject.AddComponent<Button>();
        stopButton.targetGraphic = image;
        ColorBlock colors = stopButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, .85f, .85f, 1f);
        colors.pressedColor = new Color(.72f, .72f, .72f, 1f);
        stopButton.colors = colors;
        stopButton.onClick.AddListener(PressStopButton);
        CreateText(buttonObject.transform, "Label", "STOP", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 28, TextAnchor.MiddleCenter, Color.white);
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;

        createdEventSystem = new GameObject("Fridge Mount Event System", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private void CreateHammer()
    {
        // The hammer stays on the left side of the board and only moves on Y.
        GameObject hammerObject = CreateImage(board, "Hammer", new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(160f, 175f), new Vector2(-185f, TopY), Color.white);
        hammer = hammerObject.GetComponent<RectTransform>();
        // The sprite's striking head is near its top; make this the motion
        // reference so the head, rather than the handle centre, meets the nail.
        hammer.pivot = new Vector2(.5f, .78f);
        // Keep the side-profile hammer upright: its handle points down while
        // the whole tool repeatedly travels up and down.
        hammer.localEulerAngles = Vector3.zero;
        Image image = hammerObject.GetComponent<Image>();
        Sprite sprite = FindFirstSprite("ekic");
        if (sprite != null) { image.sprite = sprite; image.preserveAspect = true; }
        else image.color = new Color(.55f, .6f, .66f, 1f);
    }

    private void CreateNail()
    {
        // Side-view nail on the right: long -> half driven -> fully driven.
        GameObject nailObject = CreateImage(board, "Nail", new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(230f, 80f), new Vector2(145f, TargetY), Color.white);
        nail = nailObject.GetComponent<Image>();
        FindNailStates(nailStates);
        if (nailStates.Count > 0) { nail.sprite = nailStates[0]; nail.preserveAspect = true; return; }

        nail.color = Color.clear;
        fallbackNail = CreateImage(nailObject.transform, "Fallback Nail", new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(170f, 14f), Vector2.zero, new Color(.72f, .76f, .8f, 1f)).GetComponent<RectTransform>();
    }

    private void SetNailState(int state)
    {
        if (nailStates.Count > state)
        {
            nail.sprite = nailStates[state];
            nail.preserveAspect = true;
            return;
        }
        if (fallbackNail == null) return;
        float width = state == 1 ? 110f : 50f;
        fallbackNail.sizeDelta = new Vector2(width, 14f);
        fallbackNail.anchoredPosition = new Vector2(-(170f - width) * .5f, 0f);
    }

    private static GameObject CreateImage(Transform parent, string name, Vector2 min, Vector2 max, Vector2 size, Vector2 position, Color color)
    {
        GameObject result = new(name, typeof(RectTransform), typeof(Image));
        result.transform.SetParent(parent, false);
        RectTransform rect = result.GetComponent<RectTransform>();
        rect.anchorMin = min; rect.anchorMax = max; rect.pivot = new Vector2(.5f, .5f); rect.sizeDelta = size; rect.anchoredPosition = position;
        result.GetComponent<Image>().color = color;
        return result;
    }

    private static Text CreateText(Transform parent, string name, string value, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax, int size, TextAnchor alignment, Color color)
    {
        GameObject result = new(name, typeof(RectTransform), typeof(Text));
        result.transform.SetParent(parent, false);
        RectTransform rect = result.GetComponent<RectTransform>();
        rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
        Text text = result.GetComponent<Text>();
        text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.fontStyle = FontStyle.Bold; text.alignment = alignment; text.color = color;
        return text;
    }

    private static Sprite FindFirstSprite(string namePart)
    {
        foreach (SpriteRenderer renderer in FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (renderer.sprite != null && renderer.gameObject.name.Contains(namePart)) return renderer.sprite;
#if UNITY_EDITOR
        foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/assets-hande" }))
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (!path.Contains(namePart)) continue;
            Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;
        }
#endif
        return null;
    }

    private static void FindNailStates(List<Sprite> states)
    {
        states.Clear();
#if UNITY_EDITOR
        // Do not rely on directory enumeration order. These are the exact
        // three authored states for this minigame.
        AddFirstSpriteFromAssetMatching(states, "yandan true");
        AddFirstSpriteFromAssetMatching(states, "yar");
        AddFirstSpriteFromAssetMatching(states, "somun");
#endif
        if (states.Count > 0) return;

        foreach (SpriteRenderer renderer in FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (renderer.sprite != null && renderer.gameObject.name.Contains("ivi"))
                states.Add(renderer.sprite);
    }

#if UNITY_EDITOR
    private static void AddFirstSpriteFromAssetMatching(List<Sprite> states, string namePart)
    {
        foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/assets-hande" }))
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (!path.Contains(namePart)) continue;

            foreach (Object asset in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite)
                {
                    states.Add(sprite);
                    return;
                }
            }
        }
    }
#endif

    private void Flash(Color color) { feedback.color = color; CancelInvoke(nameof(ClearFeedback)); Invoke(nameof(ClearFeedback), .12f); }
    private void ClearFeedback() { if (feedback != null) feedback.color = Color.clear; }
    private void SetStatus(string text) { if (status != null) status.text = text; }

    private void DisablePlayerControls()
    {
        controller = FindFirstObjectByType<PlayerController>(); interaction = FindFirstObjectByType<PlayerInteraction>();
        if (controller != null) { controllerWasEnabled = controller.enabled; controller.enabled = false; }
        if (interaction != null) { interactionWasEnabled = interaction.enabled; interaction.enabled = false; }
    }

    private void Close()
    {
        if (canvas != null) Destroy(canvas.gameObject);
        if (createdEventSystem != null) Destroy(createdEventSystem);
        if (controller != null) controller.enabled = controllerWasEnabled;
        if (interaction != null) interaction.enabled = interactionWasEnabled;
        task = null; board = null; hammer = null; nail = null; stopButton = null; createdEventSystem = null;
    }
}
