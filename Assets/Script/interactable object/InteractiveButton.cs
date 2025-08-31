using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class InteractiveButton : MonoBehaviour, IInteractable
{
    [Header("Button Settings")]
    public bool stayActivated = true; // Button stays on after activation
    public bool canToggle = false; // Can be turned off after activation
    
    [Header("Visual Feedback")]
    public GameObject activatedVisual; // GameObject to show when activated
    public Color normalColor = Color.white;
    public Color activatedColor = Color.green;
    
    [Header("Individual Object Destruction")]
    public GameObject[] objectsToDestroy; // Objects this button will destroy when activated
    public bool destroyOnActivation = true; // Destroy when pressed
    public bool destroyOnDeactivation = false; // Destroy when deactivated
    public float destructionDelay = 0.0f; // Delay before destruction
    
    [Header("Destruction Effects")]
    public bool fadeOutBeforeDestroy = true;
    public float fadeOutDuration = 0.5f;
    public bool shakeBeforeDestroy = false;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.3f;
    
    [Header("Audio")]
    public AudioClip activationSound;
    public AudioClip deactivationSound;
    public AudioClip destructionSound;
    
    // Events for puzzle system
    public System.Action<InteractiveButton> OnButtonActivated;
    public System.Action<InteractiveButton> OnButtonDeactivated;
    
    // Unity Events for additional functionality
    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;
    
    private bool isActivated = false;
    private bool hasDestroyedObjects = false;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        UpdateVisuals();
    }
    
    public void Interact(Player player)
    {
        if (!isActivated)
        {
            ActivateButton();
        }
        else if (canToggle)
        {
            DeactivateButton();
        }
        else
        {
            Debug.Log($"Button {name} is already activated and cannot be toggled");
        }
    }
    
    public void ActivateButton()
    {
        if (isActivated && stayActivated) return;
        
        isActivated = true;
        UpdateVisuals();
        PlaySound(activationSound);
        
        // Notify puzzle system
        OnButtonActivated?.Invoke(this);
        OnActivated?.Invoke();
        
        // NEW: Destroy objects when activated
        if (destroyOnActivation && objectsToDestroy.Length > 0 && !hasDestroyedObjects)
        {
            DestroyTargetObjects();
        }
        
        Debug.Log($"Interactive button {name} activated!");
        
        if (!stayActivated)
        {
            // Auto-deactivate after a short delay
            Invoke(nameof(DeactivateButton), 0.5f);
        }
    }
    
    public void DeactivateButton()
    {
        if (!isActivated) return;
        
        isActivated = false;
        UpdateVisuals();
        PlaySound(deactivationSound);
        
        // Notify puzzle system
        OnButtonDeactivated?.Invoke(this);
        OnDeactivated?.Invoke();
        
        // NEW: Destroy objects when deactivated (optional)
        if (destroyOnDeactivation && objectsToDestroy.Length > 0 && !hasDestroyedObjects)
        {
            DestroyTargetObjects();
        }
        
        Debug.Log($"Interactive button {name} deactivated!");
    }
    
    // NEW: Individual object destruction system
    private void DestroyTargetObjects()
    {
        if (hasDestroyedObjects) return; // Prevent multiple destructions
        
        hasDestroyedObjects = true;
        
        Debug.Log($"Interactive button {name} destroying {objectsToDestroy.Length} target objects...");
        
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
                Debug.Log($"🗑️ Interactive button {name} destroyed: {obj.name}");
                Destroy(obj);
            }
        }
        
        Debug.Log($"Interactive button {name} destruction complete!");
    }
    
    private void UpdateVisuals()
    {
        // Update sprite color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isActivated ? activatedColor : normalColor;
        }
        
        // Update activated visual
        if (activatedVisual != null)
        {
            activatedVisual.SetActive(isActivated);
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // Public methods
    public bool IsActivated() => isActivated;
    
    public void SetActivated(bool activated)
    {
        if (activated)
            ActivateButton();
        else
            DeactivateButton();
    }
    
    public void ManualDestroyObjects()
    {
        DestroyTargetObjects();
    }
    
    public void ResetDestructionState()
    {
        hasDestroyedObjects = false;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isActivated ? Color.green : Color.yellow;
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
        string status = isActivated ? "ACTIVATED" : "INACTIVE";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, $"Button: {status}\nTargets: {objectsToDestroy.Length}");
        #endif
    }
}
