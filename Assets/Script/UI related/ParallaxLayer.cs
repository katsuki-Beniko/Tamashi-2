using UnityEngine;

[ExecuteAlways]
public class ParallaxLayer : MonoBehaviour
{
    public Transform cameraTransform;
    [Tooltip("0 = locked to camera, 1 = moves with world. Farther layers use smaller values like 0.1–0.4")]
    public Vector2 parallaxMultiplier = new Vector2(0.3f, 0.3f);

    private Vector3 _lastCamPos;

    void Start()
    {
        if (!cameraTransform) cameraTransform = Camera.main.transform;
        _lastCamPos = cameraTransform.position;
    }

    void LateUpdate()
    {
        if (!cameraTransform) return;

        Vector3 camDelta = cameraTransform.position - _lastCamPos;
        // Move opposite to camera movement scaled by multiplier
        transform.position += new Vector3(camDelta.x * parallaxMultiplier.x, camDelta.y * parallaxMultiplier.y, 0f);

        _lastCamPos = cameraTransform.position;
    }
}
