using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class InventoryInteractionEvent : UnityEvent<Inventory>
{
}

public class Interactable : MonoBehaviour
{
    [Header("Phase")]
    [SerializeField] private bool requiresEarthquakePhase;
    [SerializeField] private EarthquakePhase allowedEarthquakePhase = EarthquakePhase.Preparation;
    [SerializeField] private EarthquakeLevelManager earthquakeLevelManager;

    [Header("Event")]
    [SerializeField] private InventoryInteractionEvent onInteractionAllowed;

    public bool Interact(Inventory inventory)
    {
        if (requiresEarthquakePhase &&
            (earthquakeLevelManager == null ||
             earthquakeLevelManager.CurrentPhase != allowedEarthquakePhase))
        {
            return false;
        }

        onInteractionAllowed?.Invoke(inventory);
        return true;
    }

    public bool Interact()
    {
        return Interact(null);
    }
}
