using UnityEngine;
using UnityEngine.Events;

public class EarthquakeShelter : MonoBehaviour
{
    [SerializeField] private EarthquakeLevelManager earthquakeLevelManager;
    [SerializeField] private UnityEvent onSheltered;

    private bool sheltered;

    public void HideUnderTable()
    {
        if (sheltered)
            return;

        if (earthquakeLevelManager == null)
            earthquakeLevelManager = FindFirstObjectByType<EarthquakeLevelManager>();

        if (earthquakeLevelManager == null ||
            earthquakeLevelManager.CurrentPhase != EarthquakePhase.Earthquake)
            return;

        sheltered = true;
        earthquakeLevelManager.RegisterShelter();
        onSheltered?.Invoke();
    }
}
