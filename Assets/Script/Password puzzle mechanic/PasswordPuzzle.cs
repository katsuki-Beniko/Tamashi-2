using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PasswordPuzzle : MonoBehaviour, IInteractable
{
    [Header("Puzzle Settings")]
    public string correctPassword = "1234";
    public bool oneTimeUse = true;
    
    [Header("UI References")]
    public GameObject numberPadUI;
    public NumberPadController numberPadController;
    
    [Header("Tilemap Deletion")]
    public GameObject[] tilemapsToDelete; // Drag tilemap GameObjects here
    public bool deleteImmediately = true;
    public float deletionDelay = 0.5f; // Delay before deletion (for dramatic effect)
    
    [Header("Audio")]
    public AudioClip puzzleSolvedSound;
    public AudioClip tilemapDeletionSound;
    
    [Header("Alternative Actions")]
    public GameObject[] objectsToActivate; // GameObjects to activate when solved
    public GameObject[] objectsToDeactivate; // GameObjects to deactivate when solved
    
    private bool hasBeenSolved = false;
    private AudioSource audioSource;
    
    void Start()
    {
        if (numberPadUI != null)
            numberPadUI.SetActive(false);
            
        if (numberPadController != null)
        {
            numberPadController.OnPasswordEntered += CheckPassword;
            numberPadController.OnPuzzleClosed += CloseNumberPad;
        }
        
        audioSource = GetComponent<AudioSource>();
    }
    
    public void Interact(Player player)
    {
        if (oneTimeUse && hasBeenSolved)
        {
            Debug.Log("This puzzle has already been solved.");
            return;
        }
        
        OpenNumberPad();
    }
    
    private void OpenNumberPad()
    {
        if (numberPadUI != null)
        {
            numberPadUI.SetActive(true);
            Debug.Log("Number pad opened. Enter the password!");
        }
        
        if (numberPadController != null)
        {
            numberPadController.ResetInput();
        }
    }
    
    private void CloseNumberPad()
    {
        if (numberPadUI != null)
            numberPadUI.SetActive(false);
    }
    
    private void CheckPassword(string enteredPassword)
    {
        if (enteredPassword == correctPassword)
        {
            OnPuzzleSolved();
        }
        else
        {
            OnWrongPassword();
        }
    }
    
    private void OnPuzzleSolved()
    {
        hasBeenSolved = true;
        Debug.Log("Password correct! Puzzle solved!");
        
        // Play success sound
        PlaySound(puzzleSolvedSound);
        
        // Execute additional actions
        ExecuteAdditionalActions();
        
        // Delete tilemaps
        if (tilemapsToDelete.Length > 0)
        {
            if (deleteImmediately)
            {
                DestroyTilemaps();
            }
            else
            {
                Invoke(nameof(DestroyTilemaps), deletionDelay);
            }
        }
        
        CloseNumberPad();
    }
    
    private void ExecuteAdditionalActions()
    {
        // Activate specified GameObjects
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                Debug.Log($"Activated: {obj.name}");
            }
        }
        
        // Deactivate specified GameObjects
        foreach (GameObject obj in objectsToDeactivate)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Debug.Log($"Deactivated: {obj.name}");
            }
        }
    }
    
    private void DestroyTilemaps()
    {
        PlaySound(tilemapDeletionSound);
        
        foreach (GameObject tilemapObj in tilemapsToDelete)
        {
            if (tilemapObj != null)
            {
                Debug.Log($"Deleting tilemap: {tilemapObj.name}");
                Destroy(tilemapObj);
            }
        }
        
        Debug.Log($"Deleted {tilemapsToDelete.Length} tilemaps!");
    }
    
    private void OnWrongPassword()
    {
        Debug.Log("Wrong password! Try again.");
        
        if (numberPadController != null)
        {
            numberPadController.ResetInput();
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    public void TriggerTilemapDeletion()
    {
        if (!hasBeenSolved)
        {
            hasBeenSolved = true;
            DestroyTilemaps();
        }
    }
    
    void OnDestroy()
    {
        if (numberPadController != null)
        {
            numberPadController.OnPasswordEntered -= CheckPassword;
            numberPadController.OnPuzzleClosed -= CloseNumberPad;
        }
    }
}
