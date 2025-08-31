using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public void Shake(float duration, float amplitude, int softLevel = 0, bool decrease = false)
    {
        AnimationCurve animation = decrease ? AnimationCurve.Linear(0, 1, 1, 0) : AnimationCurve.Constant(0, 1, 1);
        StartCoroutine(Shake_Internal(duration, amplitude, softLevel, animation));
    }

    public void Shake(float duration, float amplitude, int softLevel = 0, AnimationCurve animation = null)
    {
        if (animation == null) animation = AnimationCurve.Linear(0, 1, 1, 0);
        StartCoroutine(Shake_Internal(duration, amplitude, softLevel, animation));
    }

    private IEnumerator Shake_Internal(float duration, float amplitude, int softLevel, AnimationCurve animation)
    {
        Vector3 initialPosition = transform.position;
        float amp = amplitude;
        if (softLevel < 0) softLevel = 0;
        int softCount = 0;

        for (float i = 0; i < duration; i += Time.deltaTime)
        {
            if (softLevel != 0 && softCount < softLevel)
                softCount++;
            else
            {
                transform.position = initialPosition + Random.insideUnitSphere * amp;
                amp = amplitude * animation.Evaluate(i);
                softCount = 0;
            }
            yield return null;
        }

        transform.position = initialPosition;
    }
}
