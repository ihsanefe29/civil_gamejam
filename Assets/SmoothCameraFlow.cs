using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SmoothCameraFlow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private float damping = 0.3f;

    [Header("X Boundaries")]
    [SerializeField] private Transform leftBoundary;
    [SerializeField] private Transform rightBoundary;

    [Header("Y Boundaries")]
    [SerializeField] private Transform groundBoundary;
    [SerializeField] private Transform topBoundary;

    public Transform target;

    private Camera cam;
    private float velocityX;
    private float velocityY;
    private float fixedZ;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        fixedZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // Sol ve sað sýnýr atanmýþsa X hareketini kadraja göre sýnýrla.
        if (leftBoundary != null && rightBoundary != null)
        {
            float minCameraX = leftBoundary.position.x + halfWidth;
            float maxCameraX = rightBoundary.position.x - halfWidth;

            desiredPosition.x = ClampOrCenter(
                desiredPosition.x,
                minCameraX,
                maxCameraX
            );
        }

        // Ground ve top atanmýþsa Y hareketini kadraja göre sýnýrla.
        if (groundBoundary != null && topBoundary != null)
        {
            float minCameraY = groundBoundary.position.y + halfHeight;
            float maxCameraY = topBoundary.position.y - halfHeight;

            desiredPosition.y = ClampOrCenter(
                desiredPosition.y,
                minCameraY,
                maxCameraY
            );
        }
        else
        {
            // Y sýnýrlarý verilmediyse baþlangýç Y konumunu koru.
            desiredPosition.y = transform.position.y;
        }

        float smoothX = Mathf.SmoothDamp(
            transform.position.x,
            desiredPosition.x,
            ref velocityX,
            damping
        );

        float smoothY = Mathf.SmoothDamp(
            transform.position.y,
            desiredPosition.y,
            ref velocityY,
            damping
        );

        // Yumuþak takip sýrasýnda da sýnýrlarýn dýþýna çýkma.
        if (leftBoundary != null && rightBoundary != null)
        {
            float minCameraX = leftBoundary.position.x + halfWidth;
            float maxCameraX = rightBoundary.position.x - halfWidth;

            smoothX = ClampOrCenter(smoothX, minCameraX, maxCameraX);
        }

        if (groundBoundary != null && topBoundary != null)
        {
            float minCameraY = groundBoundary.position.y + halfHeight;
            float maxCameraY = topBoundary.position.y - halfHeight;

            smoothY = ClampOrCenter(smoothY, minCameraY, maxCameraY);
        }

        transform.position = new Vector3(smoothX, smoothY, fixedZ);
    }

    private float ClampOrCenter(float value, float min, float max)
    {
        // Sýnýrlar kameranýn kadrajýndan daha darsa kamerayý ortala.
        if (min > max)
            return (min + max) * 0.5f;

        return Mathf.Clamp(value, min, max);
    }
}