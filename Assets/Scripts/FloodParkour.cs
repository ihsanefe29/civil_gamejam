using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FloodSceneManager))]
[RequireComponent(typeof(FloodTaskManager))]
public class FloodParkour : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Rigidbody2D playerBody;

    [Tooltip("Sadece oyuncunun hareket ve zýplama scriptlerini ekle.")]
    [SerializeField]
    private Behaviour[] movementScripts =
        new Behaviour[0];

    [Header("Spawn Points")]
    [Tooltip("Oyuncunun kayýk üstündeki baþlangýç konumu.")]
    [SerializeField] private Transform boatSpawnPoint;

    [SerializeField] private Transform preparationSpawnPoint;

    [Header("Water")]
    [SerializeField] private Transform waterVisual;
    [SerializeField] private Transform waterLowPoint;
    [SerializeField] private Transform waterHighPoint;

    [SerializeField, Min(0.1f)]
    private float riseDuration = 3f;

    [SerializeField] private FloodWaterDeath waterDeath;

    [Header("Barrels")]
    [SerializeField] private GameObject barrelPrefab;
    [SerializeField] private Transform firstBarrelPoint;
    [SerializeField] private Transform lastBarrelPoint;

    [SerializeField, Min(2)]
    private int barrelCount = 6;

    [SerializeField, Min(0f)]
    private float horizontalRandomness = 0.5f;

    [Header("Düdük Balýðý")]
    [Tooltip("FloodCoin ve Interactable bulunan düdük balýðý prefabý.")]
    [SerializeField] private GameObject coinPrefab;

    [SerializeField] private Transform coinSpawnPoint;

    public bool IsReady { get; private set; }

    private FloodSceneManager sceneManager;
    private FloodTaskManager taskManager;

    private Coroutine riseRoutine;
    private GameObject spawnedCoin;

    private readonly List<GameObject> barrels = new();

    private bool playerLocked;
    private bool previousSimulated;
    private bool[] previousMovementStates;

    private void Awake()
    {
        sceneManager = GetComponent<FloodSceneManager>();
        taskManager = GetComponent<FloodTaskManager>();

        if (waterDeath != null)
            waterDeath.SetDangerous(false);

        SetWaterLow();
    }

    public void StartFlood()
    {
        StopRise();
        UnlockPlayer();

        IsReady = false;

        if (waterDeath != null)
            waterDeath.SetDangerous(false);

        ClearBarrels();
        HideCoin();

        if (!ReferencesAreValid())
        {
            Debug.LogError(
                "FloodParkour: Inspector'daki eksik referanslarý ata.",
                this
            );
            return;
        }

        // Su hareket ederken bu noktalar sabit kalmalý.
        if (playerBody.transform.IsChildOf(waterVisual) ||
            boatSpawnPoint.IsChildOf(waterVisual) ||
            waterLowPoint.IsChildOf(waterVisual) ||
            waterHighPoint.IsChildOf(waterVisual))
        {
            Debug.LogError(
                "Player, BoatSpawnPoint, WaterLowPoint ve " +
                "WaterHighPoint, WaterVisual'ýn altýnda olmamalý.",
                this
            );
            return;
        }

        SetWaterLow();

        LockPlayer();

        // Karakter sadece burada kayýða ýþýnlanýr.
        MovePlayer(boatSpawnPoint.position);

        Physics2D.SyncTransforms();

        riseRoutine = StartCoroutine(RaiseWater());
    }

    private bool ReferencesAreValid()
    {
        return playerBody != null &&
               boatSpawnPoint != null &&
               preparationSpawnPoint != null &&
               waterVisual != null &&
               waterLowPoint != null &&
               waterHighPoint != null &&
               waterDeath != null &&
               barrelPrefab != null &&
               firstBarrelPoint != null &&
               lastBarrelPoint != null &&
               coinPrefab != null &&
               coinSpawnPoint != null;
    }

    private IEnumerator RaiseWater()
    {
        Vector3 low = waterLowPoint.position;
        Vector3 high = waterHighPoint.position;

        float elapsed = 0f;
        float duration = Mathf.Max(0.1f, riseDuration);

        while (elapsed < duration)
        {
            if (sceneManager.CurrentPhase != FloodPhase.Flood)
            {
                UnlockPlayer();
                riseRoutine = null;
                yield break;
            }

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            // SADECE SU YÜKSELÝR.
            // Oyuncunun ve kayýðýn konumu deðiþtirilmez.
            waterVisual.position = Vector3.Lerp(low, high, eased);

            yield return null;
        }

        waterVisual.position = high;

        SpawnBarrels();

        spawnedCoin = Instantiate(
            coinPrefab,
            coinSpawnPoint.position,
            coinPrefab.transform.rotation
        );

        spawnedCoin.SetActive(true);

        // RegisterCoin hedefleri yenilediði için önce hazýr olmalý.
        IsReady = true;
        taskManager.RegisterCoin(spawnedCoin);

        Physics2D.SyncTransforms();

        ResetJumpCount();
        waterDeath.SetDangerous(true);
        UnlockPlayer();

        riseRoutine = null;
    }

    public void FinishFlood()
    {
        StopRise();

        IsReady = false;

        if (waterDeath != null)
            waterDeath.SetDangerous(false);

        HideCoin();
        ClearBarrels();
        SetWaterLow();

        if (playerBody != null && preparationSpawnPoint != null)
            MovePlayer(preparationSpawnPoint.position);

        Physics2D.SyncTransforms();

        UnlockPlayer();
    }

    private void SpawnBarrels()
    {
        ClearBarrels();

        int count = Mathf.Max(2, barrelCount);

        Vector3 start = firstBarrelPoint.position;
        Vector3 end = lastBarrelPoint.position;

        float spacing = Mathf.Abs(end.x - start.x) / (count - 1);

        float jitter = Mathf.Min(
            horizontalRandomness,
            spacing * 0.3f
        );

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            Vector3 position = Vector3.Lerp(start, end, t);

            // Ýlk ve son varilin konumu sabit.
            if (i > 0 && i < count - 1)
                position.x += Random.Range(-jitter, jitter);

            GameObject barrel = Instantiate(
                barrelPrefab,
                position,
                barrelPrefab.transform.rotation
            );

            barrels.Add(barrel);
        }
    }

    private void HideCoin()
    {
        if (spawnedCoin != null)
        {
            spawnedCoin.tag = "Untagged";
            spawnedCoin.SetActive(false);
            Destroy(spawnedCoin);
        }

        spawnedCoin = null;

        if (taskManager != null)
            taskManager.RegisterCoin(null);
    }

    private void ClearBarrels()
    {
        foreach (GameObject barrel in barrels)
        {
            if (barrel == null)
                continue;

            barrel.SetActive(false);
            Destroy(barrel);
        }

        barrels.Clear();
    }

    private void SetWaterLow()
    {
        if (waterVisual != null && waterLowPoint != null)
            waterVisual.position = waterLowPoint.position;
    }

    private void MovePlayer(Vector3 position)
    {
        if (playerBody == null)
            return;

        playerBody.linearVelocity = Vector2.zero;
        playerBody.angularVelocity = 0f;

        playerBody.transform.position = position;
        playerBody.position = new Vector2(position.x, position.y);

        ResetJumpCount();
        Physics2D.SyncTransforms();
    }

    private void ResetJumpCount()
    {
        if (playerBody == null)
            return;

        PlayerController controller =
            playerBody.GetComponent<PlayerController>();

        if (controller != null)
            controller.jumpCount = 0;
    }

    private void LockPlayer()
    {
        if (playerLocked || playerBody == null)
            return;

        playerLocked = true;
        previousSimulated = playerBody.simulated;

        previousMovementStates = new bool[movementScripts.Length];

        for (int i = 0; i < movementScripts.Length; i++)
        {
            Behaviour script = movementScripts[i];

            if (script == null || script == this)
                continue;

            previousMovementStates[i] = script.enabled;
            script.enabled = false;
        }

        playerBody.linearVelocity = Vector2.zero;
        playerBody.angularVelocity = 0f;
        playerBody.simulated = false;
    }

    private void UnlockPlayer()
    {
        if (!playerLocked)
            return;

        playerLocked = false;

        if (playerBody != null)
        {
            playerBody.linearVelocity = Vector2.zero;
            playerBody.angularVelocity = 0f;
            playerBody.simulated = previousSimulated;
        }

        for (int i = 0; i < movementScripts.Length; i++)
        {
            Behaviour script = movementScripts[i];

            if (script == null || script == this)
                continue;

            script.enabled = previousMovementStates[i];
        }
    }

    private void StopRise()
    {
        if (riseRoutine == null)
            return;

        StopCoroutine(riseRoutine);
        riseRoutine = null;
    }

    private void OnDisable()
    {
        StopRise();
        IsReady = false;

        if (waterDeath != null)
            waterDeath.SetDangerous(false);

        UnlockPlayer();
        ClearBarrels();
        HideCoin();
    }
}