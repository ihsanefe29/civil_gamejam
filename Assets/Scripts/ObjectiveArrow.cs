using UnityEngine;

public class ObjectiveArrow : MonoBehaviour
{
    [Header("Position")]
    [SerializeField] private float arrowHeight = 1.5f;

    [SerializeField, Range(0.01f, 0.25f)]
    private float screenMargin = 0.07f;

    [Header("Smoothing")]
    [SerializeField] private float moveSmoothTime = 0.12f;
    [SerializeField] private float rotationSpeed = 500f;

    private Transform target;
    private Camera mainCamera;

    private Vector3 velocity;
    private bool initialized;

    public void Initialize(Transform newTarget, Camera camera)
    {
        target = newTarget;
        mainCamera = camera;

        initialized = true;

        UpdateArrow(true);
    }

    private void LateUpdate()
    {
        if (!initialized)
            return;

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        UpdateArrow(false);
    }

    private void UpdateArrow(bool instant)
    {
        if (target == null || mainCamera == null)
            return;

        Vector3 targetPosition =
            target.position + Vector3.up * arrowHeight;

        Vector3 viewport =
            mainCamera.WorldToViewportPoint(targetPosition);

        bool visible =
            viewport.z > 0f &&
            viewport.x >= screenMargin &&
            viewport.x <= 1f - screenMargin &&
            viewport.y >= screenMargin &&
            viewport.y <= 1f - screenMargin;

        float x;
        float y;
        float rotation;

        if (visible)
        {
            x = viewport.x;
            y = viewport.y;

            // Arrow points DOWN
            rotation = 0f;
        }
        else
        {
            bool right = viewport.x >= 0.5f;

            x = right
                ? 1f - screenMargin
                : screenMargin;

            y = Mathf.Clamp(
                viewport.y,
                screenMargin,
                1f - screenMargin
            );

            // Arrow points horizontally
            rotation = right ? 90f : -90f;
        }

        float depth = Mathf.Abs(
            mainCamera.transform.position.z -
            target.position.z
        );

        Vector3 desiredPosition =
            mainCamera.ViewportToWorldPoint(
                new Vector3(x, y, depth)
            );

        desiredPosition.z = target.position.z;

        if (instant)
        {
            transform.position = desiredPosition;
            transform.rotation =
                Quaternion.Euler(0f, 0f, rotation);

            velocity = Vector3.zero;
            return;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            moveSmoothTime
        );

        float newAngle = Mathf.MoveTowardsAngle(
            transform.eulerAngles.z,
            rotation,
            rotationSpeed * Time.deltaTime
        );

        transform.rotation =
            Quaternion.Euler(0f, 0f, newAngle);
    }
}