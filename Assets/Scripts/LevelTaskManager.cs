using System.Collections.Generic;
using UnityEngine;

public enum ObjectiveStage
{
    CollectGlass,
    GetWater,
    ExtinguishEmbers,
    DefeatEnemies,
    Completed
}

public class LevelTaskManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FireSceneManager fireSceneManager;
    [SerializeField] private Camera mainCamera;

    [Header("Preparation Targets")]
    [SerializeField] private List<GameObject> glassTargets = new();
    [SerializeField] private List<GameObject> emberTargets = new();

    [Header("Water Stations")]
    [SerializeField] private List<WaterStation> waterStations = new();

    [Header("Fire Task")]
    [SerializeField, Min(1)] private int requiredEnemyKills = 5;

    [Header("Arrow")]
    [SerializeField] private ObjectiveArrow arrowPrefab;

    private readonly Dictionary<Transform, ObjectiveArrow> arrows = new();
    private readonly HashSet<Transform> activeTargets = new();

    private bool hasWaterForTask = false;
    private int enemyKills = 0;

    private float nextRefreshTime;
    private const float refreshInterval = 0.15f;

    public ObjectiveStage CurrentStage
    {
        get
        {
            if (fireSceneManager == null)
                return ObjectiveStage.Completed;

            FirePhase phase = fireSceneManager.CurrentPhase;

            if (phase == FirePhase.Completed)
                return ObjectiveStage.Completed;

            if (phase == FirePhase.Preparation)
            {
                if (glassTargets.Count > 0)
                    return ObjectiveStage.CollectGlass;

                if (emberTargets.Count > 0)
                {
                    return hasWaterForTask
                        ? ObjectiveStage.ExtinguishEmbers
                        : ObjectiveStage.GetWater;
                }

                return ObjectiveStage.Completed;
            }

            if (enemyKills >= requiredEnemyKills)
                return ObjectiveStage.Completed;

            return hasWaterForTask
                ? ObjectiveStage.DefeatEnemies
                : ObjectiveStage.GetWater;
        }
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        RefreshArrows();
    }

    private void Update()
    {
        if (Time.time < nextRefreshTime)
            return;

        nextRefreshTime = Time.time + refreshInterval;

        RefreshArrows();
    }

    // --------------------------------
    // ARROW MANAGEMENT
    // --------------------------------

    private void RefreshArrows()
    {
        if (fireSceneManager == null ||
            mainCamera == null ||
            arrowPrefab == null)
            return;

        // Remove destroyed objects from lists
        glassTargets.RemoveAll(glass => glass == null);
        emberTargets.RemoveAll(ember => ember == null);

        // Start fire when all preparation tasks are completed
        if (fireSceneManager.CurrentPhase == FirePhase.Preparation &&
            glassTargets.Count == 0 &&
            emberTargets.Count == 0)
        {
            fireSceneManager.BeginFire();
        }

        activeTargets.Clear();

        // Find all targets for the current objective
        switch (CurrentStage)
        {
            case ObjectiveStage.CollectGlass:

                foreach (GameObject glass in glassTargets)
                {
                    if (glass != null)
                        AddTarget(glass.transform);
                }
                break;

            case ObjectiveStage.GetWater:

                // Show arrows on every water station
                // even if empty or refilling
                foreach (WaterStation station in waterStations)
                {
                    if (station != null)
                        AddTarget(station.transform);
                }
                break;

            case ObjectiveStage.ExtinguishEmbers:

                foreach (GameObject ember in emberTargets)
                {
                    if (ember != null)
                        AddTarget(ember.transform);
                }
                break;

            case ObjectiveStage.DefeatEnemies:

                EnemyAI[] enemies = FindObjectsByType<EnemyAI>(
                    FindObjectsSortMode.None
                );

                foreach (EnemyAI enemy in enemies)
                {
                    if (enemy != null)
                        AddTarget(enemy.transform);
                }
                break;
        }

        // Find arrows that are no longer needed
        List<Transform> toRemove = new();

        foreach (var pair in arrows)
        {
            if (pair.Key == null ||
                !activeTargets.Contains(pair.Key))
            {
                if (pair.Value != null)
                    Destroy(pair.Value.gameObject);

                toRemove.Add(pair.Key);
            }
        }

        // Remove old arrow references
        foreach (Transform target in toRemove)
        {
            arrows.Remove(target);
        }

        // Create arrows for new targets
        foreach (Transform target in activeTargets)
        {
            if (arrows.ContainsKey(target))
                continue;

            ObjectiveArrow arrow = Instantiate(arrowPrefab);

            arrow.Initialize(target, mainCamera);

            arrows.Add(target, arrow);
        }
    }
    private void AddTarget(Transform target)
    {
        if (target != null && target.gameObject.activeInHierarchy)
        {
            activeTargets.Add(target);
        }
    }

    // --------------------------------
    // GLASS / EMBER TASKS
    // --------------------------------

    public void CompleteTask(GameObject obj)
    {
        if (obj == null || fireSceneManager == null)
            return;

        // Preparation only
        if (fireSceneManager.CurrentPhase != FirePhase.Preparation)
            return;

        // Remove completed glass regardless of current stage
        if (glassTargets.Remove(obj))
        {
            Debug.Log("Glass collected: " + obj.name);
        }

        // Remove extinguished ember regardless of current stage
        if (emberTargets.Remove(obj))
        {
            Debug.Log("Ember extinguished: " + obj.name);
        }

        // Start fire if all preparation objectives are complete
        if (glassTargets.Count == 0 && emberTargets.Count == 0)
        {
            fireSceneManager.BeginFire();
        }

        RefreshArrows();
    }

    // --------------------------------
    // WATER COLLECTION
    // --------------------------------

    public void WaterCollected()
    {
        if (CurrentStage != ObjectiveStage.GetWater)
            return;

        hasWaterForTask = true;

        Debug.Log("Water collected! Next objective.");

        RefreshArrows();
    }

    // --------------------------------
    // WATER ATTACK
    // --------------------------------

    public void WaterShotResolved(int killedCount)
    {
        if (!hasWaterForTask || fireSceneManager == null)
            return;

        FirePhase phase = fireSceneManager.CurrentPhase;

        if (phase == FirePhase.Fire)
        {
            enemyKills += Mathf.Max(0, killedCount);

            Debug.Log(
                $"Enemies defeated: {enemyKills}/{requiredEnemyKills}"
            );
        }

        hasWaterForTask = false;

        // All preparation tasks completed
        if (phase == FirePhase.Preparation &&
            glassTargets.Count == 0 &&
            emberTargets.Count == 0)
        {
            fireSceneManager.BeginFire();
        }

        // Required enemies defeated
        else if (phase == FirePhase.Fire &&
                 enemyKills >= requiredEnemyKills)
        {
            fireSceneManager.CompleteLevel();
        }

        RefreshArrows();
    }
}