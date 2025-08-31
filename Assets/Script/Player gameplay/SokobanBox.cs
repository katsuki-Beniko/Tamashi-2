using UnityEngine;

public class SokobanBox : MonoBehaviour
{
    [Header("Grid Reference")]
    public Grid gridReference; // Will auto-find if not assigned
    
    [Header("Movement Settings")]
    public float moveSpeed = 8f; // Fast snapping to grid
    
    [Header("Push Settings")]
    public float pushCooldown = 0.3f; // Prevent rapid pushing
    public LayerMask wallLayer = -1;
    public LayerMask boxLayer = -1;
    
    [Header("Audio")]
    public AudioClip pushSound;
    public AudioClip blockedSound;
    
    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color targetColor = Color.green;
    public Color movingColor = Color.yellow;
    
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private bool isMoving = false;
    private Vector3 targetPosition;
    private float lastPushTime = 0f;
    private bool isOnTarget = false;
    
    // Grid system properties
    private Vector3 gridCellSize;
    private Vector3 gridCellGap;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Auto-find Grid if not assigned
        if (gridReference == null)
        {
            gridReference = FindFirstObjectByType<Grid>();
        }
        
        // Get grid properties from Unity's Grid component
        if (gridReference != null)
        {
            gridCellSize = gridReference.cellSize;
            gridCellGap = gridReference.cellGap;
            Debug.Log($"Using Unity Grid - Cell Size: {gridCellSize}, Cell Gap: {gridCellGap}");
        }
        else
        {
            // Fallback values if no Grid found
            gridCellSize = Vector3.one;
            gridCellGap = Vector3.zero;
            Debug.LogWarning("No Grid component found! Using fallback grid size of 1x1");
        }
        
        // Configure physics for boxes
        rb.gravityScale = 0f;
        rb.linearDamping = 0f; // No damping for snappy movement
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Snap to grid on start
        SnapToGrid();
        targetPosition = transform.position;
        
        // Store original color
        if (spriteRenderer != null)
        {
            normalColor = spriteRenderer.color;
        }
    }
    
    void Update()
    {
        HandleGridMovement();
        UpdateVisualFeedback();
    }
    
    public bool TryPush(Vector2 direction, Player pusher)
    {
        // Check cooldown to prevent rapid pushing
        if (Time.time - lastPushTime < pushCooldown)
        {
            return false;
        }
        
        // Don't allow push if already moving
        if (isMoving)
        {
            return false;
        }
        
        // Calculate target grid position using Unity's Grid
        Vector3Int currentGridPos = GetCurrentGridPosition();
        Vector3Int targetGridPos = currentGridPos + Vector3Int.RoundToInt(new Vector3(direction.x, direction.y, 0));
        Vector3 targetWorldPos = GridToWorldPosition(targetGridPos);
        
        // Check if target position is clear
        if (IsPositionClear(targetWorldPos))
        {
            // Valid push - start moving to target
            StartMovingToPosition(targetWorldPos);
            PlaySound(pushSound);
            lastPushTime = Time.time;
            
            Debug.Log($"Box pushed from {currentGridPos} to {targetGridPos}");
            return true;
        }
        else
        {
            // Blocked push
            PlaySound(blockedSound);
            Debug.Log($"Box push blocked - target position {targetGridPos} is occupied");
            return false;
        }
    }
    
    private bool IsPositionClear(Vector3 worldPosition)
    {
        // Check for walls
        Collider2D wallCollider = Physics2D.OverlapPoint(worldPosition, wallLayer);
        if (wallCollider != null)
        {
            Debug.Log($"Position blocked by wall: {wallCollider.name}");
            return false;
        }
        
        // Check for other boxes
        Collider2D boxCollider = Physics2D.OverlapPoint(worldPosition, boxLayer);
        if (boxCollider != null && boxCollider.gameObject != gameObject)
        {
            Debug.Log($"Position blocked by box: {boxCollider.name}");
            return false;
        }
        
        return true;
    }
    
    private void StartMovingToPosition(Vector3 worldPosition)
    {
        targetPosition = worldPosition;
        targetPosition.z = transform.position.z; // Preserve Z position
        isMoving = true;
        
        // Use bodyType instead of deprecated isKinematic
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
    
    private void HandleGridMovement()
    {
        if (!isMoving) return;
        
        // Move towards target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        
        // Check if reached target
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            // Snap to exact position
            transform.position = targetPosition;
            isMoving = false;
            
            // Use bodyType instead of deprecated isKinematic
            rb.bodyType = RigidbodyType2D.Dynamic;
            
            // Final snap to grid to ensure perfect alignment
            SnapToGrid();
            
            Debug.Log($"Box reached target position: {GetCurrentGridPosition()}");
        }
    }
    
    private void UpdateVisualFeedback()
    {
        if (spriteRenderer == null) return;
        
        if (isMoving)
        {
            spriteRenderer.color = movingColor;
        }
        else if (isOnTarget)
        {
            spriteRenderer.color = targetColor;
        }
        else
        {
            spriteRenderer.color = normalColor;
        }
    }
    
    // Unity Grid integration methods
    private Vector3Int GetCurrentGridPosition()
    {
        if (gridReference != null)
        {
            return gridReference.WorldToCell(transform.position);
        }
        else
        {
            // Fallback calculation
            return Vector3Int.RoundToInt(new Vector3(
                transform.position.x / gridCellSize.x,
                transform.position.y / gridCellSize.y,
                0
            ));
        }
    }
    
    private Vector3 GridToWorldPosition(Vector3Int gridPos)
    {
        if (gridReference != null)
        {
            return gridReference.CellToWorld(gridPos);
        }
        else
        {
            // Fallback calculation
            return new Vector3(
                gridPos.x * gridCellSize.x,
                gridPos.y * gridCellSize.y,
                transform.position.z
            );
        }
    }
    
    private void SnapToGrid()
    {
        Vector3Int gridPos = GetCurrentGridPosition();
        Vector3 snappedPos = GridToWorldPosition(gridPos);
        snappedPos.z = transform.position.z; // Preserve Z position
        transform.position = snappedPos;
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // Target detection for puzzle mechanics
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Target"))
        {
            isOnTarget = true;
            Debug.Log($"Box {gameObject.name} entered target");
        }
    }
    
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Target"))
        {
            isOnTarget = false;
            Debug.Log($"Box {gameObject.name} left target");
        }
    }
    
    // Public methods for external scripts
    public bool IsMoving()
    {
        return isMoving;
    }
    
    public bool IsOnTarget()
    {
        return isOnTarget;
    }
    
    public Vector3Int GetGridPosition()
    {
        return GetCurrentGridPosition();
    }
    
    void OnDrawGizmosSelected()
    {
        // Show current grid position using Unity's Grid
        Gizmos.color = isMoving ? Color.yellow : (isOnTarget ? Color.green : Color.white);
        Vector3Int gridPos = GetCurrentGridPosition();
        Vector3 worldPos = GridToWorldPosition(gridPos);
        
        // Draw grid cell bounds
        Vector3 cellSize = gridReference != null ? gridReference.cellSize : gridCellSize;
        Gizmos.DrawWireCube(worldPos + cellSize * 0.5f, cellSize);
        
        // Show target position when moving
        if (isMoving)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(targetPosition + cellSize * 0.5f, cellSize * 0.8f);
            Gizmos.DrawLine(transform.position, targetPosition);
        }
        
        // Show push detection area
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, cellSize * 1.2f);
        
        // Show grid info
        if (Application.isPlaying && gridReference != null)
        {
            // Display grid cell coordinates
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, $"Grid: {gridPos}");
        }
    }
}
