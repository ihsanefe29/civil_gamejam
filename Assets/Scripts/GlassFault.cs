using UnityEngine;

public class GlassFault : MonoBehaviour
{
    [SerializeField] private FireSceneManager fireSceneManager;

    public void FixGlass()
    {
        fireSceneManager.DeleteFromList(gameObject);
        Destroy(this.gameObject);
    }
}