using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Two successful timing clicks advance one nail through its three art states.
public class HammerNailMinigame : MonoBehaviour
{
    private const int RequiredHits = 2;
    private const float TargetY = -42f;
    private const float BottomY = -165f;
    private const float TopY = 165f;
    private const float Speed = 235f;
    private const float Tolerance = 23f;

    private FridgeMountTask task;
    private Canvas canvas;
    private RectTransform board;
    private RectTransform hammer;
    private Image nail;
    private RectTransform fallbackNail;
    private readonly List<Sprite> nailStates = new();
    private Text status;
    private Image feedback;
    private int hits;
    private PlayerController controller;
    private PlayerInteraction interaction;
    private bool controllerWasEnabled;
    private bool interactionWasEnabled;

    public void Show(FridgeMountTask mountTask)
    {
        if (task != null || mountTask == null) return;
        task = mountTask;
        DisablePlayerControls();
        CreateInterface();
        SetStatus("Çekiç sarı çizgiden geçerken SOL TIKLA.");
    }

    private void Update()
    {
        if (task == null || hammer == null) return;
        if (Input.GetKeyDown(KeyCode.Escape)) { Close(); return; }

        float y = BottomY + Mathf.PingPong(Time.unscaledTime * Speed, TopY - BottomY);
        hammer.anchoredPosition = new Vector2(0f, y);
        if (Input.GetMouseButtonDown(0)) TryStrike();
    }

    private void TryStrike()
    {
        if (Mathf.Abs(hammer.anchoredPosition.y - TargetY) > Tolerance)
        {
            Flash(new Color(0.9f, 0.18f, 0.16f, 0.32f));
            SetStatus("Iskaladın — çekiç sarı çizgiyle hizalanmalı.");
            return;
        }

        hits++;
        SetNailState(hits);
        Flash(new Color(0.35f, 1f, 0.45f, 0.34f));
        if (hits < RequiredHits)
        {
            SetStatus("İsabet! Çivi biraz daha duvara girdi. Bir kez daha vur.");
            return;
        }

        SetStatus("Buzdolabı güvenle duvara sabitlendi!");
        task.CompleteMounting();
        Invoke(nameof(Close), 0.7f);
    }

    private void CreateInterface()
    {
        canvas = new GameObject("Fridge Mount Canvas", typeof(Canvas), typeof(CanvasScaler)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        CreateImage(canvas.transform, "Overlay", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, .7f));
        GameObject panel = CreateImage(canvas.transform, "Mount Panel", new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(760f, 640f), Vector2.zero, new Color(.13f, .09f, .055f, .98f));
        CreateText(panel.transform, "Title", "BUZDOLABINI SABİTLE", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(-25f, -82f), new Vector2(25f, -12f), 36, TextAnchor.MiddleCenter, new Color(1f, .85f, .3f));
        board = CreateImage(panel.transform, "Hit Board", new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(620f, 390f), new Vector2(0f, -20f), new Color(.22f, .16f, .1f, 1f)).GetComponent<RectTransform>();
        CreateImage(board, "Wall", new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(180f, 310f), Vector2.zero, new Color(.57f, .5f, .4f, 1f));
        CreateImage(board, "Hit Line", new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(360f, 5f), new Vector2(0f, TargetY), new Color(1f, .82f, .2f, .75f));
        CreateNail();
        CreateHammer();
        feedback = CreateImage(board, "Strike Feedback", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear).GetComponent<Image>();
        status = CreateText(panel.transform, "Status", string.Empty, Vector2.zero, new Vector2(1f, 0f), new Vector2(22f, 20f), new Vector2(-22f, 92f), 24, TextAnchor.MiddleCenter, Color.white);
        CreateText(panel.transform, "Cancel", "Vazgeçmek için ESC", Vector2.zero, new Vector2(1f, 0f), new Vector2(22f, 2f), new Vector2(-22f, 30f), 16, TextAnchor.MiddleCenter, new Color(.75f, .7f, .62f));
    }

    private void CreateHammer()
    {
        GameObject hammerObject = CreateImage(board, "Hammer", new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(160f, 175f), new Vector2(0f, TopY), Color.white);
        hammer = hammerObject.GetComponent<RectTransform>();
        Image image = hammerObject.GetComponent<Image>();
        Sprite sprite = FindFirstSprite("ekic");
        if (sprite != null) { image.sprite = sprite; image.preserveAspect = true; }
        else image.color = new Color(.55f, .6f, .66f, 1f);
    }

    private void CreateNail()
    {
        GameObject nailObject = CreateImage(board, "Nail", new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(90f, 180f), Vector2.zero, Color.white);
        nail = nailObject.GetComponent<Image>();
        FindNailStates(nailStates);
        if (nailStates.Count > 0) { nail.sprite = nailStates[0]; nail.preserveAspect = true; return; }

        nail.color = Color.clear;
        fallbackNail = CreateImage(nailObject.transform, "Fallback Nail", new Vector2(.5f, 0f), new Vector2(.5f, 0f), new Vector2(14f, 135f), new Vector2(0f, 60f), new Color(.72f, .76f, .8f, 1f)).GetComponent<RectTransform>();
    }

    private void SetNailState(int state)
    {
        if (nailStates.Count > state) { nail.sprite = nailStates[state]; return; }
        if (fallbackNail == null) return;
        float height = state == 1 ? 88f : 42f;
        fallbackNail.sizeDelta = new Vector2(14f, height);
        fallbackNail.anchoredPosition = new Vector2(0f, height * .5f);
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
        foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/assets-hande" }))
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (!path.Contains("ivi")) continue;
            foreach (Object asset in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path)) if (asset is Sprite sprite) states.Add(sprite);
            if (states.Count > 0) return;
        }
#endif
        foreach (SpriteRenderer renderer in FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (renderer.sprite != null && renderer.gameObject.name.Contains("ivi")) states.Add(renderer.sprite);
    }

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
        if (controller != null) controller.enabled = controllerWasEnabled;
        if (interaction != null) interaction.enabled = interactionWasEnabled;
        task = null; board = null; hammer = null; nail = null;
    }
}
