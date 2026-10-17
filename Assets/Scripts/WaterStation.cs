using Unity.VisualScripting;
using UnityEngine;

public class WaterStation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShootEm waterPlayer;

    [Header("Settings")]
    [SerializeField] private float refillTime = 5f;

    [SerializeField] private LevelTaskManager taskManager;

    private float nextAvailableTime = 0f;
    [SerializeField] private Sprite empty_sprt;
    private SpriteRenderer rndr;
    private Sprite full_sprt;
    private bool waterFull = true;

    // Called by Interactable's UnityEvent
    private void Awake()
    {
        rndr = this.gameObject.GetComponent<SpriteRenderer>();
        full_sprt = rndr.sprite;

    }
    private void Update()
    {
        if (waterFull == false && Time.time >= nextAvailableTime)
        {
            waterFull = true;
            rndr.sprite = full_sprt;

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
            waterFull = false;
            nextAvailableTime = Time.time + refillTime;
            Debug.Log("Water collected successfully!: " + empty_sprt.name);
            rndr.sprite = empty_sprt;

            taskManager?.WaterCollected();
        }
        else
        {
            Debug.Log("Player already has water!");
        }
    }
}