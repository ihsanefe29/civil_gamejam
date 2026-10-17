using UnityEngine;

public class EarthquakeLevelCompleter : MonoBehaviour
{
    [SerializeField] private EarthquakeLevelManager earthquakeLevelManager;

    public void CompleteLevel()
    {
        if (earthquakeLevelManager == null)
            earthquakeLevelManager = FindFirstObjectByType<EarthquakeLevelManager>();

        earthquakeLevelManager?.CompleteLevel();
    }
}
