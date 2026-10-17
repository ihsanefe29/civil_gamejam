using Unity.VisualScripting;
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

    [SerializeField] private LevelTaskManager taskManager;

    [SerializeField] private Animator anim;

    public bool hasWater = false;
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
        Debug.Log($"BEFORE GiveWater: {hasWater} | Object: {gameObject.name} | ID: {GetInstanceID()}");

        if (hasWater)
        {
            Debug.LogWarning("Already has water!");
            return false;
        }

        hasWater = true;

        Debug.Log($"AFTER GiveWater: {hasWater}");
        return true;
    }

    private void ShootWater()
    {
        anim.SetTrigger("throwEvent");
        hasWater = false;

        Vector2 center = (Vector2)player.position +
            new Vector2(
                lastDirection * attackRange / 2f,
                0f
            );

        Collider2D[] targets = Physics2D.OverlapBoxAll(
            center,
            new Vector2(attackRange, attackHeight),
            0f,
            enemyLayer
        );
        int killedEnemies = 0;

        foreach (Collider2D target in targets)
        {
            // Check if target is an Ember
            if (target.TryGetComponent<EmberFault>(out EmberFault ember))
            {
                ember.Extinguish();
                continue;
            }

            // Otherwise check if target is an Enemy
            EnemyAI enemy = target.GetComponentInParent<EnemyAI>();

            if (enemy != null)
            {
                killedEnemies++;
                Destroy(enemy.gameObject);
            }
        }
        taskManager?.WaterShotResolved(killedEnemies);

        // Spawn water animation
        if (waterEffectPrefab != null)
        {
            Vector3 effectPosition = player.position +
                new Vector3(
                    lastDirection * attackRange / 2f,
                    0f,
                    0f
                );

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