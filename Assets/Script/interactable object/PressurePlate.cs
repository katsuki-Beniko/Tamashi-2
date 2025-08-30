using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Plate Settings")]
    public int plateID = 1; // Unique ID for this plate (used in sequences)
    public bool isPressed = false;
    public bool stayPressed = false; // If true, stays pressed once activated
    
    [Header("Visual Feedback")]
    public Sprite plateUpSprite;
    public Sprite plateDownSprite;
    public Color plateUpColor = Color.gray;
    public Color plateDownColor = Color.green;
    public float pressDepth = 0.2f; // How far down the plate moves when pressed
    
    [Header("Audio")]
    public AudioClip pressSound;
    public AudioClip releaseSound;
    
    [Header("Detection")]
    public LayerMask triggerLayers = -1; // What can trigger this plate (players, objects, etc.)
    
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Vector3 originalPosition;
    private Vector3 pressedPosition;
    private PressurePlateController controller;
    private int currentTriggersOnPlate = 0;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        controller = GetComponentInParent<PressurePlateController>();
        
        // Store original position for animation
        originalPosition = transform.position;
        pressedPosition = originalPosition + Vector3.down * pressDepth;
        
        UpdatePlateVisual();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object is on a valid trigger layer
        if (IsValidTrigger(other.gameObject))
        {
            currentTriggersOnPlate++;
            
            // Press the plate if not already pressed
            if (!isPressed)
            {
                ActivatePlate();
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        // Check if the object is on a valid trigger layer
        if (IsValidTrigger(other.gameObject))
        {
            currentTriggersOnPlate--;
            
            // Release the plate if no more triggers and not set to stay pressed
            if (currentTriggersOnPlate <= 0 && !stayPressed && isPressed)
            {
                DeactivatePlate();
            }
        }
    }
    
    private bool IsValidTrigger(GameObject obj)
    {
        return ((1 << obj.layer) & triggerLayers) != 0;
    }
    
    public void ActivatePlate()
    {
        if (isPressed) return;
        
        isPressed = true;
        currentTriggersOnPlate = Mathf.Max(1, currentTriggersOnPlate);
        
        UpdatePlateVisual();
        PlaySound(pressSound);
        
        // Notify the controller
        if (controller != null)
        {
            controller.OnPlateActivated(this);
        }
        
        Debug.Log($"Pressure plate {plateID} activated!");
    }
    
    public void DeactivatePlate()
    {
        if (!isPressed) return;
        
        isPressed = false;
        currentTriggersOnPlate = 0;
        
        UpdatePlateVisual();
        PlaySound(releaseSound);
        
        // Notify the controller
        if (controller != null)
        {
            controller.OnPlateDeactivated(this);
        }
        
        Debug.Log($"Pressure plate {plateID} deactivated!");
    }
    
    public void ForceActivate()
    {
        ActivatePlate();
    }
    
    public void ForceDeactivate()
    {
        if (!stayPressed)
        {
            DeactivatePlate();
        }
    }
    
    private void UpdatePlateVisual()
    {
        if (spriteRenderer != null)
        {
            // Update sprite
            if (isPressed && plateDownSprite != null)
            {
                spriteRenderer.sprite = plateDownSprite;
            }
            else if (!isPressed && plateUpSprite != null)
            {
                spriteRenderer.sprite = plateUpSprite;
            }
            
            // Update color
            spriteRenderer.color = isPressed ? plateDownColor : plateUpColor;
        }
        
        // Animate position
        transform.position = isPressed ? pressedPosition : originalPosition;
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isPressed ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.8f);
        
        // Show plate ID
        Gizmos.color = Color.white;
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, $"Plate {plateID}");
        #endif
    }
}
