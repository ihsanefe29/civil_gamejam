using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EarthquakeShelter : MonoBehaviour, IInteractionPromptSource
{
    private const string EarthquakeCrouchTrigger = "earthquake_event";

    [SerializeField] private EarthquakeLevelManager earthquakeLevelManager;
    [SerializeField] private UnityEvent onSheltered;
    [SerializeField, Min(0f)] private float hideBelowPlayerOffset = 1f;
    [Header("Animator Triggers")]
    [SerializeField] private string normalTrigger = "normal";

    private bool sheltered;
    private Animator playerAnimator;
    private PlayerController playerController;
    private Transform playerTransform;
    private Rigidbody2D playerBody;
    private PlayerFootAlignment playerFootAlignment;
    private Vector3 originalPlayerPosition;
    private bool hasOriginalPlayerPosition;
    private Coroutine crouchHoldRoutine;

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
        if (playerAnimator != null)
            playerAnimator.speed = 1f;
        TriggerPlayerAnimation(EarthquakeCrouchTrigger);
        crouchHoldRoutine = StartCoroutine(PlayCrouchOnceAndHoldFinalFrame());
        playerController?.SetMovementLocked(true);
        earthquakeLevelManager.RegisterShelter();
        onSheltered?.Invoke();
    }

    private void Update()
    {
        if (!sheltered || earthquakeLevelManager == null ||
            earthquakeLevelManager.CurrentPhase == EarthquakePhase.Earthquake)
            return;

        sheltered = false;
        ReleaseCrouchFrame();
        RestorePlayerAfterShelter();
        TriggerPlayerAnimation(normalTrigger);
    }

    private void OnDisable()
    {
        ReleaseCrouchFrame();
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
            playerTransform = playerController.transform;

            // The Animator may be on a child object of the player prefab.
            // Search the complete player hierarchy before using a fallback.
            if (playerAnimator == null)
                playerAnimator = playerController.GetComponentInChildren<Animator>(true);
        }

        if (playerAnimator == null)
            playerAnimator = FindFirstObjectByType<Animator>();
    }

    private IEnumerator PlayCrouchOnceAndHoldFinalFrame()
    {
        int crouchStateHash = 0;
        float crouchLength = 0f;

        // Wait for the Animator transition to enter the crouch state, then
        // use that state's real clip duration instead of a hard-coded delay.
        for (int frame = 0; frame < 120; frame++)
        {
            if (!sheltered || playerAnimator == null)
                yield break;

            bool isTransitioning = playerAnimator.IsInTransition(0);
            AnimatorStateInfo stateInfo = isTransitioning
                ? playerAnimator.GetNextAnimatorStateInfo(0)
                : playerAnimator.GetCurrentAnimatorStateInfo(0);
            AnimatorClipInfo[] clips = isTransitioning
                ? playerAnimator.GetNextAnimatorClipInfo(0)
                : playerAnimator.GetCurrentAnimatorClipInfo(0);

            foreach (AnimatorClipInfo clipInfo in clips)
            {
                if (clipInfo.clip == null || !clipInfo.clip.name.Contains("Crouch"))
                    continue;

                crouchStateHash = stateInfo.fullPathHash;
                crouchLength = clipInfo.clip.length;
                break;
            }

            if (crouchStateHash != 0)
                break;

            yield return null;
        }

        if (crouchStateHash == 0)
            yield break;

        yield return new WaitForSeconds(crouchLength);
        if (!sheltered || earthquakeLevelManager == null ||
            earthquakeLevelManager.CurrentPhase != EarthquakePhase.Earthquake)
            yield break;

        // Imported Aseprite clips loop by default. Force the actual crouch
        // state to its final frame, even if its controller had an automatic
        // return transition, then freeze that pose until Aftermath.
        playerAnimator.Play(crouchStateHash, 0, 0.999f);
        playerAnimator.Update(0f);
        playerAnimator.speed = 0f;
        crouchHoldRoutine = null;
    }

    private void ReleaseCrouchFrame()
    {
        if (crouchHoldRoutine != null)
        {
            StopCoroutine(crouchHoldRoutine);
            crouchHoldRoutine = null;
        }

        if (playerAnimator != null)
            playerAnimator.speed = 1f;

    }

    private void PlacePlayerUnderTable()
    {
        ResolvePlayerReferences();

        if (playerTransform == null) return;

        Transform player = playerTransform;

        // Keep the player visibly in front of the furniture and close to its
        // base. The crouch pose makes this read as hiding under the table,
        // without sending the sprite behind the table or the background.
        if (!hasOriginalPlayerPosition)
        {
            originalPlayerPosition = player.position;
            hasOriginalPlayerPosition = true;
        }

        Vector3 underTable = player.position;
        playerFootAlignment ??= player.GetComponent<PlayerFootAlignment>();

        // Use the room's actual ground collider at the table's X coordinate.
        // This prevents the crouch sprite from appearing on a tabletop when
        // different furniture assets have different pivots/heights.
        if (playerFootAlignment == null ||
            !playerFootAlignment.TryGetGroundAlignedPosition(transform.position.x, out underTable))
        {
            underTable.x = transform.position.x;
            underTable.y -= hideBelowPlayerOffset;
        }

        player.position = underTable;

        playerBody = playerController != null
            ? playerController.GetComponent<Rigidbody2D>()
            : player.GetComponent<Rigidbody2D>();
        if (playerBody != null)
        {
            playerBody.position = underTable;
            playerBody.linearVelocity = Vector2.zero;
        }
    }

    private void RestorePlayerAfterShelter()
    {
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        playerController?.SetMovementLocked(false);

        if (playerTransform != null && hasOriginalPlayerPosition)
        {
            playerTransform.position = originalPlayerPosition;
            if (playerBody != null)
            {
                playerBody.position = originalPlayerPosition;
                playerBody.linearVelocity = Vector2.zero;
            }
        }

        hasOriginalPlayerPosition = false;
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
