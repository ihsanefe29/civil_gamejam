using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class CameraShake : MonoBehaviour
{
    [SerializeField, Min(0f)] private float defaultDuration = 4f;
    [SerializeField, Min(0f)] private float defaultStrength = 0.06f;

    private Vector3 currentOffset;
    private Vector3 appliedOffset;
    private Vector3 lastAppliedPosition;
    private bool hasAppliedPosition;
    private Coroutine shakeRoutine;

    public void Shake()
    {
        Shake(defaultDuration, defaultStrength);
    }

    public void Shake(float duration, float strength)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, strength));
    }

    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float falloff = 1f - (elapsed / duration);
            currentOffset = Random.insideUnitCircle * (strength * falloff);
            yield return null;
        }

        currentOffset = Vector3.zero;
        shakeRoutine = null;
    }

    private void LateUpdate()
    {
        Vector3 basePosition = transform.position;

        if (hasAppliedPosition &&
            (transform.position - lastAppliedPosition).sqrMagnitude < 0.000001f)
        {
            basePosition -= appliedOffset;
        }

        appliedOffset = currentOffset;
        transform.position = basePosition + appliedOffset;
        lastAppliedPosition = transform.position;
        hasAppliedPosition = true;
    }
}
