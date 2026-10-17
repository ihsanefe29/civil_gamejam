using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BergamotWalkAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] walkFrames;
    [SerializeField, Min(1f)] private float framesPerSecond = 10f;
    [SerializeField] private Rigidbody2D targetRigidbody;
    private SpriteRenderer spriteRenderer;
    private Animator animatorController;
    private float elapsed;
    private int frameIndex;

    public void Configure(Sprite[] frames)
    {
        walkFrames = frames;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (targetRigidbody == null) targetRigidbody = GetComponent<Rigidbody2D>();
        animatorController = GetComponent<Animator>();
    }

    private void Update()
    {
        // When this player has an Animator Controller, it is the single owner
        // of the sprite. Otherwise this legacy script would overwrite the
        // Crouch frame every Update even after the Animator changed state.
        if (animatorController != null && animatorController.enabled &&
            animatorController.runtimeAnimatorController != null)
            return;

        if (walkFrames == null || walkFrames.Length == 0) return;
        if (targetRigidbody == null || Mathf.Abs(targetRigidbody.linearVelocity.x) < 0.05f)
        {
            spriteRenderer.sprite = walkFrames[0];
            elapsed = 0f;
            frameIndex = 0;
            return;
        }
        Animate(walkFrames, framesPerSecond);
    }

    private void Animate(Sprite[] frames, float animationFramesPerSecond)
    {
        if (frames == null || frames.Length == 0) return;

        spriteRenderer.sprite = frames[frameIndex % frames.Length];
        elapsed += Time.deltaTime;
        if (elapsed < 1f / animationFramesPerSecond) return;
        elapsed = 0f;
        frameIndex = (frameIndex + 1) % frames.Length;
    }

}
