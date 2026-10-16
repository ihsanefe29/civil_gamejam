using UnityEngine;

public class EarthquakeLevelCompleter : MonoBehaviour
{
    [SerializeField] private EarthquakeLevelManager earthquakeLevelManager;

    public void CompleteLevel(Inventory inventory)
    {
        earthquakeLevelManager?.CompleteLevel();
    }
}
