using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.2f;
    public Vector2 offset;
    public Vector2 minBounds = new Vector2(-Mathf.Infinity, -Mathf.Infinity);
    public Vector2 maxBounds = new Vector2( Mathf.Infinity,  Mathf.Infinity);

    private Vector3 _velocity;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desired = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);

        // SmoothDamp = buttery follow
        Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);

        // Optional world bounds clamp (keeps camera inside level)
        float clampedX = Mathf.Clamp(smoothed.x, minBounds.x, maxBounds.x);
        float clampedY = Mathf.Clamp(smoothed.y, minBounds.y, maxBounds.y);

        transform.position = new Vector3(clampedX, clampedY, smoothed.z);
    }
}
