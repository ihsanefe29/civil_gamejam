using UnityEngine;

public class EarthquakeSceneBindings : MonoBehaviour
{
    [SerializeField] private string[] emergencyItemIds = { "first_aid_kit", "water_bottle", "battery", "torch" };
    [Header("Portal")]
    [Tooltip("Optional direct reference. Drag the Portal stable scene object here to avoid relying on its name.")]
    [SerializeField] private GameObject portalObject;
    [Tooltip("Leave empty until the next level is created and added to Build Settings.")]
    [SerializeField] private string portalNextSceneName;

    private EarthquakeLevelManager levelManager;
    private PortalController portalController;
    private GameObject portalInstance;
    private bool portalWarningLogged;

    private void Awake()
    {
        levelManager = GetComponent<EarthquakeLevelManager>();
        ConfigurePlayerFootAlignment();
        // There are exactly two cabinet locks in this level: one for each
        // cabinet. The old demo pickup is a third lock and made it look as if
        // a single lock could be used twice.
        DisableLegacyLockPickup();
        ConfigureLockPickup("kilit ve zincir");
        ConfigureLockPickup("kilit ve zincir (1)");
        ConfigureEmergencyCollectible("First aid kit", "first_aid_kit", new Vector2(0.9f, 0.9f));
        ConfigureEmergencyCollectible("Water bottle", "water_bottle", new Vector2(0.9f, 0.9f));
        ConfigureEmergencyCollectible("Battery", "battery", new Vector2(0.8f, 0.8f));
        ConfigureEmergencyCollectible("Torch", "torch", new Vector2(1f, 0.8f));
        DisableLegacyEmergencyBag();
        ConfigureEmergencyBag();
        ConfigureSmallCabinet();
        ConfigureFridgeMountTask();
        ConfigureShelterTables();
        ConfigurePortal();
    }

    private void Start()
    {
        // The portal prefab and its Animator can be initialized after this
        // component's Awake depending on Unity's scene load order. A second,
        // idempotent pass here makes the runtime wiring deterministic.
        ConfigurePortal();
    }

    private void Update()
    {
        if (portalController == null)
            ConfigurePortal();

        if (portalController == null || levelManager == null)
            return;

        if (levelManager.CurrentPhase == EarthquakePhase.Aftermath ||
            levelManager.CurrentPhase == EarthquakePhase.Completed)
        {
            if (portalInstance != null && !portalInstance.activeSelf)
                portalInstance.SetActive(true);

            portalController.OpenPortal();
        }
    }

    private static void ConfigurePlayerFootAlignment()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            GetOrAdd<PlayerFootAlignment>(player.gameObject);
    }

    private static void DisableLegacyLockPickup()
    {
        GameObject legacyPickup = FindByExactName("Lock Pickup (Collectible)");
        if (legacyPickup != null)
            legacyPickup.SetActive(false);
    }

    private static void DisableLegacyEmergencyBag()
    {
        // This invisible helper overlaps the drawn bag. Leaving it active
        // causes E to deposit into its separate inventory instead.
        GameObject legacyBag = FindByExactName("Emergency Bag (Deposit)");
        if (legacyBag != null)
            legacyBag.SetActive(false);
    }

    private void ConfigureEmergencyBag()
    {
        GameObject bagObject = FindByName("anta");
        if (bagObject == null) return;

        EarthquakeLevelManager manager = GetComponent<EarthquakeLevelManager>();
        if (manager == null || emergencyItemIds == null) return;

        EnsureTrigger(bagObject, new Vector2(0.8f, 0.7f));
        EmergencyBag bag = GetOrAdd<EmergencyBag>(bagObject);
        if (bag == null) return;

        bag.Configure(emergencyItemIds, manager);
        Interactable bagInteractable = GetOrAdd<Interactable>(bagObject);
        if (bagInteractable != null)
            bagInteractable.AddListener(bag.DepositItems);

        GetOrAdd<EmergencyBagHighlight>(bagObject).Configure(bag, emergencyItemIds);

        manager.SetEmergencyBag(bag);
    }

    private void ConfigureFridgeMountTask()
    {
        GameObject fridge = FindByName("buzdol");
        if (fridge == null) return;

        EarthquakeLevelManager manager = GetComponent<EarthquakeLevelManager>();
        EnsureTrigger(fridge, new Vector2(1.4f, 2.2f));

        FridgeMountTask task = GetOrAdd<FridgeMountTask>(fridge);
        task.Configure(manager);

        Interactable interactable = GetOrAdd<Interactable>(fridge);
        interactable.AddListener(task.BeginMounting);
    }

    private static void ConfigureShelterTables()
    {
        foreach (Transform candidate in FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            string objectName = candidate.name.ToLowerInvariant();
            // Include the kitchen table, bedroom side/coffee tables and desk.
            if (!objectName.Contains("table") && !objectName.Contains("masa") &&
                !objectName.Contains("sehpa") && !objectName.Contains("desk")) continue;

            GameObject table = candidate.gameObject;
            EnsureTrigger(table, new Vector2(1.25f, 0.75f));

            EarthquakeShelter shelter = GetOrAdd<EarthquakeShelter>(table);
            Interactable interactable = GetOrAdd<Interactable>(table);
            interactable.AddListener(shelter.HideUnderTable);
        }
    }

    private void ConfigurePortal()
    {
        // Prefer the newly placed portal prefab. Older scene saves contain a
        // misspelled "potal" object, so keep it as a backwards-compatible
        // fallback instead of relying on one exact runtime name.
        GameObject portal = portalObject != null ? portalObject : FindPortalObject();
        if (portal == null)
        {
            if (!portalWarningLogged)
            {
                Debug.LogWarning("EarthquakeSceneBindings could not find a Portal stable object. Assign it in the Earthquake Level Manager inspector.");
                portalWarningLogged = true;
            }
            return;
        }

        portalWarningLogged = false;
        portalInstance = portal;
        EnsureTrigger(portal, new Vector2(1.1f, 1.8f));
        GetOrAdd<Interactable>(portal);
        portalController = GetOrAdd<PortalController>(portal);
        portalController.ConfigureForLevel(levelManager, portalNextSceneName);

        // The portal's own Animator begins its opening clip when this object
        // is enabled after the earthquake. Keep it inactive during gameplay
        // instead of hiding renderers or sending Animator triggers.
        if (levelManager != null && levelManager.CurrentPhase == EarthquakePhase.Preparation)
            portal.SetActive(false);
    }

    private static GameObject FindPortalObject()
    {
        GameObject fallback = null;

        foreach (Transform candidate in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string objectName = candidate.name;
            if (string.Equals(objectName, "Portal stable", System.StringComparison.OrdinalIgnoreCase))
                return candidate.gameObject;

            if (objectName.IndexOf("portal", System.StringComparison.OrdinalIgnoreCase) >= 0)
                fallback ??= candidate.gameObject;
            else if (string.Equals(objectName, "potal", System.StringComparison.OrdinalIgnoreCase))
                fallback ??= candidate.gameObject;
        }

        return fallback;
    }

    private void ConfigureSmallCabinet()
    {
        GameObject cabinet = FindByName("dolap son");
        GameObject lockPickup = FindByExactName("kilit ve zincir (1)");
        if (cabinet == null || lockPickup == null) return;

        EnsureTrigger(cabinet, new Vector2(1.2f, 0.4f));

        GameObject lockVisual = CreateInstalledLockVisual(cabinet, lockPickup);
        LockInstaller installer = GetOrAdd<LockInstaller>(cabinet);
        if (installer == null) return;

        installer.Configure(lockVisual, 50);
        Interactable cabinetInteractable = GetOrAdd<Interactable>(cabinet);
        if (cabinetInteractable != null)
            cabinetInteractable.AddListener(installer.Install);
    }

    private static GameObject CreateInstalledLockVisual(GameObject cabinet, GameObject fallbackPickup)
    {
        const string visualName = "Installed Small Cabinet Lock Visual";
        Transform existing = cabinet.transform.Find(visualName);
        if (existing != null)
            return existing.gameObject;

        GameObject template = FindByExactName("kilit ve zincir") ?? fallbackPickup;
        SpriteRenderer templateRenderer = template.GetComponent<SpriteRenderer>();

        GameObject visual = new(visualName);
        visual.transform.SetParent(cabinet.transform);
        // The small cabinet's pivot is below its doors, so place the lock high
        // enough to sit across the two door handles instead of under the cabinet.
        visual.transform.localPosition = new Vector3(0f, 0.75f, 0f);
        visual.transform.localRotation = Quaternion.identity;

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        if (templateRenderer != null)
        {
            renderer.sprite = templateRenderer.sprite;
            renderer.sharedMaterial = templateRenderer.sharedMaterial;
            renderer.sortingLayerID = templateRenderer.sortingLayerID;
            renderer.sortingOrder = templateRenderer.sortingOrder + 10;
        }

        return visual;
    }

    private static void ConfigureLockPickup(string objectName)
    {
        GameObject lockPickup = FindByExactName(objectName);
        if (lockPickup == null) return;

        // The lock beside the table is partially covered by the chair. Give
        // both lock pickups a centered, forgiving interaction target.
        EnsureTrigger(lockPickup, new Vector2(1.05f, 1.05f));
        CollectibleItem collectible = GetOrAdd<CollectibleItem>(lockPickup);
        if (collectible == null) return;

        collectible.Configure("cabinet_lock");
        Interactable pickupInteractable = GetOrAdd<Interactable>(lockPickup);
        if (pickupInteractable != null)
            pickupInteractable.AddListener(collectible.Collect);

        if (objectName.EndsWith("(1)"))
            CopyLockSpriteToPickup(lockPickup);
    }

    private static void ConfigureEmergencyCollectible(string objectName, string itemId, Vector2 triggerSize)
    {
        GameObject pickup = FindByExactName(objectName);
        if (pickup == null) return;

        EnsureTrigger(pickup, triggerSize);
        CollectibleItem collectible = GetOrAdd<CollectibleItem>(pickup);
        if (collectible == null) return;

        collectible.Configure(itemId);
        Interactable interactable = GetOrAdd<Interactable>(pickup);
        interactable.AddListener(collectible.Collect);
    }

    private static void CopyLockSpriteToPickup(GameObject lockPickup)
    {
        GameObject template = FindByExactName("kilit ve zincir");
        if (template == null) return;

        SpriteRenderer templateRenderer = template.GetComponent<SpriteRenderer>();
        SpriteRenderer pickupRenderer = lockPickup.GetComponent<SpriteRenderer>();
        if (templateRenderer == null || pickupRenderer == null) return;

        pickupRenderer.sprite = templateRenderer.sprite;
        pickupRenderer.sharedMaterial = templateRenderer.sharedMaterial;
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : target.AddComponent<T>();
    }

    private static void EnsureTrigger(GameObject target, Vector2 size)
    {
        BoxCollider2D collider = GetOrAdd<BoxCollider2D>(target);
        collider.isTrigger = true;
        collider.offset = Vector2.zero;
        collider.size = size;
        target.tag = "interactable";
    }

    private static GameObject FindByName(string namePart)
    {
        foreach (Transform candidate in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (candidate.name.Contains(namePart)) return candidate.gameObject;
        return null;
    }

    private static GameObject FindByExactName(string name)
    {
        foreach (Transform candidate in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (candidate.name == name) return candidate.gameObject;
        return null;
    }
}
