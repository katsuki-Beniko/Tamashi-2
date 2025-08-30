using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class NumberPadController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI displayText;  // Changed from TextMeshPro to TextMeshProUGUI
    public Button[] numberButtons; // Array for buttons 0-9
    public Button enterButton;
    public Button clearButton;
    public Button closeButton;
    
    [Header("Settings")]
    public int maxPasswordLength = 8;
    public string displayPrefix = "Enter Password: ";
    
    private string currentInput = "";
    
    // Events
    public event Action<string> OnPasswordEntered;
    public event Action OnPuzzleClosed;
    
    void Start()
    {
        SetupButtons();
        UpdateDisplay();
    }
    
    private void SetupButtons()
    {
        // Setup number buttons (0-9)
        for (int i = 0; i < numberButtons.Length; i++)
        {
            int number = i; // Capture the value for closure
            if (numberButtons[i] != null)
            {
                numberButtons[i].onClick.AddListener(() => AddNumber(number.ToString()));
            }
        }
        
        // Setup control buttons
        if (enterButton != null)
            enterButton.onClick.AddListener(SubmitPassword);
            
        if (clearButton != null)
            clearButton.onClick.AddListener(ClearInput);
            
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePuzzle);
    }
    
    public void AddNumber(string number)
    {
        if (currentInput.Length < maxPasswordLength)
        {
            currentInput += number;
            UpdateDisplay();
            Debug.Log($"Added number: {number}, Current input: {currentInput}");
        }
    }
    
    public void ClearInput()
    {
        currentInput = "";
        UpdateDisplay();
        Debug.Log("Input cleared");
    }
    
    public void ResetInput()
    {
        ClearInput();
    }
    
    public void SubmitPassword()
    {
        if (!string.IsNullOrEmpty(currentInput))
        {
            Debug.Log($"Submitting password: {currentInput}");
            OnPasswordEntered?.Invoke(currentInput);
        }
        else
        {
            Debug.Log("No password entered!");
        }
    }
    
    public void ClosePuzzle()
    {
        OnPuzzleClosed?.Invoke();
    }
    
    private void UpdateDisplay()
    {
        if (displayText != null)
        {
            // Show asterisks instead of actual numbers for security
            string maskedInput = new string('*', currentInput.Length);
            displayText.text = displayPrefix + maskedInput;
        }
    }
}
