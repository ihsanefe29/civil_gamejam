using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 0.5f;

    private Rigidbody2D rb;
    private Transform player;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetTarget(Transform target)
    {
        player = target;
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        float distanceX = player.position.x - transform.position.x;

        if (Mathf.Abs(distanceX) > stopDistance)
        {
            float direction = Mathf.Sign(distanceX);

            rb.linearVelocity = new Vector2(
                direction * moveSpeed,
                rb.linearVelocity.y
            );
        }
        else
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            FireSceneManager manager = FindFirstObjectByType<FireSceneManager>();

            if (manager != null)
            {
                manager.DamagePlayer();
            }
        }
    }
}
