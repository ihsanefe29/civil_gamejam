using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EmergencyBag : MonoBehaviour
{
    [SerializeField] private List<string> acceptedItemIds = new();

    private readonly Dictionary<string, int> storedItemCounts = new();

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

    public void DepositItems(Inventory playerInventory)
    {
        if (playerInventory == null)
            return;

        foreach (string itemId in acceptedItemIds)
        {
            while (playerInventory.TryRemoveItem(itemId))
            {
                storedItemCounts.TryGetValue(itemId, out int currentCount);
                storedItemCounts[itemId] = currentCount + 1;
            }
        }
    }

    public int GetStoredItemCount(string itemId)
    {
        return storedItemCounts.TryGetValue(itemId, out int count) ? count : 0;
    }
}
