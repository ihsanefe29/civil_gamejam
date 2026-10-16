using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Inventory : MonoBehaviour
{
    private readonly Dictionary<string, int> itemCounts = new();

    public bool TryAddItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return false;

        itemCounts.TryGetValue(itemId, out int currentCount);
        itemCounts[itemId] = currentCount + 1;

        return true;
    }

    public int GetItemCount(string itemId)
    {
        return itemCounts.TryGetValue(itemId, out int count) ? count : 0;
    }

    public bool TryRemoveItem(string itemId)
    {
        if (!itemCounts.TryGetValue(itemId, out int currentCount) || currentCount <= 0)
            return false;

        if (currentCount == 1)
            itemCounts.Remove(itemId);
        else
            itemCounts[itemId] = currentCount - 1;

        return true;
    }

    public bool HasItem(string itemId)
    {
        return GetItemCount(itemId) > 0;
    }
}
