using System.Collections;
using UnityEngine;

/// <summary>
/// Keeps the physical capsule and ground probe aligned with the visible feet
/// of the imported Bergamot sprite. The Aseprite walking frames use a pivot
/// above the feet, so the default centred capsule made the sprite float.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(SpriteRenderer))]
public class PlayerFootAlignment : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer = 1 << 3;
    [SerializeField, Min(0f)] private float groundCheckBelowFeet = 0.06f;
    [SerializeField, Min(1f)] private float groundRayDistance = 20f;

    private Rigidbody2D body;
    private CapsuleCollider2D capsule;
    private SpriteRenderer spriteRenderer;
    private Transform groundCheck;
    private float footOffsetFromRoot;
    private float footOffsetLocal;
    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[1];

    private IEnumerator Start()
    {
        // Wait for the Animator's initial sprite so alignment uses the same
        // frame that the player sees on entering the level.
        yield return null;

        body = GetComponent<Rigidbody2D>();
        capsule = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        groundCheck = transform.Find("GroundCheck");

        RefreshFootOffset();
        AlignColliderAndGroundCheck();
        SnapToGround(transform.position.x);
    }

    public bool TryGetGroundAlignedPosition(float x, out Vector3 alignedPosition)
    {
        alignedPosition = transform.position;
        RefreshFootOffset();

        Vector2 origin = new(x, transform.position.y + 5f);
        // Table, cabinet and interaction colliders are triggers. They can be
        // on the same layer as the room floor, but must never be treated as
        // ground or the player would be snapped onto their sprite.
        ContactFilter2D groundFilter = new();
        groundFilter.SetLayerMask(groundLayer);
        groundFilter.useTriggers = false;

        int hitCount = Physics2D.Raycast(
            origin,
            Vector2.down,
            groundFilter,
            groundHits,
            groundRayDistance);

        if (hitCount == 0)
            return false;

        alignedPosition.x = x;
        alignedPosition.y = groundHits[0].point.y - footOffsetFromRoot;
        return true;
    }

    private void SnapToGround(float x)
    {
        if (!TryGetGroundAlignedPosition(x, out Vector3 alignedPosition))
            return;

        transform.position = alignedPosition;
        if (body != null)
        {
            body.position = alignedPosition;
            body.linearVelocity = Vector2.zero;
        }
    }

    private void RefreshFootOffset()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        // World-space bounds remain correct even when the player is flipped.
        footOffsetFromRoot = spriteRenderer.bounds.min.y - transform.position.y;
        footOffsetLocal = transform.InverseTransformPoint(
            new Vector3(transform.position.x, spriteRenderer.bounds.min.y, transform.position.z)).y;
    }

    private void AlignColliderAndGroundCheck()
    {
        if (capsule == null)
            return;

        capsule.offset = new Vector2(
            capsule.offset.x,
            footOffsetLocal + capsule.size.y * 0.5f);

        if (groundCheck != null)
        {
            Vector3 position = groundCheck.localPosition;
            position.y = footOffsetLocal - groundCheckBelowFeet;
            groundCheck.localPosition = position;
        }

        Physics2D.SyncTransforms();
    }
}
