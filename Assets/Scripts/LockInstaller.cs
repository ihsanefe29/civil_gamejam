using UnityEngine;
using UnityEngine.Events;

public class LockInstaller : MonoBehaviour, IInteractionPromptSource
{
    [SerializeField] private string requiredItemId = "cabinet_lock";
    [SerializeField, Min(0)] private int scoreOnInstall = 50;
    [SerializeField] private GameObject installedLockVisual;
    [SerializeField] private EarthquakeLevelManager earthquakeLevelManager;
    [SerializeField] private UnityEvent onInstalled;

    private bool installed;

    public bool IsInstalled => installed;
    public event System.Action Installed;

    private void Awake()
    {
        if (installedLockVisual != null)
            installedLockVisual.SetActive(false);
    }

    public void Install()
    {
        if (earthquakeLevelManager == null)
            earthquakeLevelManager =
                FindFirstObjectByType<EarthquakeLevelManager>();

        if (earthquakeLevelManager != null &&
            earthquakeLevelManager.CurrentPhase != EarthquakePhase.Preparation)
            return;

        Inventory inventory = FindFirstObjectByType<Inventory>();

        if (installed || inventory == null ||
            !inventory.TryRemoveItem(requiredItemId))
            return;

        installed = true;
        installedLockVisual?.SetActive(true);

        // Kilit başarıyla takılınca sesi çal.
        Level1AudioManager audioManager = Level1AudioManager.Instance;

        if (audioManager == null)
            audioManager = FindFirstObjectByType<Level1AudioManager>();

        if (audioManager != null)
            audioManager.PlayLock();

        earthquakeLevelManager?.AddTaskScore(scoreOnInstall);
        Installed?.Invoke();
        onInstalled?.Invoke();
    }

    public void Configure(GameObject lockVisual, int score)
    {
        installedLockVisual = lockVisual;
        scoreOnInstall = score;

        if (installedLockVisual != null)
            installedLockVisual.SetActive(false);
    }

    public bool TryGetInteractionPrompt(out string prompt)
    {
        earthquakeLevelManager ??=
            FindFirstObjectByType<EarthquakeLevelManager>();

        Inventory inventory = FindFirstObjectByType<Inventory>();

        bool canInstall = !installed &&
                          (earthquakeLevelManager == null ||
                           earthquakeLevelManager.CurrentPhase ==
                           EarthquakePhase.Preparation) &&
                          inventory != null &&
                          inventory.HasItem(requiredItemId);

        prompt = canInstall ? "E - Lock" : null;
        return canInstall;
    }
}