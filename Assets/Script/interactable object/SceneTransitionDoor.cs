using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionDoor : MonoBehaviour, IInteractable
{
    [Header("Scene Transition")]
    [SceneDropdown]
    public string targetSceneName = "";
    public bool useSceneName = true; // If false, uses build index instead
    
    [Header("Door State")]
    public bool isDoorOpen = false;
    public bool requiresLeverToOpen = true;
    
    [Header("Visual Feedback")]
    public Sprite doorClosedSprite;
    public Sprite doorOpenSprite;
    public Color doorClosedColor = Color.red;
    public Color doorOpenColor = Color.green;
    
    [Header("Audio")]
    public AudioClip doorOpenSound;
    public AudioClip doorLockedSound;
    public AudioClip sceneTransitionSound;
    
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        UpdateDoorVisual();
    }
    
    public void Interact(Player player)
    {
        if (!isDoorOpen && requiresLeverToOpen)
        {
            // Door is locked, can't use it
            Debug.Log("The door is locked. Find a way to open it!");
            PlaySound(doorLockedSound);
            return;
        }
        
        if (!isDoorOpen)
        {
            Debug.Log("The door is closed.");
            return;
        }
        
        // Door is open, transition to new scene
        TransitionToScene();
    }
    
    private void TransitionToScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("No target scene selected!");
            return;
        }
        
        Debug.Log($"Transitioning to scene: {targetSceneName}");
        
        // Play transition sound
        PlaySound(sceneTransitionSound);
        
        // Load the new scene
        try
        {
            SceneManager.LoadScene(targetSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene '{targetSceneName}': {e.Message}");
        }
    }
    
    public void SetDoorOpen(bool open)
    {
        bool wasOpen = isDoorOpen;
        isDoorOpen = open;
        
        UpdateDoorVisual();
        
        // Play door open sound if door just opened
        if (!wasOpen && open)
        {
            PlaySound(doorOpenSound);
            Debug.Log("Door opened!");
        }
        else if (wasOpen && !open)
        {
            Debug.Log("Door closed!");
        }
    }
    
    private void UpdateDoorVisual()
    {
        if (spriteRenderer != null)
        {
            // Update sprite
            if (isDoorOpen && doorOpenSprite != null)
            {
                spriteRenderer.sprite = doorOpenSprite;
            }
            else if (!isDoorOpen && doorClosedSprite != null)
            {
                spriteRenderer.sprite = doorClosedSprite;
            }
            
            // Update color
            spriteRenderer.color = isDoorOpen ? doorOpenColor : doorClosedColor;
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // For debugging in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isDoorOpen ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
        
        if (isDoorOpen)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1.5f);
        }
    }
}
