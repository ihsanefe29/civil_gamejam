using System.Collections.Generic;
using System;
using UnityEngine;

[DisallowMultipleComponent]
public class EmergencyBag : MonoBehaviour, IInteractionPromptSource
{
    [SerializeField] private List<string> acceptedItemIds = new();
    [SerializeField] private bool preparationOnly = true;
    [SerializeField] private EarthquakeLevelManager earthquakeLevelManager;

    private readonly Dictionary<string, int> storedItemCounts = new();
    private int lastDepositFrame = -1;

    public event Action<int> ItemDeposited;

    public int StoredItemCount
    {
        get
        {
            int total = 0;

            foreach (int count in storedItemCounts.Values)
                total += count;

            return total;
        }
    }

    public void DepositItems()
    {
        DepositOneItem();
    }

    public bool DepositOneItem()
    {
        // Some scene objects can have more than one UnityEvent listener while
        // being assembled. One E press must still move only one item.
        if (lastDepositFrame == Time.frameCount)
            return false;

        if (preparationOnly && !IsPreparationPhase())
            return false;

        Inventory playerInventory = FindFirstObjectByType<Inventory>();

        if (playerInventory == null)
            return false;

        foreach (string itemId in acceptedItemIds)
        {
            if (!playerInventory.TryRemoveItem(itemId))
                continue;

            storedItemCounts.TryGetValue(itemId, out int currentCount);
            storedItemCounts[itemId] = currentCount + 1;
            lastDepositFrame = Time.frameCount;
            ItemDeposited?.Invoke(StoredItemCount);
            return true;
        }

        return false;
    }

    public void Configure(IEnumerable<string> itemIds, EarthquakeLevelManager manager)
    {
        acceptedItemIds = new List<string>(itemIds);
        earthquakeLevelManager = manager;
    }

    public int GetStoredItemCount(string itemId)
    {
        return storedItemCounts.TryGetValue(itemId, out int count) ? count : 0;
    }

    public bool TryGetInteractionPrompt(out string prompt)
    {
        Inventory inventory = FindFirstObjectByType<Inventory>();
        bool canDeposit = (!preparationOnly || IsPreparationPhase()) &&
                          inventory != null &&
                          HasAcceptedItem(inventory);

        prompt = canDeposit ? "E - Çantaya Koy" : null;
        return canDeposit;
    }

    private bool HasAcceptedItem(Inventory inventory)
    {
        foreach (string itemId in acceptedItemIds)
            if (inventory.HasItem(itemId))
                return true;

        return false;
    }

    private bool IsPreparationPhase()
    {
        if (earthquakeLevelManager == null)
            earthquakeLevelManager = FindFirstObjectByType<EarthquakeLevelManager>();

        return earthquakeLevelManager == null ||
               earthquakeLevelManager.CurrentPhase == EarthquakePhase.Preparation;
    }
}
