using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSceneManager : MonoBehaviour
{
    [Header("Enemy References")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform player;

    [Header("Faulty Locations")]
    [SerializeField] private List<GameObject> faultyLocations = new();

    [Header("Map Spawn Points")]
    [SerializeField] private Transform leftSpawnPoint;
    [SerializeField] private Transform rightSpawnPoint;

    [Header("Spawn Settings")]
    [SerializeField] private float minSpawnInterval = 3f;
    [SerializeField] private float maxSpawnInterval = 5f;

    [SerializeField] private bool spawnFromFaults = true;
    [SerializeField] private bool spawnFromBoundaries = true;

    private void Start()
    {
        if (spawnFromFaults)
            StartCoroutine(FaultSpawnRoutine());

        if (spawnFromBoundaries)
            StartCoroutine(BoundarySpawnRoutine());
    }

    private void Update()
    {
        // Manual spawn with Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SpawnEnemy(transform.position);
        }
    }

    // Called by GlassFault.cs etc.
    public void DeleteFromList(GameObject obj)
    {
        faultyLocations.Remove(obj);
    }

    public List<GameObject> GetFaultyLocations()
    {
        return faultyLocations;
    }

    private IEnumerator FaultSpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetRandomInterval());

            // Create a temporary list of valid faults
            List<GameObject> availableFaults =
                faultyLocations.FindAll(fault => fault != null);

            if (availableFaults.Count == 0)
                continue;

            int index = Random.Range(0, availableFaults.Count);

            SpawnEnemy(availableFaults[index].transform.position);
        }
    }

    private IEnumerator BoundarySpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetRandomInterval());

            Transform point = Random.value < 0.5f
                ? leftSpawnPoint
                : rightSpawnPoint;

            if (point != null)
                SpawnEnemy(point.position);
        }
    }

    private float GetRandomInterval()
    {
        float min = Mathf.Max(0.1f, minSpawnInterval);
        float max = Mathf.Max(min, maxSpawnInterval);


        return Random.Range(min, max);
    }

    private void SpawnEnemy(Vector3 position)
    {
        if (enemyPrefab == null) return;

        GameObject enemy = Instantiate(
            enemyPrefab,
            position,
            Quaternion.Euler(0f, 0f, 90f)
        );

        EnemyAI follow = enemy.GetComponent<EnemyAI>();

        if (follow != null)
        {
            follow.SetTarget(player);
        }
    }
}