using System;
using UnityEngine;

[RequireComponent(typeof(EarthquakeLevelManager))]
public class EarthquakeTaskManager : MonoBehaviour
{
    public const int RequiredLocks = 2;
    public const int RequiredBagItems = 4;

    public event Action StateChanged;

    public int InstalledLockCount { get; private set; }
    public int BagItemCount { get; private set; }
    public bool IsFridgeSecured { get; private set; }

    private EmergencyBag emergencyBag;
    private FridgeMountTask fridgeTask;
    private EarthquakeLevelManager levelManager;
    private int lastLocks = -1;
    private int lastBagItems = -1;
    private bool lastFridgeSecured;
    private bool earlyEarthquakeRequested;

    private void Start()
    {
        levelManager = GetComponent<EarthquakeLevelManager>();
        if (GetComponent<EarthquakeTaskListUI>() == null)
            gameObject.AddComponent<EarthquakeTaskListUI>();

        RefreshState();
    }

    private void Update() => RefreshState();

    private void RefreshState()
    {
        // The scene contains an older deposit helper too. The level manager
        // owns the visible bag that actually receives player items.
        emergencyBag = levelManager != null ? levelManager.ActiveEmergencyBag : null;
        fridgeTask ??= FindFirstObjectByType<FridgeMountTask>();

        int locks = 0;
        foreach (LockInstaller installer in FindObjectsByType<LockInstaller>(FindObjectsSortMode.None))
            if (installer.IsInstalled) locks++;

        int bagItems = emergencyBag != null ? emergencyBag.StoredItemCount : 0;
        bool fridgeSecured = fridgeTask != null && fridgeTask.IsMounted;

        InstalledLockCount = Mathf.Min(locks, RequiredLocks);
        BagItemCount = Mathf.Min(bagItems, RequiredBagItems);
        IsFridgeSecured = fridgeSecured;

        if (lastLocks == InstalledLockCount && lastBagItems == BagItemCount && lastFridgeSecured == IsFridgeSecured)
            return;

        lastLocks = InstalledLockCount;
        lastBagItems = BagItemCount;
        lastFridgeSecured = IsFridgeSecured;
        StateChanged?.Invoke();

        TryBeginEarthquakeWhenAllTasksAreComplete();
    }

    private void TryBeginEarthquakeWhenAllTasksAreComplete()
    {
        if (earlyEarthquakeRequested || levelManager == null ||
            levelManager.CurrentPhase != EarthquakePhase.Preparation)
            return;

        bool allTasksComplete =
            InstalledLockCount >= RequiredLocks &&
            BagItemCount >= RequiredBagItems &&
            IsFridgeSecured;

        if (allTasksComplete && levelManager.BeginEarthquakeEarly())
            earlyEarthquakeRequested = true;
    }
}
