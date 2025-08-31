using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[System.Serializable]
public class BoxResetData
{
    public GameObject box;
    public Vector3 resetPosition;
    public bool useCurrentPosition = true; // If true, uses box's current position as reset position
    
    [Header("Position Override")]
    public bool overridePosition = false; // If true, uses resetPosition instead of current
}

public class BoxRobotResetSystem : MonoBehaviour, IInteractable
{
    [Header("Right Side Reset (Button)")]
    public GameObject rightBox;
    public GameObject robot;
    public Vector3 manualRightBoxPos = new Vector3(10.49f, -3.45f, 0f);
    public Vector3 manualRobotPos = new Vector3(11.537f, -2.429f, 0f);
    
    [Header("Left Side Reset (Button 1) - Multiple Boxes")]
    public BoxResetData[] leftSideBoxes; // List of boxes to reset for Button (1)
    public GameObject robot2;
    public Vector3 manualRobot2Pos = new Vector3(-7.516f, -7.503f, 0f);
    
    [Header("Reset Configuration")]
    public bool isRightSideButton = true; // True for Button, False for Button (1)
    public bool useManualPositions = true;
    
    [Header("Audio")]
    public AudioClip resetSound;
    
    // Storage for initial positions
    private Vector3 rightBoxInitialPos;
    private Vector3 robotInitialPos;
    private Vector3 robot2InitialPos;
    private Dictionary<GameObject, Vector3> leftBoxInitialPositions = new Dictionary<GameObject, Vector3>();
    
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Auto-find objects if not assigned
        AutoFindObjects();
        
        // Store initial positions
        StoreInitialPositions();
        
        // Check for problematic constraints
        CheckConstraints();
    }
    
    private void AutoFindObjects()
    {
        // Find robots if not assigned
        if (rightBox == null) rightBox = GameObject.Find("right box");
        if (robot == null) robot = GameObject.Find("robot");
        if (robot2 == null) robot2 = GameObject.Find("robot2");
        
        // Auto-populate left side boxes if array is empty
        if (leftSideBoxes == null || leftSideBoxes.Length == 0)
        {
            // Find common box objects (you can customize this logic)
            List<BoxResetData> foundBoxes = new List<BoxResetData>();
            
            // Look for "left box"
            GameObject leftBox = GameObject.Find("left box");
            if (leftBox != null)
            {
                foundBoxes.Add(new BoxResetData { box = leftBox, useCurrentPosition = true });
            }
            
            // Look for boxes in the Boxes parent
            GameObject boxesParent = GameObject.Find("Boxes");
            if (boxesParent != null)
            {
                for (int i = 0; i < boxesParent.transform.childCount; i++)
                {
                    GameObject box = boxesParent.transform.GetChild(i).gameObject;
                    
                    // Add boxes that aren't the right box
                    if (box != rightBox && box.name.ToLower().Contains("square"))
                    {
                        foundBoxes.Add(new BoxResetData { box = box, useCurrentPosition = true });
                    }
                }
            }
            
            leftSideBoxes = foundBoxes.ToArray();
            Debug.Log($"Auto-found {leftSideBoxes.Length} boxes for left side reset");
        }
    }
    
    private void StoreInitialPositions()
    {
        // Store right side positions
        if (useManualPositions)
        {
            rightBoxInitialPos = manualRightBoxPos;
            robotInitialPos = manualRobotPos;
            robot2InitialPos = manualRobot2Pos;
            
            Debug.Log("Using manual reset positions for right side and robot2");
        }
        else
        {
            if (rightBox != null) rightBoxInitialPos = rightBox.transform.position;
            if (robot != null) robotInitialPos = robot.transform.position;
            if (robot2 != null) robot2InitialPos = robot2.transform.position;
        }
        
        // Store left side box positions
        leftBoxInitialPositions.Clear();
        foreach (BoxResetData boxData in leftSideBoxes)
        {
            if (boxData.box != null)
            {
                Vector3 resetPos;
                
                if (boxData.overridePosition)
                {
                    resetPos = boxData.resetPosition;
                    Debug.Log($"Using override position for {boxData.box.name}: {resetPos}");
                }
                else if (boxData.useCurrentPosition)
                {
                    resetPos = boxData.box.transform.position;
                    Debug.Log($"Using current position for {boxData.box.name}: {resetPos}");
                }
                else
                {
                    resetPos = boxData.resetPosition;
                    Debug.Log($"Using set reset position for {boxData.box.name}: {resetPos}");
                }
                
                leftBoxInitialPositions[boxData.box] = resetPos;
            }
        }
        
        Debug.Log($"Stored reset positions for {leftBoxInitialPositions.Count} left side boxes");
    }
    
    private void CheckConstraints()
    {
        CheckObjectConstraints(rightBox, "right box");
        CheckObjectConstraints(robot, "robot");
        CheckObjectConstraints(robot2, "robot2");
        
        // Check all left side boxes
        foreach (BoxResetData boxData in leftSideBoxes)
        {
            if (boxData.box != null)
            {
                CheckObjectConstraints(boxData.box, boxData.box.name);
            }
        }
    }
    
    private void CheckObjectConstraints(GameObject obj, string name)
    {
        if (obj != null)
        {
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if (rb.constraints.HasFlag(RigidbodyConstraints2D.FreezeAll))
                {
                    Debug.LogError($"⚠️ {name} has FreezeAll constraint! This BLOCKS reset. Please remove FreezeAll and keep only FreezeRotation.");
                }
                else
                {
                    Debug.Log($"✅ {name} constraints are OK for reset");
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ {name} has no Rigidbody2D - position reset only");
            }
        }
    }
    
    public void Interact(Player player)
    {
        if (isRightSideButton)
        {
            ResetRightSide();
        }
        else
        {
            ResetLeftSide();
        }
    }
    
    // Called by Button - resets right box and robot
    public void ResetRightSide()
    {
        Debug.Log("Button pressed - Resetting right box and robot");
        
        // Reset right box
        if (rightBox != null)
        {
            ResetObjectPosition(rightBox, rightBoxInitialPos, "right box");
        }
        
        // Reset robot
        if (robot != null)
        {
            ResetObjectPosition(robot, robotInitialPos, "robot");
        }
        
        PlayResetSound();
        Debug.Log("Right side reset completed");
    }
    
    // Called by Button (1) - resets multiple left boxes and robot2
    public void ResetLeftSide()
    {
        Debug.Log("Button (1) pressed - Resetting left side boxes and robot2");
        
        int resetCount = 0;
        
        // Reset all left side boxes
        foreach (BoxResetData boxData in leftSideBoxes)
        {
            if (boxData.box != null && leftBoxInitialPositions.ContainsKey(boxData.box))
            {
                Vector3 resetPos = leftBoxInitialPositions[boxData.box];
                ResetObjectPosition(boxData.box, resetPos, boxData.box.name);
                resetCount++;
            }
        }
        
        // Reset robot2
        if (robot2 != null)
        {
            ResetObjectPosition(robot2, robot2InitialPos, "robot2");
            resetCount++;
        }
        
        PlayResetSound();
        Debug.Log($"Left side reset completed - Reset {resetCount} objects");
    }
    
    private void ResetObjectPosition(GameObject obj, Vector3 originalPosition, string objectName)
    {
        if (obj != null)
        {
            Debug.Log($"Attempting to reset {objectName} from {obj.transform.position} to {originalPosition}");
            
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            
            // Check for FreezeAll constraint
            if (rb != null && rb.constraints.HasFlag(RigidbodyConstraints2D.FreezeAll))
            {
                Debug.LogError($"❌ Cannot reset {objectName} - FreezeAll constraint blocks movement!");
                return;
            }
            
            // Reset position
            obj.transform.position = originalPosition;
            
            // Stop any movement
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            
            Debug.Log($"✅ Reset {objectName} to position: {originalPosition}");
        }
    }
    
    private void PlayResetSound()
    {
        if (audioSource != null && resetSound != null)
        {
            audioSource.PlayOneShot(resetSound);
        }
    }
    
    // Public methods for button events
    public void OnButtonPressed()
    {
        ResetRightSide();
    }
    
    public void OnButton1Pressed()
    {
        ResetLeftSide();
    }
    
    // Utility methods
    [ContextMenu("Update Stored Positions")]
    public void UpdateStoredPositions()
    {
        StoreInitialPositions();
        Debug.Log("Updated all stored positions!");
    }
    
    [ContextMenu("Set Current Positions as Reset Positions")]
    public void SetCurrentPositionsAsResetPositions()
    {
        // Update right side
        if (rightBox != null) manualRightBoxPos = rightBox.transform.position;
        if (robot != null) manualRobotPos = robot.transform.position;
        if (robot2 != null) manualRobot2Pos = robot2.transform.position;
        
        // Update left side boxes
        for (int i = 0; i < leftSideBoxes.Length; i++)
        {
            if (leftSideBoxes[i].box != null)
            {
                leftSideBoxes[i].resetPosition = leftSideBoxes[i].box.transform.position;
                leftSideBoxes[i].overridePosition = true;
                leftSideBoxes[i].useCurrentPosition = false;
            }
        }
        
        useManualPositions = true;
        StoreInitialPositions();
        
        Debug.Log("Set current positions as reset positions for all objects!");
    }
    
    [ContextMenu("Add Current Left Box to List")]
    public void AddCurrentLeftBoxToList()
    {
        GameObject leftBox = GameObject.Find("left box");
        if (leftBox != null)
        {
            List<BoxResetData> boxList = new List<BoxResetData>(leftSideBoxes);
            
            // Check if already in list
            bool alreadyExists = false;
            foreach (BoxResetData data in boxList)
            {
                if (data.box == leftBox)
                {
                    alreadyExists = true;
                    break;
                }
            }
            
            if (!alreadyExists)
            {
                boxList.Add(new BoxResetData 
                { 
                    box = leftBox, 
                    resetPosition = leftBox.transform.position,
                    useCurrentPosition = true 
                });
                
                leftSideBoxes = boxList.ToArray();
                Debug.Log($"Added {leftBox.name} to left side boxes list");
            }
            else
            {
                Debug.Log($"{leftBox.name} is already in the left side boxes list");
            }
        }
    }
    
    // Debug info
    void OnDrawGizmosSelected()
    {
        if (leftBoxInitialPositions != null)
        {
            Gizmos.color = Color.green;
            foreach (var kvp in leftBoxInitialPositions)
            {
                if (kvp.Key != null)
                {
                    Gizmos.DrawWireCube(kvp.Value, Vector3.one * 0.5f);
                }
            }
        }
        
        // Draw right side positions
        Gizmos.color = Color.blue;
        if (rightBox != null)
        {
            Gizmos.DrawWireCube(rightBoxInitialPos, Vector3.one * 0.5f);
        }
        if (robot != null)
        {
            Gizmos.DrawWireCube(robotInitialPos, Vector3.one * 0.3f);
        }
        if (robot2 != null)
        {
            Gizmos.DrawWireCube(robot2InitialPos, Vector3.one * 0.3f);
        }
    }
}
