using UnityEngine;

public class EarthquakeSceneBindings : MonoBehaviour
{
    [SerializeField] private string[] emergencyItemIds = { "first_aid_kit", "water_bottle", "battery", "torch" };

    private void Awake()
    {
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
        ConfigureEmergencyBag();
        ConfigureSmallCabinet();
        ConfigureFridgeMountTask();
    }

    private static void DisableLegacyLockPickup()
    {
        GameObject legacyPickup = FindByExactName("Lock Pickup (Collectible)");
        if (legacyPickup != null)
            legacyPickup.SetActive(false);
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

    private void ConfigureSmallCabinet()
    {
        GameObject cabinet = FindByName("dolap son");
        GameObject lockPickup = FindByExactName("kilit ve zincir (1)");
        if (cabinet == null || lockPickup == null) return;

        EnsureTrigger(cabinet, new Vector2(1.2f, 1.4f));

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
