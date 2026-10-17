using UnityEngine;

public class EmberFault : MonoBehaviour
{
    [SerializeField] private FireSceneManager fireSceneManager;
    [SerializeField] private Animator animator;

    [Header("State")]
    [SerializeField] private bool isExtinguished = false;

    [SerializeField] private LevelTaskManager taskManager;

    private void Awake()
    {
        if (fireSceneManager == null)
            fireSceneManager = FindFirstObjectByType<FireSceneManager>();
    }

    public void Extinguish()
    {
        if (fireSceneManager == null)
            return;

        // Only extinguish during Preparation
        if (fireSceneManager.CurrentPhase != FirePhase.Preparation)
            return;

        if (isExtinguished)
            return;

        isExtinguished = true;

        if (animator != null)
        {
            animator.SetBool("isExtinguished", true);
        }

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        if (sprite != null)
            sprite.color = Color.gray;

        fireSceneManager.ScoreEmber();
        fireSceneManager.DeleteFromList(this.gameObject);
        taskManager?.CompleteTask(this.gameObject);

        Debug.Log("Ember extinguished! +20 points");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Ember triggered by: " + other.name);

        if (isExtinguished) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player takes damage!");
            if(!isExtinguished)
                fireSceneManager?.DamagePlayer();
        }
    }
}