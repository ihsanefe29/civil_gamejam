using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public enum EarthquakePhase
{
    Preparation,
    Earthquake,
    Aftermath,
    Failed,
    Completed
}

public class EarthquakeLevelManager : MonoBehaviour
{
    [Header("Preparation")]
    [SerializeField, Min(1f)] private float preparationDuration = 60f;
    [SerializeField, Min(0f)] private float earlyStartNoticeDuration = 1.2f;

    [Header("Scoring")]
    [SerializeField] private EmergencyBag emergencyBag;
    [SerializeField, Min(0)] private int pointsPerBagItem = 10;

    [Header("Earthquake")]
    [SerializeField, Min(1f)] private float earthquakeDuration = 20f;
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private UnityEvent onEarthquakeStarted;
    [SerializeField] private UnityEvent onEarthquakeEnded;
    [SerializeField] private UnityEvent onPlayerDied;
    [SerializeField] private UnityEvent onLevelCompleted;

    public EarthquakePhase CurrentPhase { get; private set; } = EarthquakePhase.Preparation;
    public float RemainingPreparationTime { get; private set; }
    public float RemainingEarthquakeTime { get; private set; }
    public int TaskScore { get; private set; }
    public int TotalScore => CalculateTotalScore();
    public bool IsPlayerSheltered { get; private set; }
    public bool IsPlayerDead { get; private set; }
    public bool IsEarlyEarthquakeStarting { get; private set; }
    public EmergencyBag ActiveEmergencyBag => emergencyBag;

    private void Awake()
    {
        RemainingPreparationTime = preparationDuration;

        if (emergencyBag == null)
            emergencyBag = FindFirstObjectByType<EmergencyBag>();

        if (cameraShake == null)
            cameraShake = FindFirstObjectByType<CameraShake>();

        // Every earthquake level manager owns its own preparation countdown.
        if (GetComponent<EarthquakeCountdownUI>() == null)
            gameObject.AddComponent<EarthquakeCountdownUI>();

        if (GetComponent<InventoryHUD>() == null)
            gameObject.AddComponent<InventoryHUD>();

        if (GetComponent<EarthquakeTaskManager>() == null)
            gameObject.AddComponent<EarthquakeTaskManager>();

        if (GetComponent<EarlyEarthquakeNoticeUI>() == null)
            gameObject.AddComponent<EarlyEarthquakeNoticeUI>();
    }

    private void Update()
    {
        if (CurrentPhase == EarthquakePhase.Preparation)
        {
            // The notice is on-screen and the countdown is intentionally
            // paused while a task-completion-triggered earthquake starts.
            if (IsEarlyEarthquakeStarting)
                return;

            RemainingPreparationTime = Mathf.Max(0f, RemainingPreparationTime - Time.deltaTime);

            if (RemainingPreparationTime <= 0f)
                BeginEarthquake();
            return;
        }

        if (CurrentPhase != EarthquakePhase.Earthquake)
            return;

        RemainingEarthquakeTime = Mathf.Max(0f, RemainingEarthquakeTime - Time.deltaTime);
        if (RemainingEarthquakeTime <= 0f)
            EndEarthquake();
    }

    public void BeginEarthquake()
    {
        if (CurrentPhase != EarthquakePhase.Preparation)
            return;

        CurrentPhase = EarthquakePhase.Earthquake;
        IsEarlyEarthquakeStarting = false;
        RemainingEarthquakeTime = earthquakeDuration;
        cameraShake?.Shake();
        onEarthquakeStarted?.Invoke();
    }

    public bool BeginEarthquakeEarly()
    {
        if (CurrentPhase != EarthquakePhase.Preparation || IsEarlyEarthquakeStarting)
            return false;

        IsEarlyEarthquakeStarting = true;
        GetComponent<EarlyEarthquakeNoticeUI>()?.Show(earlyStartNoticeDuration);
        StartCoroutine(BeginEarthquakeAfterNotice());
        return true;
    }

    private IEnumerator BeginEarthquakeAfterNotice()
    {
        yield return new WaitForSeconds(earlyStartNoticeDuration);

        if (CurrentPhase == EarthquakePhase.Preparation)
            BeginEarthquake();
    }

    public void EndEarthquake()
    {
        if (CurrentPhase != EarthquakePhase.Earthquake)
            return;

        // Not reaching a table is a score consequence, not a game-over.
        // The player can still continue to the portal after the earthquake.
        if (!IsPlayerSheltered)
            Debug.Log("Earthquake ended without table shelter. No safe-shelter score was earned.");

        CurrentPhase = EarthquakePhase.Aftermath;
        onEarthquakeEnded?.Invoke();
    }

    public void CompleteLevel()
    {
        // The level can only be completed after the player survived the
        // earthquake and the post-earthquake gameplay is finished.
        if (CurrentPhase != EarthquakePhase.Aftermath)
            return;

        CurrentPhase = EarthquakePhase.Completed;
        PrintTotalScore();
        onLevelCompleted?.Invoke();
    }

    private void FailLevel()
    {
        CurrentPhase = EarthquakePhase.Failed;
        IsPlayerDead = true;
        FindFirstObjectByType<PlayerController>()?.Die();
        onPlayerDied?.Invoke();
        Debug.Log("Player was not under a table when the earthquake ended.");
    }

    public void AddTaskScore(int points)
    {
        TaskScore += Mathf.Max(0, points);
    }

    public void SetEmergencyBag(EmergencyBag bag)
    {
        emergencyBag = bag;
    }

    public void RegisterShelter()
    {
        IsPlayerSheltered = true;
        Debug.Log("Player is safely sheltered under the table.");
    }

    public int CalculateTotalScore()
    {
        if (emergencyBag == null)
            return TaskScore;

        return TaskScore + (emergencyBag.StoredItemCount * pointsPerBagItem);
    }

    public void PrintTotalScore()
    {
        Debug.Log($"Total Score: {TotalScore}");
    }
}
