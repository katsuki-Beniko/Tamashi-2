using UnityEngine;
using UnityEngine.Events;

public class InteractiveButton : MonoBehaviour, IInteractable
{
    [Header("Button Settings")]
    public bool stayActivated = true; // Button stays on after activation
    public bool canToggle = false; // Can be turned off after activation
    
    [Header("Visual Feedback")]
    public GameObject activatedVisual; // GameObject to show when activated
    public Color normalColor = Color.white;
    public Color activatedColor = Color.green;
    
    [Header("Audio")]
    public AudioClip activationSound;
    public AudioClip deactivationSound;
    
    // Events for puzzle system
    public System.Action<InteractiveButton> OnButtonActivated;
    public System.Action<InteractiveButton> OnButtonDeactivated;
    
    // Unity Events for additional functionality
    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;
    
    private bool isActivated = false;
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
        
        Debug.Log($"Interactive button {name} deactivated!");
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
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isActivated ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.8f);
        
        #if UNITY_EDITOR
        string status = isActivated ? "ACTIVATED" : "INACTIVE";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, $"Button: {status}");
        #endif
    }
}
