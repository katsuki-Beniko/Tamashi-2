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
    public GameObject[] doorsToOpen;
    
    [Header("Object Destruction")]
    public GameObject[] objectsToDestroy; // NEW: Objects to destroy when puzzle is solved
    public bool destroyWithEffect = true;
    public float destructionDelay = 0.5f; // Delay before destruction
    public AudioClip destructionSound;
    
    [Header("Destruction Effects")]
    public bool fadeOutBeforeDestroy = true;
    public float fadeOutDuration = 1f;
    public bool shakeBeforeDestroy = false;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.5f;
    
    [Header("Camera Shake on Completion")]
    public bool enableCameraShake = true;
    public CameraController cameraController;
    public float cameraShakeDuration = 2.0f; // FIXED: Added missing variable
    public float shakeAmplitude = 0.3f;
    public int shakeSoftLevel = 2;
    public bool shakeDecrease = true;
    
    [Header("Audio")]
    public AudioClip puzzleCompleteSound;
    public AudioClip componentActivatedSound;
    
    [Header("Visual Feedback")]
    public bool showProgress = true;
    public float checkInterval = 0.1f;
    
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
        
        // Auto-find camera controller if not assigned
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<CameraController>();
            if (cameraController != null)
            {
                Debug.Log($"Auto-found CameraController on: {cameraController.name}");
            }
            else
            {
                Debug.LogWarning("No CameraController found! Camera shake will be disabled.");
                enableCameraShake = false;
            }
        }
        
        // Auto-find components if arrays are empty
        if (requiredPressurePlates.Length == 0)
            requiredPressurePlates = GetComponentsInChildren<PressurePlate>();
            
        if (requiredButtons.Length == 0)
            requiredButtons = GetComponentsInChildren<InteractiveButton>();
        
        // Initialize state tracking arrays
        InitializeStateTracking();
        
        // Set up InteractiveButton events
        RegisterButtonEvents();
        
        // Start checking component states
        InvokeRepeating(nameof(CheckComponentStates), 0f, checkInterval);
        
        Debug.Log($"Multi-Component Puzzle initialized: {requiredPressurePlates.Length} plates, {requiredButtons.Length} buttons, {objectsToDestroy.Length} objects to destroy");
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
        
        for (int i = 0; i < requiredLevers.Length; i++)
        {
            previousLeverStates[i] = false;
        }
    }
    
    private void RegisterButtonEvents()
    {
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
        for (int i = 0; i < requiredLevers.Length; i++)
        {
            if (requiredLevers[i] != null)
            {
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
    
    // Component Event Handlers
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
        
        Debug.Log("🎉 Multi-Component Puzzle COMPLETED! All requirements satisfied.");
        
        // Execute completion actions in order
        ActivateConnectedObjects();
        
        // NEW: Destroy objects when puzzle is completed
        if (objectsToDestroy.Length > 0)
        {
            DestroyPuzzleObjects();
        }
        
        // TRIGGER CAMERA SHAKE AFTER DOORS ARE OPENED
        TriggerCameraShake();
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
        
        // Handle doors
        foreach (GameObject door in doorsToOpen)
        {
            if (door != null)
            {
                door.SetActive(true);
                Debug.Log($"Opened door: {door.name}");
            }
        }
        
        Debug.Log($"Door opening complete - {doorsToOpen.Length} doors opened");
    }
    
    private void TriggerCameraShake()
    {
        if (enableCameraShake && cameraController != null)
        {
            // FIXED: Use cameraShakeDuration instead of shakeDuration
            cameraController.Shake(cameraShakeDuration, shakeAmplitude, shakeSoftLevel, shakeDecrease);
            Debug.Log($"🔥 PUZZLE COMPLETE CAMERA SHAKE! Duration: {cameraShakeDuration}s, Amplitude: {shakeAmplitude}");
        }
        else if (enableCameraShake)
        {
            Debug.LogWarning("Camera shake enabled but no CameraController found!");
        }
    }
    
    // NEW: Object Destruction System
    private void DestroyPuzzleObjects()
    {
        if (destroyWithEffect)
        {
            // Start destruction with effects
            StartCoroutine(DestroyObjectsWithEffects());
        }
        else
        {
            // Immediate destruction
            if (destructionDelay > 0)
            {
                Invoke(nameof(DestroyObjectsImmediately), destructionDelay);
            }
            else
            {
                DestroyObjectsImmediately();
            }
        }
    }
    
    private System.Collections.IEnumerator DestroyObjectsWithEffects()
    {
        Debug.Log($"Starting destruction sequence for {objectsToDestroy.Length} objects...");
        
        // Wait for initial delay
        if (destructionDelay > 0)
        {
            yield return new WaitForSeconds(destructionDelay);
        }
        
        List<SpriteRenderer> renderers = new List<SpriteRenderer>();
        List<Color> originalColors = new List<Color>();
        
        // Collect renderers and start shaking if enabled
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
                
                // Start shaking effect
                if (shakeBeforeDestroy)
                {
                    StartCoroutine(ShakeObject(obj, shakeIntensity, shakeDuration));
                }
            }
        }
        
        // Fade out if enabled
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
        
        // Finally destroy the objects
        DestroyObjectsImmediately();
    }
    
    private System.Collections.IEnumerator ShakeObject(GameObject obj, float intensity, float duration)
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
        
        // Return to original position
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
                Debug.Log($"🗑️ Destroying object: {obj.name}");
                Destroy(obj);
            }
        }
        
        Debug.Log($"Destroyed {objectsToDestroy.Length} objects!");
    }
    
    // Public Methods
    public void ManualDestroyObjects()
    {
        DestroyPuzzleObjects();
    }
    
    public void AddObjectToDestroy(GameObject obj)
    {
        if (obj != null)
        {
            List<GameObject> objList = new List<GameObject>(objectsToDestroy);
            objList.Add(obj);
            objectsToDestroy = objList.ToArray();
            Debug.Log($"Added {obj.name} to destruction list");
        }
    }
    
    public void ResetPuzzle()
    {
        puzzleCompleted = false;
        activatedPlates.Clear();
        activatedButtons.Clear();
        activatedLevers.Clear();
        
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
    
    // Public getters
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
        CancelInvoke(nameof(CheckComponentStates));
        
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
        
        // Draw destruction targets
        Gizmos.color = Color.red;
        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
            {
                Gizmos.DrawLine(transform.position, obj.transform.position);
                Gizmos.DrawWireCube(obj.transform.position, Vector3.one * 0.5f);
            }
        }
        
        #if UNITY_EDITOR
        string info = "Multi-Component Puzzle\n";
        if (requireAllPressurePlates) info += $"Plates: {(activatedPlates?.Count ?? 0)}/{requiredPressurePlates?.Length ?? 0}\n";
        if (requireAllButtons) info += $"Buttons: {(activatedButtons?.Count ?? 0)}/{requiredButtons?.Length ?? 0}\n";
        if (requireAllLevers) info += $"Levers: {(activatedLevers?.Count ?? 0)}/{requiredLevers?.Length ?? 0}\n";
        info += $"Objects to Destroy: {objectsToDestroy?.Length ?? 0}";
        
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, info);
        #endif
    }
}
