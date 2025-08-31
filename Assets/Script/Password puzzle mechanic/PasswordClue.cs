using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // Add this for new Input System
using TMPro;

public class PasswordClue : MonoBehaviour, IInteractable
{
    [Header("Clue Settings")]
    [TextArea(3, 6)]
    public bool oneTimeUse = false;
    
    [Header("UI References")]
    public GameObject clueUI;
    public Button closeButton;
    
    [Header("Input Settings")]
    public bool allowEscapeToClose = true; // Option to disable ESC if needed
    
    private bool hasBeenUsed = false;
    private bool isUIOpen = false; // Track UI state
    
    void Start()
    {
        if (clueUI != null)
        {
            clueUI.SetActive(false);
            isUIOpen = false;
        }
            
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseClueUI);
    }
    
    void Update()
    {
        // Check for ESC key input when UI is open
        if (allowEscapeToClose && isUIOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseClueUI();
        }
    }
    
    public void Interact(Player player)
    {
        if (oneTimeUse && hasBeenUsed)
        {
            Debug.Log("This clue has already been read.");
            return;
        }
        
        ShowClue();
        
        if (oneTimeUse)
            hasBeenUsed = true;
    }
    
    private void ShowClue()
    {
        if (clueUI != null)
        {
            clueUI.SetActive(true);
            isUIOpen = true;
            Debug.Log("Clue UI opened. Press ESC to close.");
        }
    }
    
    private void CloseClueUI()
    {
        if (clueUI != null)
        {
            clueUI.SetActive(false);
            isUIOpen = false;
            Debug.Log("Clue UI closed.");
        }
    }
    
    // Public method to close UI from external scripts if needed
    public void ForceCloseUI()
    {
        CloseClueUI();
    }
    
    // Public method to check if UI is currently open
    public bool IsUIOpen()
    {
        return isUIOpen;
    }
}
