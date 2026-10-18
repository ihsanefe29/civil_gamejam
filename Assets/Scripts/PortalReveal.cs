using System.Collections;
using UnityEngine;

public class PortalReveal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject portal;

    [Header("Temporarily Disable")]
    [Tooltip("Kameranýn oyuncuyu takip eden scripti")]
    [SerializeField] private Behaviour cameraFollow;

    [Tooltip("Oyuncunun hareket, yön, saldýrý ve etkileþim scriptleri")]
    [SerializeField] private MonoBehaviour[] playerControls;

    [Header("Camera")]
    [SerializeField] private Vector2 portalCameraOffset;
    [SerializeField] private float travelDuration = 1.2f;
    [SerializeField] private float holdDuration = 1.5f;

    [Header("Before Freeze")]
    [SerializeField] private float beforeFreezeDelay = 0.5f;

    private bool playing;
    private float previousTimeScale;
    private bool followWasEnabled;
    private bool[] controlStates;

    private Animator[] animators;
    private AnimatorUpdateMode[] animatorModes;

    private bool revealStarted;

    public void Play()
    {
        if (revealStarted || portal == null)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            portal.SetActive(true);
            return;
        }

        revealStarted = true;
        StartCoroutine(Reveal());
    }



    private IEnumerator Reveal()
    {
        // Allow ongoing animations to finish before freezing.
        yield return new WaitForSeconds(beforeFreezeDelay);

        playing = true;

        Vector3 startPosition = mainCamera.transform.position;

        Vector3 playerCameraOffset = player != null
            ? startPosition - player.position
            : Vector3.zero;

        // Disable camera tracking during the cinematic.
        followWasEnabled = cameraFollow != null && cameraFollow.enabled;

        if (cameraFollow != null)
            cameraFollow.enabled = false;

        // Time.timeScale does not stop Update or keyboard input.
        controlStates = new bool[playerControls.Length];

        for (int i = 0; i < playerControls.Length; i++)
        {
            MonoBehaviour control = playerControls[i];

            if (control == null || control == this)
                continue;

            controlStates[i] = control.enabled;
            control.enabled = false;
        }

        // Keep Animator animations running while gameplay is paused.
        animators = FindObjectsByType<Animator>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        animatorModes = new AnimatorUpdateMode[animators.Length];

        for (int i = 0; i < animators.Length; i++)
        {
            animatorModes[i] = animators[i].updateMode;
            animators[i].updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        Vector3 portalPosition = new Vector3(
            portal.transform.position.x + portalCameraOffset.x,
            portal.transform.position.y + portalCameraOffset.y,
            startPosition.z
        );

        // Move first, then show the opening animation.
        yield return MoveCamera(portalPosition);

        portal.SetActive(true);

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, holdDuration)
        );

        Vector3 returnPosition = player != null
            ? player.position + playerCameraOffset
            : startPosition;

        returnPosition.z = startPosition.z;

        yield return MoveCamera(returnPosition);

        RestoreState();
    }

    private IEnumerator MoveCamera(Vector3 destination)
    {
        Vector3 origin = mainCamera.transform.position;

        // Y ve Z sabit, sadece X deðiþir.
        destination.y = origin.y;
        destination.z = origin.z;

        float duration = Mathf.Max(0.01f, travelDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);

            mainCamera.transform.position =
                Vector3.Lerp(origin, destination, t);

            yield return null;
        }

        mainCamera.transform.position = destination;
    }

    private void RestoreState()
    {
        if (!playing)
            return;

        Time.timeScale = previousTimeScale;

        if (animators != null)
        {
            for (int i = 0; i < animators.Length; i++)
            {
                if (animators[i] != null)
                    animators[i].updateMode = animatorModes[i];
            }
        }

        if (controlStates != null)
        {
            for (int i = 0; i < playerControls.Length; i++)
            {
                if (playerControls[i] != null && playerControls[i] != this)
                    playerControls[i].enabled = controlStates[i];
            }
        }

        if (cameraFollow != null)
            cameraFollow.enabled = followWasEnabled;

        playing = false;
    }

    private void OnDisable()
    {
        if (!playing)
            return;

        StopAllCoroutines();
        RestoreState();
    }
}