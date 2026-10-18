using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FloodWaterDeath : MonoBehaviour
{
    private Collider2D waterCollider;
    private Rigidbody2D playerBody;
    private Collider2D[] playerColliders;
    private FloodSceneManager sceneManager;

    private bool dangerous = true;
    private bool deathRequested;

    private void Awake()
    {
        waterCollider = GetComponent<Collider2D>();
        waterCollider.isTrigger = true;

        sceneManager = FindFirstObjectByType<FloodSceneManager>();

        PlayerController player =
            FindFirstObjectByType<PlayerController>();

        if (player != null)
            SetPlayer(player.GetComponent<Rigidbody2D>());
    }

    public void Initialize(
        Rigidbody2D player,
        FloodSceneManager manager)
    {
        sceneManager = manager;
        SetPlayer(player);
        deathRequested = false;
    }

    private void SetPlayer(Rigidbody2D player)
    {
        playerBody = player;

        if (playerBody != null)
        {
            playerColliders =
                playerBody.GetComponentsInChildren<Collider2D>();
        }
    }

    public void SetDangerous(bool value)
    {
        dangerous = value;

        if (value)
            deathRequested = false;
    }

    private void FixedUpdate()
    {
        if (!CanCheck())
            return;

        foreach (Collider2D playerCollider in playerColliders)
        {
            if (playerCollider == null ||
                !playerCollider.enabled ||
                !playerCollider.gameObject.activeInHierarchy ||
                playerCollider.isTrigger)
            {
                continue;
            }

            ColliderDistance2D contact =
                waterCollider.Distance(playerCollider);

            if (contact.isValid && contact.isOverlapped)
            {
                KillPlayer();
                return;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckPlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        CheckPlayer(other);
    }

    private void CheckPlayer(Collider2D other)
    {
        if (!CanCheck())
            return;

        if (other.attachedRigidbody == playerBody)
            KillPlayer();
    }

    private bool CanCheck()
    {
        return dangerous &&
               !deathRequested &&
               waterCollider != null &&
               waterCollider.enabled &&
               playerBody != null &&
               playerBody.simulated &&
               playerColliders != null &&
               sceneManager != null &&
               !sceneManager.IsDead;
    }

    private void KillPlayer()
    {
        deathRequested = true;

        sceneManager.KillPlayer("Suya düþtün!");

        if (!sceneManager.IsDead)
        {
            Debug.LogError(
                "Su temasý algýlandý fakat " +
                "FloodSceneManager.KillPlayer ölümü baþlatmadý. " +
                "Console'daki ölüm UI hatasýný kontrol et.",
                this
            );
        }
    }
}