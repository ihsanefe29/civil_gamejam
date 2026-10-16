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

    private void Awake()
    {
        RemainingPreparationTime = preparationDuration;

        if (emergencyBag == null)
            emergencyBag = FindFirstObjectByType<EmergencyBag>();
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
