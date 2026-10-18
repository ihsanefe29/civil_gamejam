
using UnityEngine;

public class PortalSoundTrigger : MonoBehaviour
{
    public void PlayPortalSound()
    {
        Level1AudioManager.Instance?.PlayPortal();
    }
}
