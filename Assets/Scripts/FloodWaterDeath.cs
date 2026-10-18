using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FloodWaterDeath : MonoBehaviour
{
    [SerializeField] private Rigidbody2D playerBody;
    [SerializeField] private FloodSceneManager sceneManager;

    private bool dangerous;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;

        if (sceneManager == null)
            sceneManager = FindFirstObjectByType<FloodSceneManager>();
    }

    public void SetDangerous(bool value)
    {
        dangerous = value;
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
        if (!dangerous ||
            playerBody == null ||
            sceneManager == null ||
            sceneManager.IsDead)
        {
            return;
        }

        if (other.attachedRigidbody != playerBody)
            return;

        dangerous = false;
        sceneManager.KillPlayer("Suya düþtün!");
    }
}