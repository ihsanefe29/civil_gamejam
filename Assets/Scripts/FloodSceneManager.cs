using UnityEngine;
using UnityEngine.Events;
using System.Collections;
public enum FloodPhase
{
    Intro,
    Preparation,
    Flood,
    Completed
}

public class FloodSceneManager : MonoBehaviour
{
    private bool endingFlood;

    [Header("References")]
    [SerializeField] private FloodTaskManager taskManager;
    [SerializeField] private FloodParkour parkour;

    [Header("Intro")]
    [SerializeField] private bool startWithIntro;

    [Header("Durations")]
    [SerializeField, Min(1f)] private float preparationDuration = 60f;
    [SerializeField, Min(1f)] private float floodDuration = 30f;

    [Header("Optional Events")]
    [SerializeField] private UnityEvent onPreparationStarted;
    [SerializeField] private UnityEvent onFloodStarted;
    [SerializeField] private UnityEvent onFloodEnded;
    [SerializeField] private UnityEvent onLevelCompleted;

    public FloodPhase CurrentPhase { get; private set; }
        = FloodPhase.Intro;

    public float RemainingPreparationTime { get; private set; }
    public float RemainingFloodTime { get; private set; }

    private void Awake()
    {
        Time.timeScale = 1f;

        if (taskManager == null)
            taskManager = GetComponent<FloodTaskManager>();

        if (parkour == null)
            parkour = GetComponent<FloodParkour>();

        RemainingPreparationTime = preparationDuration;
    }

    private void Start()
    {
        if (!startWithIntro)
            BeginPreparation();
    }

    private void Update()
    {
        if (IsDead || endingFlood)
            return;
        if (CurrentPhase == FloodPhase.Preparation)
        {
            RemainingPreparationTime = Mathf.Max(
                0f, RemainingPreparationTime - Time.deltaTime
            );

            if (RemainingPreparationTime <= 0f)
                BeginFlood();
        }
        else if (CurrentPhase == FloodPhase.Flood)
        {
            RemainingFloodTime = Mathf.Max(
                0f, RemainingFloodTime - Time.deltaTime
            );

            if (RemainingFloodTime <= 0f)
                EndFlood();
        }
    }

    public void BeginPreparation()
    {
        if (CurrentPhase != FloodPhase.Intro)
            return;

        StartPreparationCycle();
    }

    private void StartPreparationCycle()
    {
        CurrentPhase = FloodPhase.Preparation;
        RemainingPreparationTime = preparationDuration;
        RemainingFloodTime = 0f;

        taskManager.ResetPreparation();
        onPreparationStarted?.Invoke();
    }

    public void BeginFlood()
    {
        if (IsDead || CurrentPhase != FloodPhase.Preparation)
            return;

        if (taskManager.FishInteractionCount <
            taskManager.RequiredFishInteractions)
        {
            KillPlayer("Elektrik balýklarýnýn hepsini toplamadýn!");
            return;
        }

        CurrentPhase = FloodPhase.Flood;
        RemainingPreparationTime = 0f;
        RemainingFloodTime = floodDuration;

        taskManager.HandleFloodStarted();
        parkour.StartFlood();

        onFloodStarted?.Invoke();
    }

    public void EndFlood()
    {
        if (IsDead ||
            endingFlood ||
            CurrentPhase != FloodPhase.Flood)
        {
            return;
        }

        endingFlood = true;
        RemainingFloodTime = 0f;

        StartCoroutine(WaitForWaterToDisappear());
    }

    private IEnumerator WaitForWaterToDisappear()
    {
        parkour.FinishFlood();

        // Ýniþ animasyonu ve su prefabýnýn silinmesi beklenecek.
        yield return new WaitUntil(
            () => parkour == null || !parkour.IsWaterVisible
        );

        if (IsDead)
        {
            endingFlood = false;
            yield break;
        }

        onFloodEnded?.Invoke();

        if (taskManager.AllSpecialItemsCollected)
        {
            CurrentPhase = FloodPhase.Completed;

            taskManager.HandleLevelCompleted();

            Time.timeScale = 0f;
            onLevelCompleted?.Invoke();
        }
        else
        {
            CurrentPhase = FloodPhase.Intro;
            StartPreparationCycle();
        }

        endingFlood = false;
    }

    [SerializeField] private FloodDeathUI deathUI;

    public bool IsDead { get; private set; }

    public void KillPlayer(string reason)
    {
        if (IsDead || CurrentPhase == FloodPhase.Completed)
            return;

        if (deathUI == null)
            deathUI = GetComponent<FloodDeathUI>();

        if (deathUI == null)
        {
            Debug.LogError("FloodManagers objesine FloodDeathUI ekle.");
            return;
        }

        IsDead = true;
        deathUI.ShowDeath(reason);
    }
}