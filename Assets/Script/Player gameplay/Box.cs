using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    [Header("Visual Feedback")]
    public Color finishColor = Color.green;
    Color originColor;
    
    [Header("Movement Settings")]
    public LayerMask wallLayer = -1; // What blocks movement (walls)
    public LayerMask boxLayer = -1;  // Other boxes that block movement
    
    private GameManager gameManager;
    private bool isOnTarget = false;

    private void Start()
    {
        originColor = GetComponent<SpriteRenderer>().color;
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.totalBoxs++;
        }
    }

    public bool CanMoveToDir(Vector2 dir)
    {
        Vector3 targetPosition = transform.position + (Vector3)dir;
        
        // Check for walls that should block movement
        Collider2D wallHit = Physics2D.OverlapPoint(targetPosition, wallLayer);
        if (wallHit != null)
        {
            Debug.Log($"Box movement blocked by wall: {wallHit.name}");
            return false;
        }
        
        // Check for other boxes that should block movement
        Collider2D boxHit = Physics2D.OverlapPoint(targetPosition, boxLayer);
        if (boxHit != null && boxHit.gameObject != gameObject)
        {
            Debug.Log($"Box movement blocked by another box: {boxHit.name}");
            return false;
        }
        
        // Check what's at the target position
        Collider2D[] objectsAtTarget = Physics2D.OverlapPointAll(targetPosition);
        
        foreach (Collider2D obj in objectsAtTarget)
        {
            // Skip self
            if (obj.gameObject == gameObject) continue;
            
            // Allow movement onto pressure plates (tagged as "Target")
            if (obj.CompareTag("Target"))
            {
                Debug.Log($"Box can move onto pressure plate: {obj.name}");
                continue; // This is allowed
            }
            
            // Allow movement onto triggers (pressure plates might be triggers)
            if (obj.isTrigger)
            {
                Debug.Log($"Box can move onto trigger: {obj.name}");
                continue; // This is allowed
            }
            
            // Block movement onto other solid objects
            if (!obj.isTrigger && obj.GetComponent<Box>() == null)
            {
                Debug.Log($"Box movement blocked by solid object: {obj.name}");
                return false;
            }
        }
        
        // Movement is allowed - move the box
        transform.Translate(dir);
        Debug.Log($"Box moved to {targetPosition}");
        return true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Target") && !isOnTarget)
        {
            isOnTarget = true;
            if (gameManager != null)
            {
                gameManager.finishedBoxs++;
                gameManager.CheckFinish();
            }
            GetComponent<SpriteRenderer>().color = finishColor;
            Debug.Log($"Box {gameObject.name} entered target: {collision.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Target") && isOnTarget)
        {
            isOnTarget = false;
            if (gameManager != null)
            {
                gameManager.finishedBoxs--;
            }
            GetComponent<SpriteRenderer>().color = originColor;
            Debug.Log($"Box {gameObject.name} left target: {collision.name}");
        }
    }
}
