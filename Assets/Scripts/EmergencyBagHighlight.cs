using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EmergencyBagHighlight : MonoBehaviour
{
    [SerializeField] private Color glowColor = new(1f, 0.82f, 0.2f, 0.45f);
    [SerializeField, Min(0.01f)] private float pulseSpeed = 3f;
    [SerializeField, Min(0f)] private float pulseScale = 0.1f;

    private readonly List<string> acceptedItems = new();
    private EmergencyBag bag;
    private Inventory inventory;
    private SpriteRenderer bagRenderer;
    private SpriteRenderer glowRenderer;
    private TextMesh hint;
    private Vector3 originalScale;

    public void Configure(EmergencyBag targetBag, IEnumerable<string> itemIds)
    {
        bag = targetBag;
        acceptedItems.Clear();
        acceptedItems.AddRange(itemIds);
    }

    private void Start()
    {
        bagRenderer = GetComponent<SpriteRenderer>();
        inventory = FindFirstObjectByType<Inventory>();
        originalScale = transform.localScale;
        CreateGlow();
        CreateHint();
        Refresh();

        if (inventory != null)
            inventory.ItemCountChanged += OnInventoryChanged;
        if (bag != null)
            bag.ItemDeposited += OnBagItemDeposited;
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.ItemCountChanged -= OnInventoryChanged;
        if (bag != null)
            bag.ItemDeposited -= OnBagItemDeposited;
    }

    private void Update()
    {
        bool readyToDeposit = HasAcceptedInventoryItem();
        if (glowRenderer != null)
            glowRenderer.enabled = readyToDeposit;
        if (hint != null)
            hint.gameObject.SetActive(readyToDeposit);

        if (!readyToDeposit)
        {
            transform.localScale = originalScale;
            return;
        }

        float wave = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
        transform.localScale = originalScale * (1f + wave * pulseScale);
        if (glowRenderer != null)
        {
            Color color = glowColor;
            color.a *= 0.65f + wave * 0.35f;
            glowRenderer.color = color;
        }
    }

    private void OnInventoryChanged(string _, int __) => Refresh();
    private void OnBagItemDeposited(int _) => Refresh();

    private void Refresh()
    {
        bool readyToDeposit = HasAcceptedInventoryItem();
        if (glowRenderer != null) glowRenderer.enabled = readyToDeposit;
        if (hint != null) hint.gameObject.SetActive(readyToDeposit);
    }

    private bool HasAcceptedInventoryItem()
    {
        if (inventory == null) inventory = FindFirstObjectByType<Inventory>();
        if (inventory == null) return false;

        foreach (string itemId in acceptedItems)
            if (inventory.HasItem(itemId)) return true;
        return false;
    }

    private void CreateGlow()
    {
        if (bagRenderer == null || transform.Find("Bag Glow") != null) return;

        GameObject glow = new("Bag Glow");
        glow.transform.SetParent(transform, false);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = Vector3.one * 1.2f;

        glowRenderer = glow.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = bagRenderer.sprite;
        glowRenderer.sharedMaterial = bagRenderer.sharedMaterial;
        glowRenderer.sortingLayerID = bagRenderer.sortingLayerID;
        glowRenderer.sortingOrder = bagRenderer.sortingOrder - 1;
        glowRenderer.color = glowColor;
        glowRenderer.enabled = false;
    }

    private void CreateHint()
    {
        if (transform.Find("Bag Deposit Hint") != null) return;

        GameObject hintObject = new("Bag Deposit Hint");
        hintObject.transform.SetParent(transform, false);
        hintObject.transform.localPosition = new Vector3(0f, 0.6f, 0f);

        hint = hintObject.AddComponent<TextMesh>();
        hint.text = "E  ÇANTAYA KOY";
        hint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hint.fontSize = 32;
        hint.characterSize = 0.018f;
        hint.anchor = TextAnchor.MiddleCenter;
        hint.alignment = TextAlignment.Center;
        hint.color = new Color(1f, 0.94f, 0.55f);
        hint.GetComponent<MeshRenderer>().sortingOrder = (bagRenderer != null ? bagRenderer.sortingOrder : 0) + 20;
        hintObject.SetActive(false);
    }
}
