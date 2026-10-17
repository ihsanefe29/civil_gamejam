using System.Collections.Generic;
using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Inventory : MonoBehaviour
{
    private readonly Dictionary<string, int> itemCounts = new();

    public event Action<string, int> ItemCountChanged;

    public bool TryAddItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return false;

        itemCounts.TryGetValue(itemId, out int currentCount);
        int newCount = currentCount + 1;
        itemCounts[itemId] = newCount;
        ItemCountChanged?.Invoke(itemId, newCount);

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

        int newCount = currentCount - 1;
        if (newCount == 0)
            itemCounts.Remove(itemId);
        else
            itemCounts[itemId] = newCount;

        ItemCountChanged?.Invoke(itemId, newCount);

        return true;
    }

    public bool HasItem(string itemId)
    {
        return GetItemCount(itemId) > 0;
    }
}
