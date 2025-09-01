using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Add this for new Input System

public class Playersokoban : MonoBehaviour
{
    Vector2 moveDir;
    public LayerMask detectLayer;
    
    [Header("Player Settings")]
    public bool isActive = true; // For player switching integration

    void Update()
    {
        if (!isActive) return; // Don't process input if not active
        
        // NEW INPUT SYSTEM - Use Keyboard.current instead of Input
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
            moveDir = Vector2.right;

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
            moveDir = Vector2.left;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
            moveDir = Vector2.up;

        if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
            moveDir = Vector2.down;

        if(moveDir != Vector2.zero)
        {
            if(CanMoveToDir(moveDir))
            {
                Move(moveDir);
            }
        }

        moveDir = Vector2.zero;
    }

    bool CanMoveToDir(Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 1.5f, detectLayer);

        if (!hit)
            return true;
        else
        {
            if (hit.collider.GetComponent<Box>() != null)
                return hit.collider.GetComponent<Box>().CanMoveToDir(dir);
            
        }

        return false;
    }

    void Move(Vector2 dir)
    {
        transform.Translate(dir);
    }
    
    // Public method for PlayerSwitcher integration
    public void SetActive(bool active)
    {
        isActive = active;
    }
    
    // Public method for other scripts to check if player is moving
    public bool IsMoving()
    {
        return moveDir != Vector2.zero;
    }
}
