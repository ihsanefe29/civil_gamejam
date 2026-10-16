using UnityEngine;

public class WaterStation : MonoBehaviour
{
    [SerializeField] private ShootEm waterPlayer;
    [SerializeField] private Transform player;

    [SerializeField] private float interactionDistance = 1.5f;
    [SerializeField] private float refillTime = 30f;

    private float nextAvailableTime = 0f;

    private void Update()
    {
        if (Time.time < nextAvailableTime)
            return;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance <= interactionDistance &&
            Input.GetKeyDown(KeyCode.E))
        {
            if (waterPlayer.GiveWater())
            {
                nextAvailableTime = Time.time + refillTime;
                Debug.Log("Water station empty! Refilling...");
            }
        }
    }
}