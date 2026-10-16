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

    private int jumpCount = 0;
    private Interactable interactable_Obj;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    private void Update()
    {
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

        // Önceki frame havadaydý
        // þimdi yerdeyse:
        // karakter yeni yere indi.

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
        // -------------------------
        // HORIZONTAL MOVEMENT
        // -------------------------

        float targetSpeed =
            horizontalInput * moveSpeed;


        float speedDifference =
            targetSpeed - rb.linearVelocity.x;


        float rate =
            Mathf.Abs(targetSpeed) > 0.01f
            ? acceleration
            : deceleration;


        float movement =
            speedDifference *
            rate *
            Time.fixedDeltaTime;


        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x + movement,
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

            Vector3 scale =
                transform.localScale;

            scale.x *= -1;

            transform.localScale = scale;
        }
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



    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (collision.gameObject.CompareTag("interactable"))
                collision.GetComponent<Interactable>().Interact();
        }
    }
}