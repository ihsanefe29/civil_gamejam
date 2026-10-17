using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Transform interactionRange;
    // Lets the player collect small props that are visually behind furniture
    // without needing to overlap the furniture sprite.
    [SerializeField, Min(0.1f)] private float interactionRadius = 1.55f;

    private readonly HashSet<Interactable> nearbyInteractables = new();
    private void Awake()
    {
        if (interactionRange == null)
            interactionRange = transform;

    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E))
            return;

        Interactable nearestInteractable = GetNearestInteractable();

        if (nearestInteractable != null)
            nearestInteractable.Interact();
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

        // A small pickup can be visually covered by furniture. When it is in
        // range, it must win over a nearby table/cabinet interaction.
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(interactionRange.position, interactionRadius))
            if (collider.CompareTag("interactable"))
                ConsiderCandidate(collider.GetComponent<Interactable>(), true, ref nearest, ref nearestDistanceSqr);

        foreach (Interactable interactable in nearbyInteractables)
            ConsiderCandidate(interactable, true, ref nearest, ref nearestDistanceSqr);

        if (nearest != null)
            return nearest;

        foreach (Collider2D collider in Physics2D.OverlapCircleAll(interactionRange.position, interactionRadius))
            if (collider.CompareTag("interactable"))
                ConsiderCandidate(collider.GetComponent<Interactable>(), false, ref nearest, ref nearestDistanceSqr);

        foreach (Interactable interactable in nearbyInteractables)
            ConsiderCandidate(interactable, false, ref nearest, ref nearestDistanceSqr);

        return nearest;
    }

    private void ConsiderCandidate(
        Interactable interactable,
        bool collectiblesOnly,
        ref Interactable nearest,
        ref float nearestDistanceSqr)
    {
        if (interactable == null || !interactable.isActiveAndEnabled)
            return;

        bool isCollectible = interactable.GetComponent<CollectibleItem>() != null;
        if (collectiblesOnly != isCollectible)
            return;

        float distanceSqr = (interactable.transform.position - interactionRange.position).sqrMagnitude;
        if (distanceSqr >= nearestDistanceSqr)
            return;

        nearest = interactable;
        nearestDistanceSqr = distanceSqr;
    }
}
