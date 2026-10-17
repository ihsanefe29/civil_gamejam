using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public enum FirePhase
{
    Preparation,
    Fire,
    Completed
}

public class FireSceneManager : MonoBehaviour
{
    [Header("Preparation")]
    [SerializeField, Min(1f)]
    private float preparationDuration = 60f;

    [Header("Interactables")]
    [SerializeField]
    private List<GameObject> preparationInteractables = new();

    [SerializeField]
    private List<GameObject> fireInteractables = new();

    [Header("Faulty Locations")]
    [SerializeField]
    private List<GameObject> faultyLocations = new();

    [Header("Enemy References")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject player_obj;
    private Transform transformPlayer;
    [Header("Map Spawn Points")]
    [SerializeField] private Transform leftSpawnPoint;
    [SerializeField] private Transform rightSpawnPoint;

    [Header("Spawn Settings")]
    [SerializeField] private float minSpawnInterval = 3f;
    [SerializeField] private float maxSpawnInterval = 5f;

    [SerializeField] private bool spawnFromFaults = true;
    [SerializeField] private bool spawnFromBoundaries = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onFireStarted;
    [SerializeField] private UnityEvent onLevelCompleted;

    [Header("Scoring")]
    [SerializeField] private int pointsPerGlass = 10;
    [SerializeField] private int pointsPerEmber = 20;

    [Header("Player Health")]
    [SerializeField] private int health = 3;

    

    public void DamagePlayer()
    {
        if (CurrentPhase == FirePhase.Completed)
            return;

        health--;

        Debug.Log("Player Health: " + health);

        if (health <= 0)
        {
            Debug.Log("Player Died!");
            CompleteLevel();
            Destroy(player_obj);
        }
    }

    public int TotalScore { get; private set; }

    public void AddScore(int points)
    {
        if (points <= 0) return;

        TotalScore += points;

        Debug.Log($"Score: +{points} | Total: {TotalScore}");
    }

    public void ScoreGlass()
    {
        AddScore(pointsPerGlass);
    }

    public void ScoreEmber()
    {
        AddScore(pointsPerEmber);
    }

    public FirePhase CurrentPhase { get; private set; }
        = FirePhase.Preparation;

    public float RemainingPreparationTime { get; private set; }

    private void Awake()
    {
        RemainingPreparationTime = preparationDuration;
    }

    private void Start()
    {
        transformPlayer = player_obj.transform;
        // Preparation phase interactions
        SetInteractables(preparationInteractables, true);

        // Fire phase interactions disabled
        SetInteractables(fireInteractables, false);
    }

    private void Update()
    {
        if (CurrentPhase == FirePhase.Preparation)
        {
            RemainingPreparationTime = Mathf.Max(
                0f,
                RemainingPreparationTime - Time.deltaTime
            );

            if (RemainingPreparationTime <= 0f)
            {
                BeginFire();
            }
        }

        // Manual enemy spawn for testing
        if (CurrentPhase == FirePhase.Fire &&
            Input.GetKeyDown(KeyCode.Q))
        {
            SpawnEnemy(transform.position);
        }
    }

    // --------------------------------
    // PHASE MANAGEMENT
    // --------------------------------

    public void BeginFire()
    {
        if (CurrentPhase != FirePhase.Preparation)
            return;

        CurrentPhase = FirePhase.Fire;

        // Disable preparation interactions
        SetInteractables(preparationInteractables, false);

        // Enable fire interactions
        SetInteractables(fireInteractables, true);

        // Start enemy spawning
        if (spawnFromFaults)
            StartCoroutine(FaultSpawnRoutine());

        if (spawnFromBoundaries)
            StartCoroutine(BoundarySpawnRoutine());

        onFireStarted?.Invoke();

        Debug.Log("Fire phase started!");
    }

    public void CompleteLevel()
    {
        if (CurrentPhase != FirePhase.Fire)
            return;

        CurrentPhase = FirePhase.Completed;

        StopAllCoroutines();

        SetInteractables(preparationInteractables, false);
        SetInteractables(fireInteractables, false);

        onLevelCompleted?.Invoke();

        Debug.Log($"Fire level completed!\n TotalScore:{TotalScore}");
    }

    // --------------------------------
    // INTERACTABLE MANAGEMENT
    // --------------------------------

    private void SetInteractables(
        List<GameObject> objects,
        bool canInteract)
    {
        foreach (GameObject obj in objects)
        {
            if (obj == null)
                continue;

            obj.tag = canInteract
                ? "interactable"
                : "Untagged";
        }
    }

    // --------------------------------
    // FAULT MANAGEMENT
    // --------------------------------

    public void DeleteFromList(GameObject obj)
    {
        if (faultyLocations.Remove(obj))
        {
            Debug.Log($"Fault resolved: {obj.name}");
        }
    }

    public List<GameObject> GetFaultyLocations()
    {
        return new List<GameObject>(faultyLocations);
    }

    // --------------------------------
    // ENEMY SPAWNING
    // --------------------------------

    private IEnumerator FaultSpawnRoutine()
    {
        while (CurrentPhase == FirePhase.Fire)
        {
            yield return new WaitForSeconds(
                GetRandomInterval()
            );

            if (CurrentPhase != FirePhase.Fire)
                yield break;

            List<GameObject> availableFaults =
                faultyLocations.FindAll(
                    fault => fault != null
                );

            if (availableFaults.Count == 0)
                continue;

            int randomIndex = Random.Range(
                0,
                availableFaults.Count
            );

            SpawnEnemy(
                availableFaults[randomIndex].transform.position
            );
        }
    }

    private IEnumerator BoundarySpawnRoutine()
    {
        while (CurrentPhase == FirePhase.Fire)
        {
            yield return new WaitForSeconds(
                GetRandomInterval()
            );

            if (CurrentPhase != FirePhase.Fire)
                yield break;

            Transform spawnPoint = Random.value < 0.5f
                ? leftSpawnPoint
                : rightSpawnPoint;

            if (spawnPoint != null)
            {
                SpawnEnemy(spawnPoint.position);
            }
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
        if (enemyPrefab == null)
            return;

        GameObject enemy = Instantiate(
            enemyPrefab,
            position,
            Quaternion.Euler(0f, 0f, 90f)
        );

        EnemyAI follow = enemy.GetComponent<EnemyAI>();

        if (follow != null)
        {
            follow.SetTarget(transformPlayer);
        }
    }
}