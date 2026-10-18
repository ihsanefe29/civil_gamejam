using System.Collections;
using UnityEngine;

/// <summary>
/// Keeps the level-exit portal hidden until the earthquake has ended, then
/// exposes the existing portal animation and interaction collider exactly once.
/// The prefab starts its own authored animation when its GameObject is enabled;
/// this script deliberately does not control Animator states or triggers.
/// </summary>
[DisallowMultipleComponent]
public class PortalController : MonoBehaviour, IInteractionPromptSource
{
    [Header("Portal Timing")]
    [SerializeField, Min(0f)] private float openingDuration = 3f;

    [Header("Interaction")]
    [Tooltip("A portal remains non-interactable while it opens. This is the local collider size used when the scene binding creates the trigger at runtime.")]
    [SerializeField] private Vector2 interactionColliderSize = new(1.1f, 1.8f);
    [SerializeField, Min(0.1f)] private float directInteractionDistance = 2f;

    [Header("Optional Scene Transition")]
    [Tooltip("Leave empty until the next level exists and is added to Build Settings.")]
    [SerializeField] private string nextSceneName;

    private EarthquakeLevelManager levelManager;
    private Collider2D portalCollider;
    private LevelCompleteUI levelCompleteUi;
    private bool portalOpened;
    private bool portalReady;
    private bool transitionRequested;

    private void Awake()
    {
        ResolveReferences();
        SetInteractionEnabled(false);
    }

    private void Update()
    {
        if (!portalOpened)
        {
            levelManager ??= FindFirstObjectByType<EarthquakeLevelManager>();
            if (levelManager != null &&
                (levelManager.CurrentPhase == EarthquakePhase.Aftermath ||
                 levelManager.CurrentPhase == EarthquakePhase.Completed))
                OpenPortal();
        }

        // This is deliberately a fallback to PlayerInteraction. The portal
        // must never become unusable merely because its prefab collider/tag
        // was reimported or changed while the scene was being assembled.
        if (portalReady && !transitionRequested && Input.GetKeyDown(KeyCode.E))
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null && Vector2.Distance(player.transform.position, transform.position) <= directInteractionDistance)
                EnterPortal();
        }
    }

    /// <summary>
    /// Public so a future cutscene or UnityEvent can open the same portal,
    /// without duplicating its animation/collider logic.
    /// </summary>
    public void OpenPortal()
    {
        if (portalOpened)
            return;

        portalOpened = true;
        StartCoroutine(EnableInteractionAfterOpening());
    }

    public void ConfigureNextScene(string sceneName)
    {
        nextSceneName = sceneName;
    }

    /// <summary>
    /// Called by the level's scene binder. Keeping this here makes the portal
    /// self-sufficient even when the prefab itself has no gameplay components
    /// saved on it yet.
    /// </summary>
    public void ConfigureForLevel(EarthquakeLevelManager manager, string sceneName)
    {
        levelManager = manager != null ? manager : levelManager;
        ConfigureNextScene(sceneName);

        ResolveReferences();
        if (portalCollider is BoxCollider2D boxCollider)
        {
            boxCollider.size = interactionColliderSize;
            boxCollider.offset = Vector2.zero;
        }
    }

    private IEnumerator EnableInteractionAfterOpening()
    {
        if (openingDuration > 0f)
            yield return new WaitForSeconds(openingDuration);

        portalReady = true;
        SetInteractionEnabled(true);
    }

    public void EnterPortal()
    {
        if (!portalReady || transitionRequested)
            return;

        transitionRequested = true;
        SetInteractionEnabled(false);
        FindFirstObjectByType<PlayerController>()?.SetMovementLocked(true);

        levelManager ??= FindFirstObjectByType<EarthquakeLevelManager>();
        levelManager?.CompleteLevel();

        levelCompleteUi ??= GetComponent<LevelCompleteUI>() ?? gameObject.AddComponent<LevelCompleteUI>();
        levelCompleteUi.Show(levelManager != null ? levelManager.TotalScore : 0, ContinueToNextFloor);
    }

    private void ContinueToNextFloor()
    {
        if (!HasValidDestination())
        {
            levelCompleteUi.ShowUnavailableDestination();
            return;
        }

        levelCompleteUi.PullIntoPortal(transform, LoadNextScene);
    }

    private bool HasValidDestination()
    {
        return !string.IsNullOrWhiteSpace(nextSceneName) &&
               Application.CanStreamedLevelBeLoaded(nextSceneName);
    }

    private void LoadNextScene()
    {
        SceneTransitionFade.FadeOutAndLoad(nextSceneName);
    }

    public bool TryGetInteractionPrompt(out string prompt)
    {
        prompt = portalReady && !transitionRequested ? "E - Portaldan Geç" : null;
        return prompt != null;
    }

    private void ResolveReferences()
    {
        portalCollider ??= GetComponent<Collider2D>();
        levelManager ??= FindFirstObjectByType<EarthquakeLevelManager>();
        if (portalCollider != null)
        {
            portalCollider.isTrigger = true;
            portalCollider.usedByEffector = false;
        }

        gameObject.tag = "interactable";

        Interactable interactable = GetComponent<Interactable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<Interactable>();

        // Awake may be reached once through AddComponent and again after a
        // script reload. Re-registering the same callback would make E invoke
        // the portal transition more than once.
        interactable.onInteract?.RemoveListener(EnterPortal);
        interactable.AddListener(EnterPortal);
    }

    private void SetInteractionEnabled(bool isEnabled)
    {
        if (portalCollider != null)
            portalCollider.enabled = isEnabled;
    }
}
