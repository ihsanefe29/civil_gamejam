using UnityEngine;

public class CollectibleItem : MonoBehaviour, IInteractionPromptSource
{
    [SerializeField] private string itemId;
    [SerializeField] private bool hideAfterCollection = true;
    [SerializeField] private EarthquakeLevelManager earthquakeLevelManager;
    [SerializeField] private bool preparationOnly = true;

    private bool collected;

    public bool IsAvailableToCollect => !collected &&
        (!preparationOnly || IsPreparationPhase());

    private void Awake()
    {
        // Every pickup remains an Interactable. This listener is registered in
        // code as a safety net so a missing Inspector event can never leave a
        // visible item that does nothing when the player presses E.
        Interactable interactable = GetComponent<Interactable>();
        if (interactable != null)
            interactable.AddListener(Collect);

    }

    public void Collect()
    {
        if (preparationOnly && !IsPreparationPhase())
            return;

        Inventory inventory = FindFirstObjectByType<Inventory>();

        if (collected || inventory == null)
            return;

        string resolvedItemId = string.IsNullOrWhiteSpace(itemId)
            ? gameObject.name
            : itemId;

        if (!inventory.TryAddItem(resolvedItemId))
            return;

        collected = true;

        if (hideAfterCollection)
            gameObject.SetActive(false);
    }

    public void Configure(string newItemId)
    {
        itemId = newItemId;
    }

    public bool TryGetInteractionPrompt(out string prompt)
    {
        prompt = IsAvailableToCollect ? "E - Collect" : null;
        return prompt != null;
    }

    private bool IsPreparationPhase()
    {
        if (earthquakeLevelManager == null)
            earthquakeLevelManager = FindFirstObjectByType<EarthquakeLevelManager>();

        return earthquakeLevelManager == null ||
               earthquakeLevelManager.CurrentPhase == EarthquakePhase.Preparation;
    }
}
