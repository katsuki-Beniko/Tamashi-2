using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MultiComponentPuzzleController : MonoBehaviour
{
    [Header("Puzzle Requirements")]
    public bool requireAllPressurePlates = true;
    public bool requireAllButtons = true;
    public bool requireAllLevers = false;
    
    [Header("Puzzle Components")]
    public PressurePlate[] requiredPressurePlates;
    public InteractiveButton[] requiredButtons;
    public GameObject[] requiredLevers; // Using GameObject instead of Lever class
    
    [Header("Connected Objects")]
    public GameObject[] objectsToActivate;
    public GameObject[] objectsToDeactivate;
    public GameObject[] doorsToOpen; // Using GameObject instead of SceneTransitionDoor
    
    [Header("Audio")]
    public AudioClip puzzleCompleteSound;
    public AudioClip componentActivatedSound;
    
    [Header("Visual Feedback")]
    public bool showProgress = true;
    public float checkInterval = 0.1f; // How often to check component states
    
    private HashSet<PressurePlate> activatedPlates = new HashSet<PressurePlate>();
    private HashSet<InteractiveButton> activatedButtons = new HashSet<InteractiveButton>();
    private HashSet<GameObject> activatedLevers = new HashSet<GameObject>();
    
    private bool puzzleCompleted = false;
    private AudioSource audioSource;
    
    // Track previous states for change detection
    private bool[] previousPlateStates;
    private bool[] previousButtonStates;
    private bool[] previousLeverStates;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Auto-find components if arrays are empty
        if (requiredPressurePlates.Length == 0)
            requiredPressurePlates = GetComponentsInChildren<PressurePlate>();
            
        if (requiredButtons.Length == 0)
            requiredButtons = GetComponentsInChildren<InteractiveButton>();
        
        // Initialize state tracking arrays
        InitializeStateTracking();
        
        // Set up InteractiveButton events (these have events)
        RegisterButtonEvents();
        
        // Start checking component states
        InvokeRepeating(nameof(CheckComponentStates), 0f, checkInterval);
        
        Debug.Log($"Multi-Component Puzzle initialized: {requiredPressurePlates.Length} plates, {requiredButtons.Length} buttons, {requiredLevers.Length} levers");
    }
    
    private void InitializeStateTracking()
    {
        previousPlateStates = new bool[requiredPressurePlates.Length];
        previousButtonStates = new bool[requiredButtons.Length];
        previousLeverStates = new bool[requiredLevers.Length];
        
        // Initialize with current states
        for (int i = 0; i < requiredPressurePlates.Length; i++)
        {
            if (requiredPressurePlates[i] != null)
                previousPlateStates[i] = requiredPressurePlates[i].isPressed;
        }
        
        for (int i = 0; i < requiredButtons.Length; i++)
        {
            if (requiredButtons[i] != null)
                previousButtonStates[i] = requiredButtons[i].IsActivated();
        }
        
        // Initialize lever states (you can customize this based on your lever implementation)
        for (int i = 0; i < requiredLevers.Length; i++)
        {
            previousLeverStates[i] = false;
        }
    }
    
    private void RegisterButtonEvents()
    {
        // Register button events (InteractiveButton has events)
        foreach (InteractiveButton button in requiredButtons)
        {
            if (button != null)
            {
                button.OnButtonActivated += OnButtonActivated;
                button.OnButtonDeactivated += OnButtonDeactivated;
            }
        }
    }
    
    private void CheckComponentStates()
    {
        CheckPressurePlateStates();
        CheckLeverStates();
        CheckPuzzleCompletion();
    }
    
    private void CheckPressurePlateStates()
    {
        for (int i = 0; i < requiredPressurePlates.Length; i++)
        {
            if (requiredPressurePlates[i] != null)
            {
                bool currentState = requiredPressurePlates[i].isPressed;
                bool previousState = previousPlateStates[i];
                
                // Check for state changes
                if (currentState != previousState)
                {
                    previousPlateStates[i] = currentState;
                    
                    if (currentState)
                    {
                        OnPlateActivated(requiredPressurePlates[i]);
                    }
                    else
                    {
                        OnPlateDeactivated(requiredPressurePlates[i]);
                    }
                }
            }
        }
    }
    
    private void CheckLeverStates()
    {
        // Implement lever state checking based on your lever system
        // This is a placeholder - customize based on your lever implementation
        for (int i = 0; i < requiredLevers.Length; i++)
        {
            if (requiredLevers[i] != null)
            {
                // Example: Check if lever GameObject is active, or has a specific component state
                bool currentState = requiredLevers[i].activeInHierarchy;
                bool previousState = previousLeverStates[i];
                
                if (currentState != previousState)
                {
                    previousLeverStates[i] = currentState;
                    
                    if (currentState)
                    {
                        OnLeverActivated(requiredLevers[i]);
                    }
                    else
                    {
                        OnLeverDeactivated(requiredLevers[i]);
                    }
                }
            }
        }
    }
    
    // Pressure Plate Events (called by state checking)
    private void OnPlateActivated(PressurePlate plate)
    {
        activatedPlates.Add(plate);
        PlaySound(componentActivatedSound);
        Debug.Log($"Pressure plate {plate.name} activated. Progress: {activatedPlates.Count}/{requiredPressurePlates.Length}");
    }
    
    private void OnPlateDeactivated(PressurePlate plate)
    {
        activatedPlates.Remove(plate);
        Debug.Log($"Pressure plate {plate.name} deactivated. Progress: {activatedPlates.Count}/{requiredPressurePlates.Length}");
    }
    
    // Button Events (called by InteractiveButton events)
    private void OnButtonActivated(InteractiveButton button)
    {
        activatedButtons.Add(button);
        PlaySound(componentActivatedSound);
        Debug.Log($"Button {button.name} activated. Progress: {activatedButtons.Count}/{requiredButtons.Length}");
    }
    
    private void OnButtonDeactivated(InteractiveButton button)
    {
        activatedButtons.Remove(button);
        Debug.Log($"Button {button.name} deactivated. Progress: {activatedButtons.Count}/{requiredButtons.Length}");
    }
    
    // Lever Events (called by state checking)
    private void OnLeverActivated(GameObject lever)
    {
        activatedLevers.Add(lever);
        PlaySound(componentActivatedSound);
        Debug.Log($"Lever {lever.name} activated. Progress: {activatedLevers.Count}/{requiredLevers.Length}");
    }
    
    private void OnLeverDeactivated(GameObject lever)
    {
        activatedLevers.Remove(lever);
        Debug.Log($"Lever {lever.name} deactivated. Progress: {activatedLevers.Count}/{requiredLevers.Length}");
    }
    
    private void CheckPuzzleCompletion()
    {
        bool platesComplete = !requireAllPressurePlates || activatedPlates.Count >= requiredPressurePlates.Length;
        bool buttonsComplete = !requireAllButtons || activatedButtons.Count >= requiredButtons.Length;
        bool leversComplete = !requireAllLevers || activatedLevers.Count >= requiredLevers.Length;
        
        bool allComplete = platesComplete && buttonsComplete && leversComplete;
        
        if (allComplete && !puzzleCompleted)
        {
            CompletePuzzle();
        }
        else if (!allComplete && puzzleCompleted)
        {
            // Optionally reset puzzle if components are deactivated
            // ResetPuzzle();
        }
        
        if (showProgress)
        {
            ShowProgress();
        }
    }
    
    private void ShowProgress()
    {
        string progress = "Puzzle Progress: ";
        
        if (requireAllPressurePlates)
            progress += $"Plates: {activatedPlates.Count}/{requiredPressurePlates.Length} ";
            
        if (requireAllButtons)
            progress += $"Buttons: {activatedButtons.Count}/{requiredButtons.Length} ";
            
        if (requireAllLevers)
            progress += $"Levers: {activatedLevers.Count}/{requiredLevers.Length}";
        
        Debug.Log(progress);
    }
    
    private void CompletePuzzle()
    {
        puzzleCompleted = true;
        PlaySound(puzzleCompleteSound);
        
        // Activate connected objects
        ActivateConnectedObjects();
        
        Debug.Log("🎉 Multi-Component Puzzle COMPLETED! All requirements satisfied.");
    }
    
    private void ActivateConnectedObjects()
    {
        // Activate GameObjects
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                Debug.Log($"Activated: {obj.name}");
            }
        }
        
        // Deactivate GameObjects
        foreach (GameObject obj in objectsToDeactivate)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Debug.Log($"Deactivated: {obj.name}");
            }
        }
        
        // Handle doors (assuming they have a method to open or are just GameObjects)
        foreach (GameObject door in doorsToOpen)
        {
            if (door != null)
            {
                // Option 1: If doors are just GameObjects, activate them
                door.SetActive(true);
                
                // Option 2: If doors have a specific script, uncomment and customize:
                // SceneTransitionDoor doorScript = door.GetComponent<SceneTransitionDoor>();
                // if (doorScript != null) doorScript.SetDoorOpen(true);
                
                Debug.Log($"Opened door: {door.name}");
            }
        }
    }
    
    public void ResetPuzzle()
    {
        puzzleCompleted = false;
        activatedPlates.Clear();
        activatedButtons.Clear();
        activatedLevers.Clear();
        
        // Reset state tracking
        InitializeStateTracking();
        
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
    public bool IsPuzzleCompleted() => puzzleCompleted;
    
    public float GetCompletionPercentage()
    {
        int totalRequired = 0;
        int totalActivated = 0;
        
        if (requireAllPressurePlates)
        {
            totalRequired += requiredPressurePlates.Length;
            totalActivated += activatedPlates.Count;
        }
        
        if (requireAllButtons)
        {
            totalRequired += requiredButtons.Length;
            totalActivated += activatedButtons.Count;
        }
        
        if (requireAllLevers)
        {
            totalRequired += requiredLevers.Length;
            totalActivated += activatedLevers.Count;
        }
        
        return totalRequired > 0 ? (float)totalActivated / totalRequired : 1f;
    }
    
    void OnDestroy()
    {
        // Stop the repeating invoke
        CancelInvoke(nameof(CheckComponentStates));
        
        // Unregister button events
        foreach (InteractiveButton button in requiredButtons)
        {
            if (button != null)
            {
                button.OnButtonActivated -= OnButtonActivated;
                button.OnButtonDeactivated -= OnButtonDeactivated;
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = puzzleCompleted ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 3f);
        
        #if UNITY_EDITOR
        string info = "Multi-Component Puzzle\n";
        if (requireAllPressurePlates) info += $"Plates: {(activatedPlates?.Count ?? 0)}/{requiredPressurePlates?.Length ?? 0}\n";
        if (requireAllButtons) info += $"Buttons: {(activatedButtons?.Count ?? 0)}/{requiredButtons?.Length ?? 0}\n";
        if (requireAllLevers) info += $"Levers: {(activatedLevers?.Count ?? 0)}/{requiredLevers?.Length ?? 0}";
        
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, info);
        #endif
    }
}
