using UnityEngine;

public class ShootEm : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform player;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float spawnOffset = 0.7f;

    private float lastDirection = 1f;

    private void Update()
    {
        // Remember player's last horizontal direction
        float horizontal = Input.GetAxisRaw("Horizontal");

        if (horizontal > 0f)
            lastDirection = 1f;
        else if (horizontal < 0f)
            lastDirection = -1f;

        // Shoot on left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (player == null || projectilePrefab == null) return;

        Vector3 spawnPosition = player.position +
            new Vector3(lastDirection * spawnOffset, 0f, 0f);

        GameObject projectile = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                lastDirection * projectileSpeed,
                0f
            );
        }
    }
}
