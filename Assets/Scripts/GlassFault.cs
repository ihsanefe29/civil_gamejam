using UnityEngine;

public class GlassFault : MonoBehaviour
{
    [SerializeField] private FireSceneManager fireSceneManager;

    private bool isFixed = false;

    public void FixGlass()
    {
        if (isFixed || fireSceneManager == null)
            return;

        isFixed = true;

        fireSceneManager.DeleteFromList(gameObject);
        fireSceneManager.ScoreGlass();
        Destroy(this.gameObject);

        Debug.Log("Glass repaired! +10 points");
    }
}