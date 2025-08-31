using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
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
    public float switchCooldownAfterHit = 3f;
    public bool disableSwitchWhenThreatened = true;
    
    [Header("Scene-Specific Camera Settings")]
    public SceneCameraConfig[] sceneCameraConfigs;
    
    [System.Serializable]
    public class SceneCameraConfig
    {
        [Header("Scene Info")]
        [SceneDropdown] // Add the dropdown attribute here
        public string sceneName;
        public bool useFixedPositions = false;
        
        [Header("Fixed Camera Positions (if enabled)")]
        public CameraPosition[] playerCameraPositions;
        
        [System.Serializable]
        public class CameraPosition
        {
            public Vector3 position;
            public float lensValue = 6f;
            public float transitionSpeed = 2f;
        }
    }
    
    private int currentPlayerIndex = 0;
    private Player currentActivePlayer;
    private List<EnemyChaseAI> allEnemies = new List<EnemyChaseAI>();
    
    // Cooldown system
    private float switchCooldownTimer = 0f;
    private bool isOnCooldown = false;
    
    // Threat detection
    private bool isAnyPlayerThreatened = false;
    
    // Camera system
    private CinemachineFollow cinemachineFollow;
    private SceneCameraConfig currentSceneConfig;
    private bool isTransitioningCamera = false;
    private Vector3 targetCameraPosition;
    private float targetLensValue;
    private float cameraTransitionSpeed = 2f;
    
    void Start()
    {
        if (players.Length == 0)
        {
            Debug.LogError("No players assigned to PlayerSwitcher!");
            return;
        }
        
        // Find all enemies in the scene
        FindAllEnemies();
        
        // Initialize camera system
        InitializeCameraSystem();
        
        // Initialize the first player as active
        SwitchToPlayer(0);
    }
    
    void Update()
    {
        UpdateCooldown();
        UpdateThreatStatus();
        HandleSwitchInput();
        HandleCameraTransition();
    }
    
    private void InitializeCameraSystem()
    {
        if (cinemachineCamera != null)
        {
            cinemachineFollow = cinemachineCamera.GetComponent<CinemachineFollow>();
        }
        
        // Find configuration for current scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        currentSceneConfig = GetSceneConfig(currentSceneName);
        
        if (currentSceneConfig != null && currentSceneConfig.useFixedPositions)
        {
            // Disable follow component for fixed camera mode
            if (cinemachineFollow != null)
            {
                cinemachineFollow.enabled = false;
            }
            
            Debug.Log($"Using fixed camera positions for scene: {currentSceneName}");
        }
        else
        {
            // Enable follow component for normal following mode
            if (cinemachineFollow != null)
            {
                cinemachineFollow.enabled = true;
            }
            
            Debug.Log($"Using follow camera mode for scene: {currentSceneName}");
        }
    }
    
    private SceneCameraConfig GetSceneConfig(string sceneName)
    {
        foreach (var config in sceneCameraConfigs)
        {
            if (config.sceneName == sceneName)
            {
                return config;
            }
        }
        return null;
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
        
        foreach (EnemyChaseAI enemy in allEnemies)
        {
            if (enemy != null && enemy.CanSeeAnyPlayer())
            {
                isAnyPlayerThreatened = true;
                break;
            }
        }
        
        UpdateAllPlayerVisuals();
    }
    
    private void HandleSwitchInput()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryToSwitchPlayer();
        }
    }
    
    private void HandleCameraTransition()
    {
        if (isTransitioningCamera && cinemachineCamera != null)
        {
            // Smoothly move camera to target position
            Vector3 currentPos = cinemachineCamera.transform.position;
            Vector3 newPos = Vector3.Lerp(currentPos, targetCameraPosition, cameraTransitionSpeed * Time.deltaTime);
            cinemachineCamera.transform.position = newPos;
            
            // Smoothly change lens value
            var lens = cinemachineCamera.Lens;
            lens.OrthographicSize = Mathf.Lerp(lens.OrthographicSize, targetLensValue, cameraTransitionSpeed * Time.deltaTime);
            cinemachineCamera.Lens = lens;
            
            // Check if transition is complete
            if (Vector3.Distance(newPos, targetCameraPosition) < 0.1f && 
                Mathf.Abs(lens.OrthographicSize - targetLensValue) < 0.1f)
            {
                // Snap to final position and stop transition
                cinemachineCamera.transform.position = targetCameraPosition;
                lens.OrthographicSize = targetLensValue;
                cinemachineCamera.Lens = lens;
                isTransitioningCamera = false;
                
                Debug.Log($"Camera transition complete. Position: {targetCameraPosition}, Lens: {targetLensValue}");
            }
        }
    }
    
    private void TryToSwitchPlayer()
    {
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
        if (players.Length <= 1) return false;
        if (isOnCooldown) return false;
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
        
        // Update camera
        UpdateCameraForPlayer(playerIndex);
        
        // Update visuals
        UpdateAllPlayerVisuals();
        
        Debug.Log($"Switched to {currentActivePlayer.name}");
    }
    
    private void UpdateCameraForPlayer(int playerIndex)
    {
        if (cinemachineCamera == null) return;
        
        if (currentSceneConfig != null && currentSceneConfig.useFixedPositions)
        {
            // Use fixed camera positions
            if (playerIndex < currentSceneConfig.playerCameraPositions.Length)
            {
                var cameraPos = currentSceneConfig.playerCameraPositions[playerIndex];
                
                // Start camera transition to fixed position
                targetCameraPosition = cameraPos.position;
                targetLensValue = cameraPos.lensValue;
                cameraTransitionSpeed = cameraPos.transitionSpeed;
                isTransitioningCamera = true;
                
                Debug.Log($"Transitioning camera to fixed position: {targetCameraPosition}, Lens: {targetLensValue}");
            }
        }
        else
        {
            // Use follow mode - update camera target
            if (cinemachineFollow != null && currentActivePlayer != null)
            {
                cinemachineCamera.Target.TrackingTarget = currentActivePlayer.transform;
                Debug.Log($"Camera now following {currentActivePlayer.name}");
            }
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
                spriteRenderer.color = isAnyPlayerThreatened ? threatenedPlayerColor : activePlayerColor;
            }
            else
            {
                spriteRenderer.color = inactivePlayerColor;
            }
        }
    }
    
    public void OnPlayer2Hit()
    {
        Debug.Log("Player 2 was hit! Forcing switch to Player 1 and starting cooldown");
        
        SwitchToPlayer(0);
        
        isOnCooldown = true;
        switchCooldownTimer = switchCooldownAfterHit;
        
        Debug.Log($"Cannot switch to Player 2 for {switchCooldownAfterHit} seconds");
    }
    
    public void OnMainPlayerHit()
    {
        Debug.Log("Main Player was hit! Game Over!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
            GUILayout.BeginArea(new Rect(10, 10, 350, 200));
            GUILayout.Label($"Active Player: {(currentActivePlayer ? currentActivePlayer.name : "None")}");
            GUILayout.Label($"Threatened: {(isAnyPlayerThreatened ? "YES" : "No")}");
            GUILayout.Label($"Can Switch: {(CanSwitchPlayers() ? "YES" : "No")}");
            
            if (currentSceneConfig != null)
            {
                GUILayout.Label($"Scene: {SceneManager.GetActiveScene().name}");
                GUILayout.Label($"Camera Mode: {(currentSceneConfig.useFixedPositions ? "Fixed Positions" : "Follow")}");
            }
            
            if (isTransitioningCamera)
            {
                GUILayout.Label($"Camera Transitioning: {targetCameraPosition}");
            }
            
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
