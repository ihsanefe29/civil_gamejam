using System;
using UnityEngine;

[DisallowMultipleComponent]
public class FridgeMountTask : MonoBehaviour
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
        CreateMountedLabel();
        Completed?.Invoke();
    }

    private bool IsPreparationPhase()
    {
        earthquakeLevelManager ??= FindFirstObjectByType<EarthquakeLevelManager>();
        return earthquakeLevelManager == null || earthquakeLevelManager.CurrentPhase == EarthquakePhase.Preparation;
    }

    private void CreateMountedLabel()
    {
        if (transform.Find("Fridge Secured Label") != null) return;

        GameObject labelObject = new("Fridge Secured Label");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.text = "✓ SABİTLENDİ";
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 28;
        label.characterSize = 0.012f;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = new Color(0.45f, 1f, 0.55f);
        label.GetComponent<MeshRenderer>().sortingOrder = 100;
    }
}
