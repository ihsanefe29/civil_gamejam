using UnityEngine;

public class FloodCoin : MonoBehaviour
{
    private FloodTaskManager taskManager;

    // Mevcut Interactable event'inden çaðrýlýr.
    public void Collect()
    {
        if (taskManager == null)
            taskManager = FindFirstObjectByType<FloodTaskManager>();

        if (taskManager != null)
            taskManager.CompleteTask(gameObject);
    }
}