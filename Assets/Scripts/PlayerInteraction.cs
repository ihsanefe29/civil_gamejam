using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Transform interactionRange;

    private readonly HashSet<Interactable> nearbyInteractables = new();
    private Inventory inventory;

    private void Awake()
    {
        if (interactionRange == null)
            interactionRange = transform;

        inventory = GetComponent<Inventory>();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E))
            return;

        Interactable nearestInteractable = GetNearestInteractable();

        if (nearestInteractable != null)
            nearestInteractable.Interact(inventory);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("interactable"))
            return;

        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable != null)
            nearbyInteractables.Add(interactable);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("interactable"))
            return;

        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable != null)
            nearbyInteractables.Remove(interactable);
    }

    private Interactable GetNearestInteractable()
    {
        Interactable nearest = null;
        float nearestDistanceSqr = float.MaxValue;

        foreach (Interactable interactable in nearbyInteractables)
        {
            if (interactable == null)
                continue;

            float distanceSqr = (interactable.transform.position - interactionRange.position).sqrMagnitude;

            if (distanceSqr < nearestDistanceSqr)
            {
                nearest = interactable;
                nearestDistanceSqr = distanceSqr;
            }
        }

        return nearest;
    }
}
