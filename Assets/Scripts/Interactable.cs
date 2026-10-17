using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public UnityEvent onInteract;

    public void AddListener(UnityAction listener)
    {
        onInteract ??= new UnityEvent();
        onInteract.AddListener(listener);
    }

    public void Interact()
    {
        onInteract?.Invoke();
    }
}
