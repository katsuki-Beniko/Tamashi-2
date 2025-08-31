using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    
    [Header("Interaction")]
    public float interactionRange = 2f;
    public LayerMask interactableLayer = -1;
    
    [Header("Audio")]
    public float minimumSpeedForFootsteps = 0.1f; // Minimum speed to play footsteps
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Transform currentInteractable;
    private bool isActive = false;
    private PlayerSwitcher playerSwitcher;
    
    // Footstep audio variables
    private bool wasMovingLastFrame = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Remove gravity for top-down movement
        rb.freezeRotation = true; // Prevent rotation from physics
        
        // Find the PlayerSwitcher in the scene (updated for Unity 6)
        playerSwitcher = FindFirstObjectByType<PlayerSwitcher>();
    }
    
    void Update()
    {
        if (isActive)
        {
            HandleInput();
            CheckForInteractables();
            HandleFootstepAudio();
        }
    }
    
    void FixedUpdate()
    {
        if (isActive)
        {
            HandleMovement();
        }
        else
        {
            // Stop movement when inactive
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
        
        // Stop movement immediately when deactivated
        if (!active)
        {
            rb.linearVelocity = Vector2.zero;
            moveInput = Vector2.zero;
            currentInteractable = null;
            
            // Stop footstep audio when becoming inactive
            StopFootstepSound();
            wasMovingLastFrame = false;
        }
    }
    
    private void HandleInput()
    {
        // WASD Movement input
        moveInput = Vector2.zero;
        
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            moveInput.y = 1f;
        else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            moveInput.y = -1f;
            
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            moveInput.x = -1f;
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            moveInput.x = 1f;
        
        // Normalize diagonal movement to prevent faster diagonal speed
        moveInput = moveInput.normalized;
        
        // Interaction input
        if (Keyboard.current.fKey.wasPressedThisFrame && currentInteractable != null)
        {
            InteractWithObject();
        }
    }
    
    private void HandleMovement()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }
    
    private void HandleFootstepAudio()
    {
        // Check if player is moving
        bool isMoving = rb.linearVelocity.magnitude > minimumSpeedForFootsteps;
        
        if (isMoving && !wasMovingLastFrame)
        {
            // Just started moving - start footstep audio
            StartFootstepSound();
        }
        else if (!isMoving && wasMovingLastFrame)
        {
            // Just stopped moving - stop footstep audio
            StopFootstepSound();
        }
        
        wasMovingLastFrame = isMoving;
    }
    
    private void StartFootstepSound()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.StartFootsteps();
        }
    }
    
    private void StopFootstepSound()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.StopFootsteps();
        }
    }
    
    private void CheckForInteractables()
    {
        Collider2D nearestInteractable = Physics2D.OverlapCircle(transform.position, interactionRange, interactableLayer);
        
        if (nearestInteractable != null && nearestInteractable.GetComponent<IInteractable>() != null)
        {
            if (currentInteractable != nearestInteractable.transform)
            {
                currentInteractable = nearestInteractable.transform;
                Debug.Log($"Can interact with: {currentInteractable.name}");
            }
        }
        else
        {
            if (currentInteractable != null)
            {
                currentInteractable = null;
                Debug.Log("No interactable nearby");
            }
        }
    }
    
    private void InteractWithObject()
    {
        IInteractable interactable = currentInteractable.GetComponent<IInteractable>();
        if (interactable != null)
        {
            interactable.Interact(this);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Visualize interaction range in Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
