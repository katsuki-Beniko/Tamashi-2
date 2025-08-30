using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PasswordClue : MonoBehaviour, IInteractable
{
    [Header("Clue Settings")]
    [TextArea(3, 6)]
    public string clueText = "The answer is the year this building was constructed: 1984";
    public bool oneTimeUse = false;
    
    [Header("UI References")]
    public GameObject clueUI;
    public TextMeshProUGUI clueTextDisplay;
    public Button closeButton;
    
    private bool hasBeenUsed = false;
    
    void Start()
    {
        if (clueUI != null)
            clueUI.SetActive(false);
            
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseClueUI);
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
            
            if (clueTextDisplay != null)
                clueTextDisplay.text = clueText;
                
            Debug.Log($"Showing clue: {clueText}");
        }
    }
    
    private void CloseClueUI()
    {
        if (clueUI != null)
            clueUI.SetActive(false);
    }
}
