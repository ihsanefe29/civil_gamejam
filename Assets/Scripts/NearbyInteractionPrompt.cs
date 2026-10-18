using UnityEngine;
using UnityEngine.UI;

public class NearbyInteractionPrompt : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private LayerMask detectionLayers = ~0;

    [Header("Appearance")]
    [SerializeField] private string message = "E - Topla";
    [SerializeField] private float worldOffset = 0.55f;
    [SerializeField] private float fadeSpeed = 9f;

    [Header("Portal")]
    [SerializeField] private float portalOffset = 1f;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform promptRect;
    private Text promptText;
    private Camera mainCamera;

    private FloodTaskManager floodTaskManager;

    private void Awake()
    {
        mainCamera = Camera.main;

        // Diðer sahnelerde bulunmayabilir; sorun deðil.
        floodTaskManager = FindFirstObjectByType<FloodTaskManager>();

        CreateUI();
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Transform target = Time.timeScale > 0f
            ? FindClosestTarget()
            : null;

        bool visible = target != null && mainCamera != null;
        bool isPortal = HasComponent<ExitPortal>(target);

        float offset = isPortal ? portalOffset : worldOffset;

        if (visible)
        {
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(
                target.position + Vector3.up * offset
            );

            visible = screenPosition.z > 0f;

            if (visible)
                promptRect.position = screenPosition;
        }

        promptText.text = GetMessage(target);
        promptText.fontSize = isPortal ? 26 : 19;

        // Uzun metinlerde panel otomatik geniþler.
        float width = Mathf.Max(
            isPortal ? 250f : 190f,
            promptText.preferredWidth + 24f
        );

        promptRect.sizeDelta = new Vector2(
            width,
            isPortal ? 60f : 44f
        );

        canvasGroup.alpha = Mathf.MoveTowards(
            canvasGroup.alpha,
            visible ? 1f : 0f,
            fadeSpeed * Time.unscaledDeltaTime
        );
    }

    private string GetMessage(Transform target)
    {
        if (target == null)
            return message;

        if (HasComponent<ExitPortal>(target))
            return "E - Portala Gir";

        if (HasComponent<WaterStation>(target))
            return "E - Su Doldur";

        if (HasComponent<ElectricFish>(target))
            return "E - Balýðý Topla";

        if (HasComponent<FloodCoin>(target))
            return "E - Coin Topla";

        if (floodTaskManager != null)
        {
            if (MatchesTarget(target, floodTaskManager.Ladder))
                return "E - Merdiveni Al";

            if (MatchesTarget(target, floodTaskManager.Boat))
                return "E - Kayýða Çýk";

            if (MatchesTarget(target, floodTaskManager.Coin))
                return "E - Düdük Balýðýný Topla";
        }

        return message;
    }

    private static bool HasComponent<T>(Transform target)
        where T : Component
    {
        return target != null &&
            (target.GetComponentInParent<T>() != null ||
             target.GetComponentInChildren<T>() != null);
    }

    private static bool MatchesTarget(
        Transform target,
        GameObject obj)
    {
        if (target == null || obj == null)
            return false;

        return target == obj.transform ||
               target.IsChildOf(obj.transform);
    }

    private Transform FindClosestTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            detectionRadius,
            detectionLayers
        );

        Transform closest = null;
        float closestDistance = float.PositiveInfinity;

        foreach (Collider2D hit in hits)
        {
            Transform candidate = hit.transform;

            // Collider veya parent üzerinde tag bulunabilir.
            while (candidate != null &&
                   !candidate.CompareTag("interactable"))
            {
                candidate = candidate.parent;
            }

            if (candidate == null)
                continue;

            Interactable interactable =
                candidate.GetComponent<Interactable>();

            if (interactable != null &&
                !interactable.isActiveAndEnabled)
            {
                continue;
            }

            Vector2 point = hit.ClosestPoint(transform.position);

            float distance = (
                point - (Vector2)transform.position
            ).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = candidate;
            }
        }

        return closest;
    }

    private void CreateUI()
    {
        GameObject canvasObject = new GameObject(
            "Interaction Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler)
        );

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panelObject = new GameObject(
            "Interaction Prompt",
            typeof(RectTransform),
            typeof(Image),
            typeof(CanvasGroup)
        );

        panelObject.transform.SetParent(canvas.transform, false);

        promptRect = panelObject.GetComponent<RectTransform>();
        promptRect.pivot = new Vector2(0.5f, 0f);
        promptRect.sizeDelta = new Vector2(190f, 44f);

        Image background = panelObject.GetComponent<Image>();
        background.color = new Color(0.08f, 0.06f, 0.04f, 0.75f);
        background.raycastTarget = false;

        canvasGroup = panelObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        GameObject textObject = new GameObject(
            "Text",
            typeof(RectTransform),
            typeof(Text)
        );

        textObject.transform.SetParent(panelObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 3f);
        textRect.offsetMax = new Vector2(-8f, -3f);

        promptText = textObject.GetComponent<Text>();
        promptText.font = Resources.GetBuiltinResource<Font>(
            "LegacyRuntime.ttf"
        );
        promptText.fontSize = 19;
        promptText.fontStyle = FontStyle.Bold;
        promptText.alignment = TextAnchor.MiddleCenter;
        promptText.horizontalOverflow = HorizontalWrapMode.Overflow;
        promptText.color = new Color(1f, 0.96f, 0.82f);
        promptText.raycastTarget = false;
        promptText.text = message;
    }

    private void OnDisable()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void OnDestroy()
    {
        if (canvas != null)
            Destroy(canvas.gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRadius
        );
    }
}