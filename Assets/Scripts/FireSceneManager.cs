using UnityEngine;

public class FireSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform player;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        GameObject enemy = Instantiate(
            enemyPrefab,
            transform.position,
            Quaternion.Euler(0f, 0f, 90f)
        );

        EnemyAI follow = enemy.GetComponent<EnemyAI>();

        if (follow != null)
        {
            follow.SetTarget(player);
        }
    }
}
