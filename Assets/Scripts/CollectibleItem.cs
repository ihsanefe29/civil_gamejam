using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [SerializeField] private string itemId;
    [SerializeField] private bool hideAfterCollection = true;

    private bool collected;

    public void Collect(Inventory inventory)
    {
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
}
