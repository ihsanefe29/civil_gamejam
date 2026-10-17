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
        if (rb == null) return;

        rb.gravityScale = 0f;

        if (player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 targetPosition =
            (Vector2)player.position + Vector2.down * 0.4f;

        Vector2 toPlayer = targetPosition - rb.position;

        Vector2 desiredVelocity = toPlayer.magnitude > stopDistance
            ? toPlayer.normalized * moveSpeed
            : Vector2.zero;

        // Gradually change velocity instead of instantly turning.
        rb.linearVelocity = Vector2.MoveTowards(
            rb.linearVelocity,
            desiredVelocity,
            2f * Time.fixedDeltaTime
        );

        // Face the actual movement direction.
        Vector2 movement = rb.linearVelocity;

        if (movement.sqrMagnitude > 0.01f)
        {
            bool facingLeft = movement.x < 0f;

            float angle = Mathf.Atan2(
                movement.y,
                Mathf.Abs(movement.x)
            ) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(
                0f,
                facingLeft ? 180f : 0f,
                facingLeft ? -angle : angle
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
