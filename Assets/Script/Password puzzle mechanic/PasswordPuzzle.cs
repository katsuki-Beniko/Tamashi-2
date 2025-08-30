using UnityEngine;

public class PasswordPuzzle : MonoBehaviour, IInteractable
{
    [Header("Puzzle Settings")]
    public string correctPassword = "1234";
    public bool oneTimeUse = true;
    
    [Header("UI References")]
    public GameObject numberPadUI;
    public NumberPadController numberPadController;
    
    private bool hasBeenSolved = false;
    
    void Start()
    {
        if (numberPadUI != null)
            numberPadUI.SetActive(false);
            
        if (numberPadController != null)
        {
            numberPadController.OnPasswordEntered += CheckPassword;
            numberPadController.OnPuzzleClosed += CloseNumberPad;
        }
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
        
        // Add your success logic here
        // Examples: open doors, spawn items, trigger events, etc.
        
        CloseNumberPad();
    }
    
    private void OnWrongPassword()
    {
        Debug.Log("Wrong password! Try again.");
        
        if (numberPadController != null)
        {
            numberPadController.ResetInput();
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
