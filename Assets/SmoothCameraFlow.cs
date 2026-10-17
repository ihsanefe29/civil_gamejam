using UnityEngine;

public class SmoothCameraFlow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private float damping = 0.3f;

    public Transform target;

    private float velocityX = 0f;
    private float fixedY;
    private float fixedZ;

    private void Start()
    {
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float targetX = target.position.x + offset.x;

        float smoothX = Mathf.SmoothDamp(
            transform.position.x,
            targetX,
            ref velocityX,
            damping
        );

        transform.position = new Vector3(
            smoothX,
            fixedY,
            fixedZ
        );
    }
}