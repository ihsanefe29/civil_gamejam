using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EarthquakeLevelManager))]
public class InventoryHUD : MonoBehaviour
{
    private readonly string[] itemIds = { "first_aid_kit", "battery", "water_bottle", "torch" };
    private readonly string[] itemNames = { "FIRST AID KIT", "BATTERY", "WATER BOTTLE", "TORCH" };
    private readonly string[] sceneObjectNames = { "First aid kit", "Battery", "Water bottle", "Torch" };

    private readonly Dictionary<string, Image> iconImages = new();
    private Inventory inventory;

    private void Start()
    {
        inventory = FindFirstObjectByType<Inventory>();
        if (inventory == null)
            return;

        CreateHud();
        inventory.ItemCountChanged += OnItemCountChanged;
        RefreshAllSlots();
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.ItemCountChanged -= OnItemCountChanged;
    }

    private void OnItemCountChanged(string itemId, int _)
    {
        if (iconImages.ContainsKey(itemId))
            RefreshSlot(itemId);
    }

    private void CreateHud()
    {
        Canvas canvas = new GameObject("Inventory Canvas", typeof(Canvas), typeof(CanvasScaler)).GetComponent<Canvas>();
        canvas.transform.SetParent(transform, false);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 101;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject bar = new GameObject("Emergency Inventory", typeof(RectTransform));
        bar.transform.SetParent(canvas.transform, false);
        RectTransform barRect = bar.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.5f, 0f);
        barRect.anchorMax = new Vector2(0.5f, 0f);
        barRect.pivot = new Vector2(0.5f, 0f);
        barRect.anchoredPosition = new Vector2(0f, 28f);
        barRect.sizeDelta = new Vector2(520f, 120f);

        for (int index = 0; index < itemIds.Length; index++)
            CreateSlot(bar.transform, index);
    }

    private void CreateSlot(Transform parent, int index)
    {
        GameObject slot = new GameObject($"Slot {itemIds[index]}", typeof(RectTransform), typeof(Image));
        slot.transform.SetParent(parent, false);

        RectTransform slotRect = slot.GetComponent<RectTransform>();
        slotRect.anchorMin = new Vector2(0f, 0.5f);
        slotRect.anchorMax = new Vector2(0f, 0.5f);
        slotRect.pivot = new Vector2(0f, 0.5f);
        slotRect.anchoredPosition = new Vector2(index * 130f, 0f);
        slotRect.sizeDelta = new Vector2(112f, 112f);

        Image slotBackground = slot.GetComponent<Image>();
        slotBackground.color = new Color(0.1f, 0.07f, 0.05f, 0.86f);

        GameObject iconObject = new GameObject("Item Icon", typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(slot.transform, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(0f, 10f);
        iconRect.sizeDelta = new Vector2(72f, 72f);

        Image icon = iconObject.GetComponent<Image>();
        icon.sprite = FindItemSprite(sceneObjectNames[index]);
        icon.preserveAspect = true;
        iconImages[itemIds[index]] = icon;

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(slot.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.anchoredPosition = new Vector2(0f, 7f);
        labelRect.sizeDelta = new Vector2(0f, 22f);

        Text label = labelObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 14;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.text = itemNames[index];
    }

    private Sprite FindItemSprite(string objectName)
    {
        foreach (SpriteRenderer renderer in FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (renderer.gameObject.name == objectName)
                return renderer.sprite;
        }

        return null;
    }

    private void RefreshAllSlots()
    {
        foreach (string itemId in itemIds)
            RefreshSlot(itemId);
    }

    private void RefreshSlot(string itemId)
    {
        if (!iconImages.TryGetValue(itemId, out Image icon))
            return;

        icon.enabled = inventory != null && inventory.GetItemCount(itemId) > 0 && icon.sprite != null;
    }
}
