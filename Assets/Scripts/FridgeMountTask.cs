using System;
using UnityEngine;

[DisallowMultipleComponent]
public class FridgeMountTask : MonoBehaviour, IInteractionPromptSource
{
    [SerializeField, Min(0)] private int scoreOnCompletion = 50;
    [SerializeField] private EarthquakeLevelManager earthquakeLevelManager;

    public bool IsMounted { get; private set; }
    public event Action Completed;

    public void Configure(EarthquakeLevelManager manager)
    {
        earthquakeLevelManager = manager;
    }

    public void BeginMounting()
    {
        if (IsMounted || !IsPreparationPhase()) return;

        HammerNailMinigame minigame = FindFirstObjectByType<HammerNailMinigame>();
        if (minigame == null)
            minigame = new GameObject("Hammer Nail Minigame").AddComponent<HammerNailMinigame>();

        minigame.Show(this);
    }

    public void CompleteMounting()
    {
        if (IsMounted) return;

        IsMounted = true;
        earthquakeLevelManager ??= FindFirstObjectByType<EarthquakeLevelManager>();
        earthquakeLevelManager?.AddTaskScore(scoreOnCompletion);
        Completed?.Invoke();
    }

    private bool IsPreparationPhase()
    {
        earthquakeLevelManager ??= FindFirstObjectByType<EarthquakeLevelManager>();
        return earthquakeLevelManager == null || earthquakeLevelManager.CurrentPhase == EarthquakePhase.Preparation;
    }

    public bool TryGetInteractionPrompt(out string prompt)
    {
        bool canMount = !IsMounted && IsPreparationPhase();
        prompt = canMount ? "E - Secure" : null;
        return canMount;
    }
}
