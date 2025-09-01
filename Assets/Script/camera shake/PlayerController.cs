using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public InputAction MoveAction;
    Rigidbody2D rb;
    Vector2 move;
    private Animator _animator;

    // declare animator parameter
    public const string _horizontal = "Horizontal";
    public const string _vertical = "Vertical";
    public const string _lastHorizontal = "LastHorizontal";
    public const string _lastVertical = "LastVertical";

    [Header("Camera Shake Control")]
    public bool enableMovementShake = false; // Option to enable shake while moving
    public float movementShakeIntensity = 0.1f;
    public float movementShakeDuration = 0.1f;
    
    private CameraController cameraController;
    private float lastShakeTime = 0f;
    private float shakeInterval = 0.2f; // How often to shake while moving

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        MoveAction.Enable();

        // animator define
        _animator = GetComponent<Animator>();
        
        // Get camera controller reference
        cameraController = Camera.main.GetComponent<CameraController>();
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<CameraController>();
        }
    }

    void Update()
    {
        move = MoveAction.ReadValue<Vector2>();
        
        // REMOVED: Constant camera shake call
        // Camera.main.GetComponent<CameraController>().Shake(0.5f, 0.3f, 0, true);
        
        // Optional: Add movement-based shake (disabled by default)
        if (enableMovementShake && move != Vector2.zero && cameraController != null)
        {
            // Only shake occasionally while moving, not every frame
            if (Time.time - lastShakeTime > shakeInterval)
            {
                cameraController.Shake(movementShakeDuration, movementShakeIntensity, 2, true);
                lastShakeTime = Time.time;
            }
        }

        // character moving position
        _animator.SetFloat(_horizontal, move.x);
        _animator.SetFloat(_vertical, move.y);

        if (move != Vector2.zero)
        {
            _animator.SetFloat(_lastHorizontal, move.x);
            _animator.SetFloat(_lastVertical, move.y);
        }
    }

    private void FixedUpdate()
    {
        Vector2 position = (Vector2)transform.position + 3.0f * Time.deltaTime * move;
        transform.position = position;
    }
    
    // Public method to trigger camera shake from external sources
    public void TriggerCameraShake(float duration, float amplitude, int softLevel = 0, bool decrease = true)
    {
        if (cameraController != null)
        {
            cameraController.Shake(duration, amplitude, softLevel, decrease);
        }
    }
    
    // Method to enable/disable movement shake
    public void SetMovementShake(bool enabled)
    {
        enableMovementShake = enabled;
    }
}
