using System.Collections.Generic;
using UnityEngine;

public enum FloodObjectiveStage
{
    None,
    InteractWithFish,
    TakeLadder,
    CarryLadderToBoat,
    WaitForFlood,
    WaitForWater,
    CollectSpecialItems,
    WaitForFloodEnd,
    Completed
}

public class FloodTaskManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FloodSceneManager sceneManager;
    [SerializeField] private FloodParkour parkour;
    [SerializeField] private Camera mainCamera;

    [Header("Objects")]
    [SerializeField] private GameObject ladder;
    [SerializeField] private GameObject boat;
    [SerializeField] private GameObject coin;

    [Header("Arrow")]
    [SerializeField] private ObjectiveArrow arrowPrefab;

    private readonly List<GameObject> allFish = new();
    private readonly List<GameObject> remainingFish = new();

    private readonly HashSet<Transform> activeTargets = new();
    private readonly Dictionary<Transform, ObjectiveArrow> arrows = new();
    private readonly List<Transform> removeArrows = new();

    private bool hasLadder;
    private bool ladderDelivered;
    private bool coinCollectedThisFlood;

    private int totalCoins;
    private float nextRefreshTime;

    public GameObject Coin => coin;

    public bool HasLadder => hasLadder;
    public bool LadderDelivered => ladderDelivered;

    public int RequiredFishInteractions => allFish.Count;
    public int FishInteractionCount => allFish.Count - remainingFish.Count;

    public int SpecialItemCount => totalCoins;
    public int RequiredSpecialItems => 2;

    public GameObject Ladder => ladder;
    public GameObject Boat => boat;

    public bool AllSpecialItemsCollected =>
        totalCoins >= RequiredSpecialItems;

    public FloodObjectiveStage CurrentStage
    {
        get
        {
            if (sceneManager == null)
                return FloodObjectiveStage.None;

            switch (sceneManager.CurrentPhase)
            {
                case FloodPhase.Preparation:
                    if (remainingFish.Count > 0)
                        return FloodObjectiveStage.InteractWithFish;

                    if (ladderDelivered)
                        return FloodObjectiveStage.WaitForFlood;

                    return hasLadder
                        ? FloodObjectiveStage.CarryLadderToBoat
                        : FloodObjectiveStage.TakeLadder;

                case FloodPhase.Flood:
                    if (coinCollectedThisFlood)
                        return FloodObjectiveStage.WaitForFloodEnd;

                    return parkour != null && parkour.IsReady
                        ? FloodObjectiveStage.CollectSpecialItems
                        : FloodObjectiveStage.WaitForWater;

                case FloodPhase.Completed:
                    return FloodObjectiveStage.Completed;

                default:
                    return FloodObjectiveStage.None;
            }
        }
    }

    private void Awake()
    {
        if (sceneManager == null)
            sceneManager = GetComponent<FloodSceneManager>();

        if (parkour == null)
            parkour = GetComponent<FloodParkour>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        foreach (ElectricFish fish in FindObjectsByType<ElectricFish>(
            FindObjectsSortMode.None))
        {
            if (!allFish.Contains(fish.gameObject))
                allFish.Add(fish.gameObject);
        }

        remainingFish.AddRange(allFish);

        SetAllTagsOff();

        if (coin != null)
            coin.SetActive(false);
    }

    private void Update()
    {
        if (Time.time < nextRefreshTime)
            return;

        nextRefreshTime = Time.time + 0.15f;

        remainingFish.RemoveAll(fish => fish == null);
        RefreshTargets();
    }

    public void ResetPreparation()
    {
        hasLadder = false;
        ladderDelivered = false;
        coinCollectedThisFlood = false;

        remainingFish.Clear();

        foreach (GameObject fish in allFish)
        {
            if (fish == null)
                continue;

            fish.SetActive(true);
            remainingFish.Add(fish);
        }

        if (ladder != null)
            ladder.SetActive(true);

        if (coin != null)
            coin.SetActive(false);

        RefreshTargets();
    }

    public void HandleFloodStarted()
    {
        hasLadder = false;
        coinCollectedThisFlood = false;

        if (ladder != null)
            ladder.SetActive(false);

        RefreshTargets();
    }

    public void HandleLevelCompleted()
    {
        SetAllTagsOff();
        ClearArrows();
    }

    // Interactable event'ine baðlanacak tek görev metodu.
    public void CompleteTask(GameObject obj)
    {
        if (obj == null || !obj.activeInHierarchy ||
            sceneManager == null || Time.timeScale <= 0f)
        {
            return;
        }

        switch (CurrentStage)
        {
            case FloodObjectiveStage.InteractWithFish:
                if (!remainingFish.Remove(obj))
                    return;

                obj.tag = "Untagged";
                obj.SetActive(false);
                break;

            case FloodObjectiveStage.TakeLadder:
                if (obj != ladder)
                    return;

                hasLadder = true;
                obj.tag = "Untagged";
                obj.SetActive(false);
                break;

            case FloodObjectiveStage.CarryLadderToBoat:
                {
                    if (obj != boat)
                        return;

                    hasLadder = false;
                    ladderDelivered = true;

                    obj.tag = "Untagged";

                    // E etkileþimi burada gerçekleþtiði için oyuncu kayýða çýkar.
                    parkour.BoardBoat();

                    sceneManager.BeginFlood();
                    return;
                }
            case FloodObjectiveStage.CollectSpecialItems:
                if (obj != coin || coinCollectedThisFlood)
                    return;

                coinCollectedThisFlood = true;
                totalCoins++;
                coin = null;

                obj.tag = "Untagged";
                obj.SetActive(false);
                Destroy(obj);

                sceneManager.EndFlood();
                return;

            default:
                return;
        }

        RefreshTargets();
    }

    public void RefreshTargets()
    {
        SetAllTagsOff();
        activeTargets.Clear();

        switch (CurrentStage)
        {
            case FloodObjectiveStage.InteractWithFish:
                foreach (GameObject fish in remainingFish)
                    EnableTarget(fish);
                break;

            case FloodObjectiveStage.TakeLadder:
                EnableTarget(ladder);
                break;

            case FloodObjectiveStage.CarryLadderToBoat:
                EnableTarget(boat);
                break;

            case FloodObjectiveStage.CollectSpecialItems:
                EnableTarget(coin);
                break;
        }

        RefreshArrows();
    }

    private void EnableTarget(GameObject obj)
    {
        if (obj == null || !obj.activeInHierarchy)
            return;

        obj.tag = "interactable";
        activeTargets.Add(obj.transform);
    }

    private void SetAllTagsOff()
    {
        foreach (GameObject fish in allFish)
            DisableTag(fish);

        DisableTag(ladder);
        DisableTag(boat);
        DisableTag(coin);
    }

    private static void DisableTag(GameObject obj)
    {
        if (obj != null)
            obj.tag = "Untagged";
    }

    private void RefreshArrows()
    {
        removeArrows.Clear();

        foreach (var pair in arrows)
        {
            if (pair.Key == null || pair.Value == null ||
                !activeTargets.Contains(pair.Key))
            {
                if (pair.Value != null)
                    Destroy(pair.Value.gameObject);

                removeArrows.Add(pair.Key);
            }
        }

        foreach (Transform target in removeArrows)
            arrows.Remove(target);

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null || arrowPrefab == null)
            return;

        foreach (Transform target in activeTargets)
        {
            if (arrows.ContainsKey(target))
                continue;

            ObjectiveArrow arrow = Instantiate(arrowPrefab);
            arrow.Initialize(target, mainCamera);
            arrows.Add(target, arrow);
        }
    }
    public void RegisterCoin(GameObject spawnedCoin)
    {
        coin = spawnedCoin;
        RefreshTargets();
    }

    private void ClearArrows()
    {
        foreach (var pair in arrows)
        {
            if (pair.Value != null)
                Destroy(pair.Value.gameObject);
        }

        arrows.Clear();
        activeTargets.Clear();
    }

    private void OnDisable()
    {
        SetAllTagsOff();
        ClearArrows();
    }
}