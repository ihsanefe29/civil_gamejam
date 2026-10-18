using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FloodSceneManager))]
[RequireComponent(typeof(FloodTaskManager))]
public class FloodParkour : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Rigidbody2D playerBody;

    [Tooltip("Oyuncunun hareket ve zýplama scriptlerini ekle.")]
    [SerializeField]
    private Behaviour[] movementScripts =
        new Behaviour[0];

    [Header("Boat")]
    [SerializeField] private Transform boatSpawnPoint;

    [Header("Water")]
    [SerializeField] private GameObject waterPrefab;
    [SerializeField] private Transform waterSpawnPoint;

    [SerializeField, Min(0.1f)]
    private float riseDuration = 3f;

    [SerializeField] private string lowerTrigger = "Lower";

    [SerializeField, Min(0.1f)]
    private float lowerDuration = 3f;

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
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform coinSpawnPoint;

    public bool IsReady { get; private set; }

    private FloodSceneManager sceneManager;
    private FloodTaskManager taskManager;

    private Coroutine riseRoutine;
    private Coroutine lowerWaterRoutine;

    private GameObject spawnedWater;
    private Animator spawnedWaterAnimator;
    private GameObject spawnedCoin;

    private readonly List<GameObject> barrels = new();

    private bool playerLocked;
    private bool movementLockedForWaterEnd;
    private bool previousSimulated;
    private bool[] previousMovementStates;

    private void Awake()
    {
        sceneManager = GetComponent<FloodSceneManager>();
        taskManager = GetComponent<FloodTaskManager>();

        if (waterDeath != null)
            waterDeath.SetDangerous(false);
    }

    // Sadece kayýða E ile etkileþim yapýldýðýnda çaðrýlýr.
    public void BoardBoat()
    {
        if (playerBody == null || boatSpawnPoint == null)
        {
            Debug.LogError(
                "FloodParkour: Player Body ve Boat Spawn Point ata.",
                this
            );
            return;
        }

        MovePlayer(boatSpawnPoint.position);
    }

    public void StartFlood()
    {
        StopRise();
        StopLoweringWater();
        DestroySpawnedWater();

        UnlockMovementAfterWaterEnd();
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

        // Oyuncu bulunduðu yerde kalýr.
        // Burada kayýða ýþýnlama yapýlmaz.
        LockPlayer();

        if (!SpawnWater())
        {
            UnlockPlayer();
            return;
        }

        riseRoutine = StartCoroutine(RaiseWater());
    }

    private bool ReferencesAreValid()
    {
        return playerBody != null &&
               waterPrefab != null &&
               waterSpawnPoint != null &&
               waterDeath != null &&
               barrelPrefab != null &&
               firstBarrelPoint != null &&
               lastBarrelPoint != null &&
               coinPrefab != null &&
               coinSpawnPoint != null;
    }

    private bool SpawnWater()
    {
        spawnedWater = Instantiate(
            waterPrefab,
            waterSpawnPoint.position,
            waterSpawnPoint.rotation
        );

        spawnedWaterAnimator =
            spawnedWater.GetComponentInChildren<Animator>();

        if (spawnedWaterAnimator == null)
        {
            Debug.LogError(
                "Water prefab'ýnda Animator bulunamadý.",
                waterPrefab
            );

            DestroySpawnedWater();
            return false;
        }

        // Prefabýn varsayýlan yükselme animasyonu otomatik oynar.
        spawnedWaterAnimator.speed = 1f;

        return true;
    }

    private IEnumerator RaiseWater()
    {
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
            yield return null;
        }

        if (sceneManager.CurrentPhase != FloodPhase.Flood)
        {
            UnlockPlayer();
            riseRoutine = null;
            yield break;
        }

        SpawnBarrels();

        spawnedCoin = Instantiate(
            coinPrefab,
            coinSpawnPoint.position,
            coinPrefab.transform.rotation
        );

        spawnedCoin.SetActive(true);

        IsReady = true;
        taskManager.RegisterCoin(spawnedCoin);

        Physics2D.SyncTransforms();

        ResetJumpCount();

        // Aþaðýda kalan oyuncu suyla temas ederse ölür.
        waterDeath.SetDangerous(true);
        UnlockPlayer();

        riseRoutine = null;
    }

    public void FinishFlood()
    {
        StopRise();

        IsReady = false;

        if (waterDeath != null)
            waterDeath.SetDangerous(true);

        HideCoin();
        ClearBarrels();

        // Kontroller kapanýr ancak fizik açýk kalýr.
        // Variller gidince oyuncu suya düþerse ölür.
        LockMovementButKeepPhysics();

        StopLoweringWater();

        if (spawnedWaterAnimator != null)
        {
            lowerWaterRoutine =
                StartCoroutine(LowerWaterAndDestroy());
        }
        else
        {
            FinishWaterLowering();
        }
    }

    private IEnumerator LowerWaterAndDestroy()
    {
        spawnedWaterAnimator.updateMode =
            AnimatorUpdateMode.UnscaledTime;

        spawnedWaterAnimator.SetTrigger(lowerTrigger);

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0.1f, lowerDuration)
        );

        FinishWaterLowering();
    }

    private void FinishWaterLowering()
    {
        DestroySpawnedWater();
        lowerWaterRoutine = null;

        if (waterDeath != null)
            waterDeath.SetDangerous(false);

        // Oyuncuyu hiçbir yere ýþýnlama.
        UnlockMovementAfterWaterEnd();
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

    private void MovePlayer(Vector3 position)
    {
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

    private void SaveAndDisableMovement()
    {
        previousMovementStates = new bool[movementScripts.Length];

        for (int i = 0; i < movementScripts.Length; i++)
        {
            Behaviour script = movementScripts[i];

            if (script == null || script == this)
                continue;

            previousMovementStates[i] = script.enabled;
            script.enabled = false;
        }
    }

    private void RestoreMovement()
    {
        if (previousMovementStates == null)
            return;

        int count = Mathf.Min(
            movementScripts.Length,
            previousMovementStates.Length
        );

        for (int i = 0; i < count; i++)
        {
            Behaviour script = movementScripts[i];

            if (script != null && script != this)
                script.enabled = previousMovementStates[i];
        }
    }

    private void LockPlayer()
    {
        if (playerLocked || playerBody == null)
            return;

        playerLocked = true;
        previousSimulated = playerBody.simulated;

        SaveAndDisableMovement();

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

        RestoreMovement();
    }

    private void LockMovementButKeepPhysics()
    {
        if (movementLockedForWaterEnd)
            return;

        movementLockedForWaterEnd = true;

        if (playerLocked)
        {
            playerLocked = false;

            if (playerBody != null)
            {
                playerBody.linearVelocity = Vector2.zero;
                playerBody.angularVelocity = 0f;
                playerBody.simulated = previousSimulated;
            }

            // Hareket scriptleri zaten kapalý.
            return;
        }

        SaveAndDisableMovement();
    }

    private void UnlockMovementAfterWaterEnd()
    {
        if (!movementLockedForWaterEnd)
            return;

        movementLockedForWaterEnd = false;
        RestoreMovement();
    }

    private void StopRise()
    {
        if (riseRoutine == null)
            return;

        StopCoroutine(riseRoutine);
        riseRoutine = null;
    }

    private void StopLoweringWater()
    {
        if (lowerWaterRoutine == null)
            return;

        StopCoroutine(lowerWaterRoutine);
        lowerWaterRoutine = null;
    }

    private void DestroySpawnedWater()
    {
        if (spawnedWater != null)
        {
            spawnedWater.SetActive(false);
            Destroy(spawnedWater);
        }

        spawnedWater = null;
        spawnedWaterAnimator = null;
    }

    private void OnDisable()
    {
        StopRise();
        StopLoweringWater();

        IsReady = false;

        if (waterDeath != null)
            waterDeath.SetDangerous(false);

        UnlockPlayer();
        UnlockMovementAfterWaterEnd();

        ClearBarrels();
        HideCoin();
        DestroySpawnedWater();
    }
}