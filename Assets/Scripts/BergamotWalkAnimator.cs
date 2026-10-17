using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BergamotWalkAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] walkFrames;
    [SerializeField, Min(1f)] private float framesPerSecond = 10f;
    [SerializeField] private Rigidbody2D targetRigidbody;
    private SpriteRenderer spriteRenderer;
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
    }

    private void Update()
    {
        if (walkFrames == null || walkFrames.Length == 0) return;
        if (targetRigidbody == null || Mathf.Abs(targetRigidbody.linearVelocity.x) < 0.05f)
        {
            spriteRenderer.sprite = walkFrames[0];
            elapsed = 0f;
            frameIndex = 0;
            return;
        }
        elapsed += Time.deltaTime;
        if (elapsed < 1f / framesPerSecond) return;
        elapsed = 0f;
        frameIndex = (frameIndex + 1) % walkFrames.Length;
        spriteRenderer.sprite = walkFrames[frameIndex];
    }
}
