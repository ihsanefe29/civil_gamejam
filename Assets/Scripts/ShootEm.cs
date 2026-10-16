using UnityEngine;

public class ShootEm : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject waterEffectPrefab;

    [Header("Attack")]
    [SerializeField] private float attackRange = 4f;
    [SerializeField] private float attackHeight = 2f;

    private bool hasWater = false;
    private float lastDirection = 1f;

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        if (horizontal > 0)
            lastDirection = 1f;
        else if (horizontal < 0)
            lastDirection = -1f;

        if (Input.GetMouseButtonDown(0) && hasWater)
        {
            ShootWater();
        }
    }

    public bool GiveWater()
    {
        if (hasWater) return false;

        hasWater = true;
        Debug.Log("Water collected!");
        return true;
    }

    private void ShootWater()
    {
        hasWater = false;

        Vector2 center = (Vector2)player.position +
            new Vector2(
                lastDirection * attackRange / 2f,
                0f
            );

        Collider2D[] enemies = Physics2D.OverlapBoxAll(
            center,
            new Vector2(attackRange, attackHeight),
            0f,
            enemyLayer
        );

        foreach (Collider2D enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }

        // Spawn water animation
        if (waterEffectPrefab != null)
        {
            Vector3 effectPosition = player.position +
                new Vector3(lastDirection * attackRange / 2f, 0f, 0f);

            GameObject effect = Instantiate(
                waterEffectPrefab,
                effectPosition,
                Quaternion.identity
            );

            Destroy(effect, 0.5f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.cyan;

        Vector3 center = player.position +
            new Vector3(lastDirection * attackRange / 2f, 0f, 0f);

        Gizmos.DrawWireCube(
            center,
            new Vector3(attackRange, attackHeight, 0f)
        );
    }
}
