using UnityEngine;
using UnityEngine.Events;

public enum EarthquakePhase
{
    Preparation,
    Earthquake,
    Completed
}

public class EarthquakeLevelManager : MonoBehaviour
{
    [Header("Preparation")]
    [SerializeField, Min(1f)] private float preparationDuration = 60f;

    [Header("Scoring")]
    [SerializeField] private EmergencyBag emergencyBag;
    [SerializeField, Min(0)] private int pointsPerBagItem = 10;

    [Header("Earthquake")]
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private UnityEvent onEarthquakeStarted;
    [SerializeField] private UnityEvent onLevelCompleted;

    public EarthquakePhase CurrentPhase { get; private set; } = EarthquakePhase.Preparation;
    public float RemainingPreparationTime { get; private set; }
    public int TaskScore { get; private set; }
    public int TotalScore => CalculateTotalScore();
    public bool IsPlayerSheltered { get; private set; }
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
    }

    private void Update()
    {
        if (CurrentPhase != EarthquakePhase.Preparation)
            return;

        RemainingPreparationTime = Mathf.Max(0f, RemainingPreparationTime - Time.deltaTime);

        if (RemainingPreparationTime <= 0f)
            BeginEarthquake();
    }

    public void BeginEarthquake()
    {
        if (CurrentPhase != EarthquakePhase.Preparation)
            return;

        CurrentPhase = EarthquakePhase.Earthquake;
        cameraShake?.Shake();
        onEarthquakeStarted?.Invoke();
    }

    public void CompleteLevel()
    {
        if (CurrentPhase != EarthquakePhase.Earthquake)
            return;

        CurrentPhase = EarthquakePhase.Completed;
        PrintTotalScore();
        onLevelCompleted?.Invoke();
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
