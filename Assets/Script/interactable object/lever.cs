using UnityEngine;

public class Lever : MonoBehaviour, IInteractable
{
    [Header("Lever Settings")]
    public bool isActivated = false;
    public bool oneTimeUse = false;
    
    [Header("Connected Objects")]
    public GameObject[] doorsToOpen; // Array of doors this lever controls
    public SceneTransitionDoor[] sceneDoorsToOpen; // Array of scene doors this lever controls
    
    [Header("Visual/Audio Feedback")]
    public Sprite leverUpSprite;
    public Sprite leverDownSprite;
    public AudioClip leverSound;
    
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool hasBeenUsed = false;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        UpdateLeverVisual();
    }
    
    public void Interact(Player player)
    {
        if (oneTimeUse && hasBeenUsed)
        {
            Debug.Log("This lever has already been used.");
            return;
        }
        
        // Toggle lever state
        isActivated = !isActivated;
        
        // Update visual
        UpdateLeverVisual();
        
        // Play sound
        PlayLeverSound();
        
        // Activate/deactivate connected doors
        ControlConnectedDoors();
        
        // Mark as used if one-time use
        if (oneTimeUse)
        {
            hasBeenUsed = true;
        }
        
        Debug.Log($"Lever {(isActivated ? "activated" : "deactivated")}!");
    }
    
    private void UpdateLeverVisual()
    {
        if (spriteRenderer != null)
        {
            if (isActivated && leverDownSprite != null)
            {
                spriteRenderer.sprite = leverDownSprite;
            }
            else if (!isActivated && leverUpSprite != null)
            {
                spriteRenderer.sprite = leverUpSprite;
            }
        }
    }
    
    private void PlayLeverSound()
    {
        if (audioSource != null && leverSound != null)
        {
            audioSource.PlayOneShot(leverSound);
        }
    }
    
    private void ControlConnectedDoors()
    {
        // Control regular doors
        foreach (GameObject door in doorsToOpen)
        {
            if (door != null)
            {
                door.SetActive(isActivated);
            }
        }
        
        // Control scene transition doors
        foreach (SceneTransitionDoor sceneDoor in sceneDoorsToOpen)
        {
            if (sceneDoor != null)
            {
                sceneDoor.SetDoorOpen(isActivated);
            }
        }
    }
    
    // Method to set lever state from external scripts
    public void SetLeverState(bool activated)
    {
        isActivated = activated;
        UpdateLeverVisual();
        ControlConnectedDoors();
    }
}
