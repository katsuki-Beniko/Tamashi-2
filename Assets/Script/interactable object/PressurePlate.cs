using UnityEngine;
using System.Collections;

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
    
    [Header("Individual Object Destruction")]
    public GameObject[] objectsToDestroy; // Objects this plate will destroy when activated
    public bool destroyOnActivation = true; // Destroy when pressed
    public bool destroyOnDeactivation = false; // Destroy when released
    public float destructionDelay = 0.0f; // Delay before destruction
    
    [Header("Destruction Effects")]
    public bool fadeOutBeforeDestroy = true;
    public float fadeOutDuration = 0.5f;
    public bool shakeBeforeDestroy = false;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.3f;
    
    [Header("Audio")]
    public AudioClip pressSound;
    public AudioClip releaseSound;
    public AudioClip destructionSound;
    
    [Header("Detection")]
    public LayerMask triggerLayers = -1; // What can trigger this plate (players, objects, etc.)
    
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Vector3 originalPosition;
    private Vector3 pressedPosition;
    private PressurePlateController controller;
    private int currentTriggersOnPlate = 0;
    private bool hasDestroyedObjects = false;
    
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
        
        // NEW: Destroy objects when activated
        if (destroyOnActivation && objectsToDestroy.Length > 0 && !hasDestroyedObjects)
        {
            DestroyTargetObjects();
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
        
        // NEW: Destroy objects when deactivated (optional)
        if (destroyOnDeactivation && objectsToDestroy.Length > 0 && !hasDestroyedObjects)
        {
            DestroyTargetObjects();
        }
        
        Debug.Log($"Pressure plate {plateID} deactivated!");
    }
    
    // NEW: Individual object destruction system
    private void DestroyTargetObjects()
    {
        if (hasDestroyedObjects) return; // Prevent multiple destructions
        
        hasDestroyedObjects = true;
        
        Debug.Log($"Pressure plate {plateID} destroying {objectsToDestroy.Length} target objects...");
        
        if (destructionDelay > 0)
        {
            Invoke(nameof(ExecuteDestruction), destructionDelay);
        }
        else
        {
            ExecuteDestruction();
        }
    }
    
    private void ExecuteDestruction()
    {
        if (fadeOutBeforeDestroy || shakeBeforeDestroy)
        {
            StartCoroutine(DestroyWithEffects());
        }
        else
        {
            DestroyObjectsImmediately();
        }
    }
    
    private IEnumerator DestroyWithEffects()
    {
        System.Collections.Generic.List<SpriteRenderer> renderers = new System.Collections.Generic.List<SpriteRenderer>();
        System.Collections.Generic.List<Color> originalColors = new System.Collections.Generic.List<Color>();
        
        // Collect renderers and start shaking
        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
            {
                SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderers.Add(renderer);
                    originalColors.Add(renderer.color);
                }
                
                // Start shaking
                if (shakeBeforeDestroy)
                {
                    StartCoroutine(ShakeObject(obj, shakeIntensity, shakeDuration));
                }
            }
        }
        
        // Fade out
        if (fadeOutBeforeDestroy && renderers.Count > 0)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
                
                for (int i = 0; i < renderers.Count; i++)
                {
                    if (renderers[i] != null)
                    {
                        Color color = originalColors[i];
                        color.a = alpha;
                        renderers[i].color = color;
                    }
                }
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        
        // Wait for shake to finish
        if (shakeBeforeDestroy)
        {
            yield return new WaitForSeconds(shakeDuration);
        }
        
        DestroyObjectsImmediately();
    }
    
    private IEnumerator ShakeObject(GameObject obj, float intensity, float duration)
    {
        if (obj == null) yield break;
        
        Vector3 originalPos = obj.transform.position;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity),
                0f
            );
            
            obj.transform.position = originalPos + randomOffset;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        if (obj != null)
        {
            obj.transform.position = originalPos;
        }
    }
    
    private void DestroyObjectsImmediately()
    {
        PlaySound(destructionSound);
        
        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
            {
                Debug.Log($"🗑️ Pressure plate {plateID} destroyed: {obj.name}");
                Destroy(obj);
            }
        }
        
        Debug.Log($"Pressure plate {plateID} destruction complete!");
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
    
    // Public methods for external control
    public void ManualDestroyObjects()
    {
        DestroyTargetObjects();
    }
    
    public void ResetDestructionState()
    {
        hasDestroyedObjects = false;
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
        
        // Draw lines to destruction targets
        Gizmos.color = Color.red;
        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
            {
                Gizmos.DrawLine(transform.position, obj.transform.position);
                Gizmos.DrawWireCube(obj.transform.position, Vector3.one * 0.3f);
            }
        }
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, $"Plate {plateID}\nTargets: {objectsToDestroy.Length}");
        #endif
    }
}
