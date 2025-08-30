using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections.Generic;

public class PlayerSwitcher : MonoBehaviour
{
    [Header("Players")]
    public Player[] players;
    
    [Header("Camera")]
    public CinemachineCamera cinemachineCamera;
    
    [Header("Visual Feedback")]
    public Color activePlayerColor = Color.red;
    public Color inactivePlayerColor = Color.gray;
    public Color threatenedPlayerColor = Color.orange;
    
    [Header("Switching Rules")]
    public float switchCooldownAfterHit = 3f; // Cooldown when Player 2 gets hit
    public bool disableSwitchWhenThreatened = true; // Can't switch when enemy sees you
    
    private int currentPlayerIndex = 0;
    private Player currentActivePlayer;
    private List<EnemyChaseAI> allEnemies = new List<EnemyChaseAI>();
    
    // Cooldown system
    private float switchCooldownTimer = 0f;
    private bool isOnCooldown = false;
    
    // Threat detection
    private bool isAnyPlayerThreatened = false;
    
    void Start()
    {
        if (players.Length == 0)
        {
            Debug.LogError("No players assigned to PlayerSwitcher!");
            return;
        }
        
        // Find all enemies in the scene
        FindAllEnemies();
        
        // Initialize the first player as active (Player is always the main)
        SwitchToPlayer(0);
    }
    
    void Update()
    {
        UpdateCooldown();
        UpdateThreatStatus();
        HandleSwitchInput();
    }
    
    private void FindAllEnemies()
    {
        EnemyChaseAI[] enemies = FindObjectsByType<EnemyChaseAI>(FindObjectsSortMode.None);
        allEnemies.Clear();
        allEnemies.AddRange(enemies);
        Debug.Log($"Found {allEnemies.Count} enemies in scene");
    }
    
    private void UpdateCooldown()
    {
        if (isOnCooldown)
        {
            switchCooldownTimer -= Time.deltaTime;
            if (switchCooldownTimer <= 0f)
            {
                isOnCooldown = false;
                Debug.Log("Switch cooldown ended - can switch to Player 2 again");
            }
        }
    }
    
    private void UpdateThreatStatus()
    {
        isAnyPlayerThreatened = false;
        
        // Check if any enemy can see any player
        foreach (EnemyChaseAI enemy in allEnemies)
        {
            if (enemy != null && enemy.CanSeeAnyPlayer())
            {
                isAnyPlayerThreatened = true;
                break;
            }
        }
        
        // Update visual feedback based on threat status
        UpdateAllPlayerVisuals();
    }
    
    private void HandleSwitchInput()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            TryToSwitchPlayer();
        }
    }
    
    private void TryToSwitchPlayer()
    {
        // Check if switching is allowed
        if (!CanSwitchPlayers())
        {
            string reason = GetSwitchBlockReason();
            Debug.Log($"Cannot switch players: {reason}");
            return;
        }
        
        SwitchToNextPlayer();
    }
    
    private bool CanSwitchPlayers()
    {
        // Can't switch if only one player
        if (players.Length <= 1) return false;
        
        // Can't switch during cooldown
        if (isOnCooldown) return false;
        
        // Can't switch when threatened (if enabled)
        if (disableSwitchWhenThreatened && isAnyPlayerThreatened) return false;
        
        return true;
    }
    
    private string GetSwitchBlockReason()
    {
        if (players.Length <= 1) return "Only one player available";
        if (isOnCooldown) return $"Cooldown active ({switchCooldownTimer:F1}s remaining)";
        if (disableSwitchWhenThreatened && isAnyPlayerThreatened) return "Enemy is watching - cannot switch!";
        return "Unknown reason";
    }
    
    private void SwitchToNextPlayer()
    {
        if (players.Length <= 1) return;
        
        // Calculate next player index (loop back to 0 if at the end)
        int nextPlayerIndex = (currentPlayerIndex + 1) % players.Length;
        SwitchToPlayer(nextPlayerIndex);
    }
    
    private void SwitchToPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= players.Length) return;
        
        // Deactivate current player
        if (currentActivePlayer != null)
        {
            currentActivePlayer.SetActive(false);
        }
        
        // Activate new player
        currentPlayerIndex = playerIndex;
        currentActivePlayer = players[currentPlayerIndex];
        currentActivePlayer.SetActive(true);
        
        // Update camera to follow new player
        UpdateCameraTarget(currentActivePlayer.transform);
        
        // Update visuals
        UpdateAllPlayerVisuals();
        
        Debug.Log($"Switched to {currentActivePlayer.name}");
    }
    
    private void UpdateCameraTarget(Transform newTarget)
    {
        if (cinemachineCamera != null)
        {
            cinemachineCamera.Target.TrackingTarget = newTarget;
        }
    }
    
    private void UpdateAllPlayerVisuals()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null)
            {
                UpdatePlayerVisuals(players[i], i == currentPlayerIndex);
            }
        }
    }
    
    private void UpdatePlayerVisuals(Player player, bool isActive)
    {
        SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (isActive)
            {
                // Active player - red if threatened, normal active color if safe
                spriteRenderer.color = isAnyPlayerThreatened ? threatenedPlayerColor : activePlayerColor;
            }
            else
            {
                // Inactive player
                spriteRenderer.color = inactivePlayerColor;
            }
        }
    }
    
    // Called when Player 2 gets hit by enemy
    public void OnPlayer2Hit()
    {
        Debug.Log("Player 2 was hit! Forcing switch to Player 1 and starting cooldown");
        
        // Force switch to Player (index 0)
        SwitchToPlayer(0);
        
        // Start cooldown
        isOnCooldown = true;
        switchCooldownTimer = switchCooldownAfterHit;
        
        Debug.Log($"Cannot switch to Player 2 for {switchCooldownAfterHit} seconds");
    }
    
    // Called when main Player gets hit by enemy
    public void OnMainPlayerHit()
    {
        Debug.Log("Main Player was hit! Game Over!");
        // Handle game over logic here
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
    
    public Player GetActivePlayer()
    {
        return currentActivePlayer;
    }
    
    public bool IsPlayerActive(Player player)
    {
        return currentActivePlayer == player;
    }
    
    public bool IsMainPlayer(Player player)
    {
        // Player at index 0 is always the main player
        return players.Length > 0 && players[0] == player;
    }
    
    public bool IsThreatened()
    {
        return isAnyPlayerThreatened;
    }
    
    public bool IsOnSwitchCooldown()
    {
        return isOnCooldown;
    }
    
    public float GetCooldownTimeRemaining()
    {
        return isOnCooldown ? switchCooldownTimer : 0f;
    }
    
    // Debug information
    void OnGUI()
    {
        if (Application.isPlaying)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Label($"Active Player: {(currentActivePlayer ? currentActivePlayer.name : "None")}");
            GUILayout.Label($"Threatened: {(isAnyPlayerThreatened ? "YES" : "No")}");
            GUILayout.Label($"Can Switch: {(CanSwitchPlayers() ? "YES" : "No")}");
            
            if (isOnCooldown)
            {
                GUILayout.Label($"Cooldown: {switchCooldownTimer:F1}s");
            }
            
            if (!CanSwitchPlayers())
            {
                GUILayout.Label($"Blocked: {GetSwitchBlockReason()}");
            }
            GUILayout.EndArea();
        }
    }
}
