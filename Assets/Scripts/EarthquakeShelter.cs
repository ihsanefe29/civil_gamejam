using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EarthquakeShelter : MonoBehaviour, IInteractionPromptSource
{
    private const string EarthquakeCrouchTrigger = "earthquake_event";

    [SerializeField] private EarthquakeLevelManager earthquakeLevelManager;
    [SerializeField] private UnityEvent onSheltered;
    [Tooltip("Optional child used only to choose the horizontal shelter position. The player is still snapped to the floor.")]
    [SerializeField] private Transform shelterPoint;
    [Header("Visual Placement")]
    [Tooltip("Small visual adjustment for crouch sprites whose feet do not share the walking sprite's pivot.")]
    [SerializeField] private float crouchGroundOffset = 0f;
    [SerializeField, Min(0f)] private float restoreDelayAfterEarthquake = 1f;
    [Header("Animator Triggers")]
    [SerializeField] private string normalTrigger = "normal";

    private bool sheltered;
    private bool restoreQueued;
    private Animator playerAnimator;
    private PlayerController playerController;
    private Rigidbody2D playerBody;
    private PlayerFootAlignment playerFootAlignment;
    private SpriteRenderer playerRenderer;
    private bool restoredPlayerVisual = true;
    private int originalPlayerSortingLayer;
    private int originalPlayerSortingOrder;

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
        PlacePlayerUnderTable();
        TriggerPlayerAnimation(EarthquakeCrouchTrigger);
        playerController?.SetMovementLocked(true);
        earthquakeLevelManager.RegisterShelter();
        onSheltered?.Invoke();
    }

    private void Update()
    {
        if (!sheltered || earthquakeLevelManager == null ||
            earthquakeLevelManager.CurrentPhase == EarthquakePhase.Earthquake)
            return;

        if (!restoreQueued)
            StartCoroutine(RestoreAfterEarthquake());
    }

    private IEnumerator RestoreAfterEarthquake()
    {
        restoreQueued = true;
        yield return new WaitForSeconds(restoreDelayAfterEarthquake);

        // If a phase were ever restarted while waiting, do not pull the
        // player out of shelter during that new earthquake.
        if (earthquakeLevelManager != null &&
            earthquakeLevelManager.CurrentPhase == EarthquakePhase.Earthquake)
        {
            restoreQueued = false;
            yield break;
        }

        sheltered = false;
        restoreQueued = false;
        RestorePlayerAfterShelter();
        TriggerPlayerAnimation(normalTrigger);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        restoreQueued = false;
        TriggerPlayerAnimation(normalTrigger);
        RestorePlayerAfterShelter();
    }

    // This method only sends a trigger to the Animator already assigned to
    // the player. It never assigns a controller or creates transitions.
    private void TriggerPlayerAnimation(string trigger)
    {
        ResolvePlayerReferences();

        if (playerAnimator == null || playerAnimator.runtimeAnimatorController == null)
            return;

        playerAnimator.SetTrigger(trigger);
    }

    private void ResolvePlayerReferences()
    {
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        if (playerController != null)
        {
            // The Animator may be on a child object of the player prefab.
            // Search the complete player hierarchy before using a fallback.
            if (playerAnimator == null)
                playerAnimator = playerController.GetComponentInChildren<Animator>(true);
        }

        if (playerAnimator == null)
            playerAnimator = FindFirstObjectByType<Animator>();
    }

    private void PlacePlayerUnderTable()
    {
        ResolvePlayerReferences();
        shelterPoint ??= transform.Find("ShelterPoint");
        if (playerController == null)
            return;

        // Furniture pivots are inconsistent, so never use transform.position
        // as a Y destination. The visual bounds identify the table's usable
        // horizontal span; the player is then snapped to the actual floor.
        float shelterX = GetShelterX();
        Vector3 shelterPosition = GetFloorPosition(shelterX);
        shelterPosition.y += crouchGroundOffset;

        playerController.transform.position = shelterPosition;
        playerBody = playerController.GetComponent<Rigidbody2D>();
        if (playerBody != null)
        {
            playerBody.position = shelterPosition;
            playerBody.linearVelocity = Vector2.zero;
        }

        PutPlayerInFrontOfTable();
        Physics2D.SyncTransforms();
    }

    private float GetShelterX()
    {
        if (shelterPoint != null)
            return shelterPoint.position.x;

        SpriteRenderer tableRenderer = GetComponentInChildren<SpriteRenderer>();
        if (tableRenderer == null)
            return transform.position.x;

        // The centre is consistently underneath a side-view table and keeps
        // the crouching player out of the chair/leg silhouettes at its edges.
        return tableRenderer.bounds.center.x;
    }

    private Vector3 GetFloorPosition(float x)
    {
        playerFootAlignment ??= playerController.GetComponent<PlayerFootAlignment>();
        if (playerFootAlignment != null &&
            playerFootAlignment.TryGetGroundAlignedPosition(x, out Vector3 floorPosition))
            return floorPosition;

        Vector3 fallback = playerController.transform.position;
        fallback.x = x;
        return fallback;
    }

    private void PutPlayerInFrontOfTable()
    {
        playerRenderer ??= playerController.GetComponentInChildren<SpriteRenderer>(true);
        if (playerRenderer == null || !restoredPlayerVisual)
            return;

        originalPlayerSortingLayer = playerRenderer.sortingLayerID;
        originalPlayerSortingOrder = playerRenderer.sortingOrder;

        int frontOrder = originalPlayerSortingOrder;
        foreach (SpriteRenderer tableRenderer in GetComponentsInChildren<SpriteRenderer>(true))
            if (tableRenderer.sortingLayerID == originalPlayerSortingLayer)
                frontOrder = Mathf.Max(frontOrder, tableRenderer.sortingOrder + 1);

        playerRenderer.sortingOrder = frontOrder;
        restoredPlayerVisual = false;
    }

    private void RestorePlayerAfterShelter()
    {
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        playerController?.SetMovementLocked(false);

        if (!restoredPlayerVisual && playerRenderer != null)
        {
            playerRenderer.sortingLayerID = originalPlayerSortingLayer;
            playerRenderer.sortingOrder = originalPlayerSortingOrder;
            restoredPlayerVisual = true;
        }

        playerBody = null;
    }

    public bool TryGetInteractionPrompt(out string prompt)
    {
        earthquakeLevelManager ??= FindFirstObjectByType<EarthquakeLevelManager>();
        bool canShelter = !sheltered &&
                          earthquakeLevelManager != null &&
                          earthquakeLevelManager.CurrentPhase == EarthquakePhase.Earthquake;

        prompt = canShelter ? "E - Masanın Altına Saklan" : null;
        return canShelter;
    }
}
