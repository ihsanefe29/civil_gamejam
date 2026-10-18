using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Interactable))]
public class FloodInteractionAction : MonoBehaviour
{

    public void DestroyObject()
    {
        Destroy(gameObject);
    }
}