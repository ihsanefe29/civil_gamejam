using Unity.VisualScripting;
using UnityEngine;

public class WaterStation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShootEm waterPlayer;

    [Header("Settings")]
    [SerializeField] private float refillTime = 30f;

    [SerializeField] private LevelTaskManager taskManager;

    private float nextAvailableTime = 0f;
    private SpriteRenderer clr;
    private Color og_Color;

    // Called by Interactable's UnityEvent
    private void Awake()
    {
        clr = this.gameObject.GetComponent<SpriteRenderer>();
        og_Color = clr.color;
    }
    private void Update()
    {
        if (clr.color == Color.gray && Time.time >= nextAvailableTime)
        {
            clr.color = og_Color;

            Debug.Log("WaterStation refilled!");
        }
    }
    public void CollectWater()
    {
        Debug.Log("WaterStation interaction triggered!");

        if (Time.time < nextAvailableTime)
        {
            Debug.Log("Water station is refilling!");
            return;
        }

        if (waterPlayer == null)
        {
            Debug.LogError("WaterPlayer reference is missing!");
            return;
        }

        if (waterPlayer.GiveWater())
        {
            nextAvailableTime = Time.time + refillTime;
            Debug.Log("Water collected successfully!");
            clr.color = Color.gray;

            taskManager?.WaterCollected();
        }
        else
        {
            Debug.Log("Player already has water!");
        }
    }
}