using System.Collections.Generic;
using UnityEngine;

public enum ObjectiveStage
{
    CollectGlass,
    GetWater,
    ExtinguishEmbers,
    DefeatEnemies,
    Completed,
    EnterPortal
}

public class LevelTaskManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FireSceneManager fireSceneManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private ShootEm waterPlayer;

    [Header("Preparation Targets")]
    [SerializeField] private List<GameObject> glassTargets = new();
    [SerializeField] private List<GameObject> emberTargets = new();

    [Header("Water Stations")]
    [SerializeField] private List<WaterStation> waterStations = new();

    [Header("Fire Task")]
    [SerializeField, Min(1)] private int requiredEnemyKills = 5;

    [Header("Arrow")]
    [SerializeField] private ObjectiveArrow arrowPrefab;

    [Header("Exit Portal")]
    [SerializeField] private GameObject exitPortal;

    private readonly Dictionary<Transform, ObjectiveArrow> arrows = new();
    private readonly HashSet<Transform> activeTargets = new();

    private int enemyKills;

    private float nextRefreshTime;
    private const float refreshInterval = 0.15f;

    public bool HasWaterForTask =>
        waterPlayer != null && waterPlayer.hasWater;

    public int RemainingGlassCount => CountRemainingTargets(glassTargets);
    public int RemainingEmberCount => CountRemainingTargets(emberTargets);
    public int EnemyKillCount => enemyKills;
    public int RequiredEnemyKills => requiredEnemyKills;

    public ObjectiveStage CurrentStage
    {
        get
        {
            if (fireSceneManager == null)
                return ObjectiveStage.Completed;

            FirePhase phase = fireSceneManager.CurrentPhase;

            if (phase == FirePhase.Intro)
                return ObjectiveStage.Completed;

            if (phase == FirePhase.Completed)
            {
                return exitPortal != null && exitPortal.activeInHierarchy
                    ? ObjectiveStage.EnterPortal
                    : ObjectiveStage.Completed;
            }

            if (phase == FirePhase.Preparation)
            {
                if (RemainingGlassCount > 0)
                    return ObjectiveStage.CollectGlass;

                if (RemainingEmberCount > 0)
                {
                    return HasWaterForTask
                        ? ObjectiveStage.ExtinguishEmbers
                        : ObjectiveStage.GetWater;
                }

                return ObjectiveStage.Completed;
            }

            if (enemyKills >= requiredEnemyKills)
                return ObjectiveStage.Completed;

            return HasWaterForTask
                ? ObjectiveStage.DefeatEnemies
                : ObjectiveStage.GetWater;
        }
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (waterPlayer == null)
            waterPlayer = FindFirstObjectByType<ShootEm>();

        RefreshArrows();
    }

    private void Update()
    {
        if (Time.time < nextRefreshTime)
            return;

        nextRefreshTime = Time.time + refreshInterval;
        RefreshArrows();
    }

    private void RefreshArrows()
    {
        if (fireSceneManager == null)
            return;

        glassTargets.RemoveAll(glass => glass == null);
        emberTargets.RemoveAll(ember => ember == null);

        if (fireSceneManager.CurrentPhase == FirePhase.Preparation &&
            glassTargets.Count == 0 &&
            emberTargets.Count == 0)
        {
            fireSceneManager.BeginFire();
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null || arrowPrefab == null)
            return;

        activeTargets.Clear();

        switch (CurrentStage)
        {
            case ObjectiveStage.EnterPortal:
                if (exitPortal != null)
                    AddTarget(exitPortal.transform);
                break;
            case ObjectiveStage.CollectGlass:
                foreach (GameObject glass in glassTargets)
                {
                    if (glass != null)
                        AddTarget(glass.transform);
                }
                break;

            case ObjectiveStage.GetWater:
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

        foreach (Transform target in toRemove)
            arrows.Remove(target);

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
            activeTargets.Add(target);
    }

    public void CompleteTask(GameObject obj)
    {
        if (obj == null || fireSceneManager == null)
            return;

        if (fireSceneManager.CurrentPhase != FirePhase.Preparation)
            return;

        if (glassTargets.Remove(obj))
            Debug.Log("Glass collected: " + obj.name);

        if (emberTargets.Remove(obj))
            Debug.Log("Ember extinguished: " + obj.name);

        RefreshArrows();
    }

    public void WaterCollected()
    {
        // ShootEm.GiveWater() has already updated hasWater.
        // Refresh even if water was collected before its objective.
        RefreshArrows();
    }

    public void WaterShotResolved(int killedCount)
    {
        if (fireSceneManager == null)
            return;

        FirePhase phase = fireSceneManager.CurrentPhase;

        if (phase == FirePhase.Completed)
            return;

        // Do not check hasWater here:
        // the shot has already consumed the player's water.
        if (phase == FirePhase.Fire)
        {
            enemyKills += Mathf.Max(0, killedCount);

            Debug.Log(
                $"Enemies defeated: {enemyKills}/{requiredEnemyKills}"
            );

            if (enemyKills >= requiredEnemyKills)
                fireSceneManager.CompleteLevel();
        }

        // Read the current water state without overwriting it.
        RefreshArrows();
    }

    private static int CountRemainingTargets(List<GameObject> targets)
    {
        int count = 0;

        foreach (GameObject target in targets)
        {
            if (target != null)
                count++;
        }

        return count;
    }

    private void OnDestroy()
    {
        foreach (var pair in arrows)
        {
            if (pair.Value != null)
                Destroy(pair.Value.gameObject);
        }

        arrows.Clear();
    }
}