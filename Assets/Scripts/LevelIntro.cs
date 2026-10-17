using System.Collections;
using UnityEngine;

public class LevelIntro : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FireSceneManager fireSceneManager;
    [SerializeField] private GameObject entrancePortal;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Rigidbody2D playerBody;

    [Header("Player Controls")]
    [Tooltip("Hareket, yön deðiþtirme, ShootEm ve etkileþim scriptleri")]
    [SerializeField] private MonoBehaviour[] playerControls;

    [Header("Timing")]
    [SerializeField] private float portalAnimationDuration = 1f;
    [SerializeField] private float delayAfterLanding = 0.3f;

    [Header("Landing")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField, Min(0.1f)] private float fallGravity = 3f;

    private readonly ContactPoint2D[] contacts = new ContactPoint2D[16];

    private bool[] previousControlStates;
    private float previousGravity;
    private RigidbodyConstraints2D previousConstraints;

    private bool ready;

    private void Awake()
    {
        if (fireSceneManager == null)
            fireSceneManager = GetComponent<FireSceneManager>();

        if (fireSceneManager == null ||
            entrancePortal == null ||
            spawnPoint == null ||
            playerBody == null)
        {
            Debug.LogError(
                $"LevelIntro eksik referans: " +
                $"Manager={fireSceneManager}, Portal={entrancePortal}, " +
                $"SpawnPoint={spawnPoint}, PlayerBody={playerBody}",
                this
            );

            enabled = false;
            return;
        }

        previousControlStates = new bool[playerControls.Length];

        for (int i = 0; i < playerControls.Length; i++)
        {
            MonoBehaviour control = playerControls[i];

            if (control == null)
                continue;

            previousControlStates[i] = control.enabled;
            control.enabled = false;
        }

        previousGravity = playerBody.gravityScale;
        previousConstraints = playerBody.constraints;

        playerBody.linearVelocity = Vector2.zero;
        playerBody.angularVelocity = 0f;
        playerBody.simulated = false;

        Vector3 spawnPosition = spawnPoint.position;
        spawnPosition.z = playerBody.transform.position.z;

        playerBody.transform.position = spawnPosition;
        playerBody.position = new Vector2(spawnPosition.x, spawnPosition.y);

        playerBody.gameObject.SetActive(false);
        entrancePortal.SetActive(false);

        ready = true;
    }

    private IEnumerator Start()
    {
        if (!ready)
            yield break;

        yield return new WaitForSeconds(1.5f);

        // Activate the portal and play its default animation.
        entrancePortal.SetActive(true);

        yield return new WaitForSeconds(portalAnimationDuration);

        // Reveal the player and allow them to fall.
        playerBody.position = spawnPoint.position;
        playerBody.gameObject.SetActive(true);

        // Keep input disabled even if an OnEnable method changed it.
        foreach (MonoBehaviour control in playerControls)
        {
            if (control != null)
                control.enabled = false;
        }

        playerBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerBody.gravityScale = fallGravity;
        playerBody.simulated = true;
        playerBody.linearVelocity = Vector2.zero;

        // Wait for a real ground contact.
        do
        {
            yield return new WaitForFixedUpdate();
        }
        while (!IsGrounded());

        yield return new WaitForSeconds(delayAfterLanding);

        playerBody.gravityScale = previousGravity;
        playerBody.constraints = previousConstraints;

        entrancePortal.GetComponent<Animator>().SetTrigger("close");

        yield return new WaitUntil(() => entrancePortal == null);

        fireSceneManager.BeginPreparation();

        for (int i = 0; i < playerControls.Length; i++)
        {
            if (playerControls[i] != null)
                playerControls[i].enabled = previousControlStates[i];
        }
    }

    private bool IsGrounded()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayers);
        filter.useTriggers = false;

        int count = playerBody.GetContacts(filter, contacts);

        for (int i = 0; i < count; i++)
        {
            // An upward normal means support from below.
            if (contacts[i].normal.y > 0.5f)
                return true;
        }

        return false;
    }
}