
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float acceleration = 45f;
    [SerializeField] private float deceleration = 55f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private int maxJumpCount = 2;

    private Rigidbody2D rb;

    private float horizontalInput;

    private bool isGrounded;
    private bool wasGrounded;
    private bool facingRight = true;
    private bool movementLocked;

    public bool IsDead { get; private set; }

    private int jumpCount = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (movementLocked || IsDead)
        {
            StopWalkingSound();
            return;
        }

        // -------------------------
        // MOVEMENT INPUT
        // -------------------------

        horizontalInput = Input.GetAxisRaw("Horizontal");

        // -------------------------
        // GROUND CHECK
        // -------------------------

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // -------------------------
        // LANDING CHECK
        // -------------------------

        if (isGrounded && !wasGrounded)
        {
            jumpCount = 0;
        }

        // -------------------------
        // JUMP INPUT
        // -------------------------

        bool jumpPressed =
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.W);

        // -------------------------
        // JUMP
        // -------------------------

        if (jumpPressed && jumpCount < maxJumpCount)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            jumpCount++;
        }

        // -------------------------
        // WALKING SOUND
        // -------------------------

        bool isWalking =
            isGrounded &&
            Mathf.Abs(horizontalInput) > 0.01f &&
            jumpCount == 0 &&
            Mathf.Abs(rb.linearVelocity.x) > 0.05f;

        if (Level1AudioManager.Instance != null)
        {
            Level1AudioManager.Instance.SetWalking(isWalking);
        }

        // -------------------------
        // SAVE GROUND STATE
        // -------------------------

        wasGrounded = isGrounded;

        // -------------------------
        // FLIP CHARACTER
        // -------------------------

        FlipCharacter();
    }

    private void FixedUpdate()
    {
        if (movementLocked || IsDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // -------------------------
        // HORIZONTAL MOVEMENT
        // -------------------------

        float targetSpeed =
            horizontalInput * moveSpeed;

        float rate =
            Mathf.Abs(targetSpeed) > 0.01f
            ? acceleration
            : deceleration;

        rb.linearVelocity = new Vector2(
            Mathf.MoveTowards(
                rb.linearVelocity.x,
                targetSpeed,
                rate * Time.fixedDeltaTime
            ),
            rb.linearVelocity.y
        );
    }

    private void FlipCharacter()
    {
        if (
            (horizontalInput > 0 && !facingRight) ||
            (horizontalInput < 0 && facingRight)
        )
        {
            facingRight = !facingRight;

            Vector3 scale = transform.localScale;

            scale.x *= -1;

            transform.localScale = scale;
        }
    }

    // -------------------------
    // WALKING SOUND HELPER
    // -------------------------

    private void StopWalkingSound()
    {
        if (Level1AudioManager.Instance != null)
        {
            Level1AudioManager.Instance.SetWalking(false);
        }
    }

    public void SetMovementLocked(bool value)
    {
        if (IsDead) return;

        movementLocked = value;

        if (movementLocked)
        {
            rb.linearVelocity = Vector2.zero;
            StopWalkingSound();
        }
    }

    public void Die()
    {
        if (IsDead) return;

        IsDead = true;
        movementLocked = true;
        horizontalInput = 0f;
        rb.linearVelocity = Vector2.zero;

        StopWalkingSound();

        PlayerInteraction interaction = GetComponent<PlayerInteraction>();

        if (interaction != null)
            interaction.enabled = false;
    }

    private void OnDisable()
    {
        StopWalkingSound();
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}
