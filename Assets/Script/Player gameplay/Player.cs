using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    
    [Header("Interaction")]
    public float interactionRange = 2f;
    public LayerMask interactableLayer = -1;
    
    [Header("Box Pushing")]
    public LayerMask boxLayer = -1;
    public float pushForce = 2f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Transform currentInteractable;
    private bool isActive = false;
    private PlayerSwitcher playerSwitcher;

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
    
    void OnCollisionStay2D(Collision2D collision)
    {
        // Check if we're colliding with a box
        SokobanBox box = collision.gameObject.GetComponent<SokobanBox>();
        if (box != null && moveInput.magnitude > 0.1f)
        {
            // Calculate push direction based on collision
            Vector2 pushDirection = GetPushDirection(collision);
            
            // Try to push the box
            box.TryPush(pushDirection, this);
        }
    }
    
    private Vector2 GetPushDirection(Collision2D collision)
    {
        // Get direction from player to box
        Vector2 dirToBox = (collision.transform.position - transform.position).normalized;
        
        // Snap to cardinal directions (Sokoban-style)
        Vector2 cardinalDirection;
        if (Mathf.Abs(dirToBox.x) > Mathf.Abs(dirToBox.y))
        {
            // Horizontal push
            cardinalDirection = new Vector2(dirToBox.x > 0 ? 1 : -1, 0);
        }
        else
        {
            // Vertical push
            cardinalDirection = new Vector2(0, dirToBox.y > 0 ? 1 : -1);
        }
        
        return cardinalDirection;
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
    
    // PUBLIC METHODS for other scripts
    public bool IsMoving()
    {
        return moveInput.magnitude > 0.1f && rb.linearVelocity.magnitude > 0.1f && isActive;
    }
    
    public Vector2 GetMovementDirection()
    {
        return moveInput;
    }
    
    public Vector2 GetVelocity()
    {
        return rb.linearVelocity;
    }
    
    void OnDrawGizmosSelected()
    {
        // Visualize interaction range in Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Show movement direction
        if (Application.isPlaying && IsMoving())
        {
            Gizmos.color = Color.green;
            Vector3 moveDir = new Vector3(moveInput.x, moveInput.y, 0);
            Gizmos.DrawLine(transform.position, transform.position + moveDir * 1.5f);
        }
    }
}
