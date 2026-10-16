using UnityEngine;
using UnityEngine.Events;

public class LockInstaller : MonoBehaviour
{
    [SerializeField] private string requiredItemId = "cabinet_lock";
    [SerializeField, Min(0)] private int scoreOnInstall = 50;
    [SerializeField] private EarthquakeLevelManager earthquakeLevelManager;
    [SerializeField] private UnityEvent onInstalled;

    private bool installed;

    public void Install(Inventory inventory)
    {
        if (installed || inventory == null || !inventory.TryRemoveItem(requiredItemId))
            return;

        installed = true;
        earthquakeLevelManager?.AddTaskScore(scoreOnInstall);
        onInstalled?.Invoke();
    }
}
