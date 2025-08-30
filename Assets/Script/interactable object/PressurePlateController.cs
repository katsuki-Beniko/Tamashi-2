using UnityEngine;
using System.Collections.Generic;

public class PressurePlateController : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    public PuzzleType puzzleType = PuzzleType.AllPlates;
    
    [Header("Sequence Settings (for Sequential puzzles)")]
    public int[] requiredSequence = {1, 2, 3}; // Example: press plates 1, 2, then 3
    public bool allowRepeats = false; // Can press the same plate multiple times in sequence
    public float sequenceTimeout = 5f; // Time before sequence resets (0 = no timeout)
    
    [Header("Connected Objects")]
    public GameObject[] objectsToActivate; // Regular GameObjects to activate
    public SceneTransitionDoor[] doorsToOpen; // Scene doors to open
    public Lever[] leversToActivate; // Levers to activate
    
    [Header("Audio")]
    public AudioClip puzzleCompleteSound;
    public AudioClip puzzleFailSound;
    public AudioClip sequenceResetSound;
    
    private PressurePlate[] allPlates;
    private List<int> currentSequence = new List<int>();
    private bool puzzleCompleted = false;
    private float lastActivationTime;
    private AudioSource audioSource;
    
    public enum PuzzleType
    {
        AllPlates,      // All plates must be pressed (any order)
        AnyPlate,       // Any single plate activates the puzzle
        Sequential,     // Plates must be pressed in specific order
        Simultaneous    // All plates must be pressed at the same time
    }
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Find all pressure plates in children
        allPlates = GetComponentsInChildren<PressurePlate>();
        
        // Initialize
        ResetPuzzle();
        
        Debug.Log($"Pressure Plate Controller initialized with {allPlates.Length} plates. Puzzle type: {puzzleType}");
    }
    
    void Update()
    {
        // Handle sequence timeout
        if (puzzleType == PuzzleType.Sequential && sequenceTimeout > 0 && currentSequence.Count > 0)
        {
            if (Time.time - lastActivationTime > sequenceTimeout)
            {
                Debug.Log("Sequence timed out! Resetting...");
                ResetSequence();
            }
        }
    }
    
    public void OnPlateActivated(PressurePlate plate)
    {
        if (puzzleCompleted) return;
        
        lastActivationTime = Time.time;
        
        switch (puzzleType)
        {
            case PuzzleType.AllPlates:
                CheckAllPlatesPuzzle();
                break;
                
            case PuzzleType.AnyPlate:
                CompletePuzzle();
                break;
                
            case PuzzleType.Sequential:
                HandleSequentialActivation(plate);
                break;
                
            case PuzzleType.Simultaneous:
                CheckSimultaneousPuzzle();
                break;
        }
    }
    
    public void OnPlateDeactivated(PressurePlate plate)
    {
        if (puzzleCompleted) return;
        
        switch (puzzleType)
        {
            case PuzzleType.AllPlates:
                // Check if puzzle is still complete
                CheckAllPlatesPuzzle();
                break;
                
            case PuzzleType.Simultaneous:
                // All plates must be pressed simultaneously
                CheckSimultaneousPuzzle();
                break;
                
            // Sequential and AnyPlate don't care about deactivation
        }
    }
    
    private void CheckAllPlatesPuzzle()
    {
        bool allPressed = true;
        foreach (PressurePlate plate in allPlates)
        {
            if (!plate.isPressed)
            {
                allPressed = false;
                break;
            }
        }
        
        if (allPressed && !puzzleCompleted)
        {
            CompletePuzzle();
        }
        else if (!allPressed && puzzleCompleted)
        {
            // Puzzle was completed but now a plate is released
            // Decide if you want to deactivate or keep completed
            // For now, keeping it completed once solved
        }
    }
    
    private void CheckSimultaneousPuzzle()
    {
        bool allPressed = true;
        foreach (PressurePlate plate in allPlates)
        {
            if (!plate.isPressed)
            {
                allPressed = false;
                break;
            }
        }
        
        if (allPressed && !puzzleCompleted)
        {
            CompletePuzzle();
        }
    }
    
    private void HandleSequentialActivation(PressurePlate plate)
    {
        int plateID = plate.plateID;
        
        // Check if this plate is the next in the sequence
        if (currentSequence.Count < requiredSequence.Length)
        {
            int expectedPlateID = requiredSequence[currentSequence.Count];
            
            if (plateID == expectedPlateID)
            {
                // Correct plate in sequence
                if (!allowRepeats && currentSequence.Contains(plateID))
                {
                    Debug.Log($"Plate {plateID} already used in sequence! Resetting...");
                    FailSequence();
                    return;
                }
                
                currentSequence.Add(plateID);
                Debug.Log($"Correct! Sequence progress: {string.Join(" -> ", currentSequence)}");
                
                // Check if sequence is complete
                if (currentSequence.Count >= requiredSequence.Length)
                {
                    CompletePuzzle();
                }
            }
            else
            {
                Debug.Log($"Wrong plate! Expected {expectedPlateID}, got {plateID}. Resetting sequence...");
                FailSequence();
            }
        }
    }
    
    private void FailSequence()
    {
        PlaySound(puzzleFailSound);
        ResetSequence();
    }
    
    private void ResetSequence()
    {
        currentSequence.Clear();
        lastActivationTime = Time.time;
        PlaySound(sequenceResetSound);
        Debug.Log("Sequence reset!");
    }
    
    private void CompletePuzzle()
    {
        if (puzzleCompleted) return;
        
        puzzleCompleted = true;
        PlaySound(puzzleCompleteSound);
        
        // Activate connected objects
        ActivateConnectedObjects();
        
        Debug.Log("Puzzle completed! All connected objects activated.");
    }
    
    private void ActivateConnectedObjects()
    {
        // Activate regular GameObjects
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
        
        // Open scene doors
        foreach (SceneTransitionDoor door in doorsToOpen)
        {
            if (door != null)
            {
                door.SetDoorOpen(true);
            }
        }
        
        // Activate levers
        foreach (Lever lever in leversToActivate)
        {
            if (lever != null)
            {
                lever.SetLeverState(true);
            }
        }
    }
    
    public void ResetPuzzle()
    {
        puzzleCompleted = false;
        currentSequence.Clear();
        lastActivationTime = Time.time;
        
        // Reset all plates if needed
        if (puzzleType == PuzzleType.Sequential)
        {
            foreach (PressurePlate plate in allPlates)
            {
                if (!plate.stayPressed)
                {
                    plate.DeactivatePlate();
                }
            }
        }
        
        Debug.Log("Puzzle reset!");
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // Public methods for external control
    public bool IsPuzzleCompleted()
    {
        return puzzleCompleted;
    }
    
    public List<int> GetCurrentSequence()
    {
        return new List<int>(currentSequence);
    }
    
    public int[] GetRequiredSequence()
    {
        return requiredSequence;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = puzzleCompleted ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
        
        #if UNITY_EDITOR
        // Show puzzle type and sequence
        string info = $"Type: {puzzleType}";
        if (puzzleType == PuzzleType.Sequential)
        {
            info += $"\nSequence: {string.Join(" -> ", requiredSequence)}";
            if (Application.isPlaying)
            {
                info += $"\nProgress: {string.Join(" -> ", currentSequence)}";
            }
        }
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, info);
        #endif
    }
}
