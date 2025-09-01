using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class TimelineManager : MonoBehaviour
{
    [Header("Player Animation Control")]
    public Animator playerAnimator;
    public RuntimeAnimatorController playerAnim;
    public PlayableDirector director;
    
    [Header("Scene Transition Timing")]
    public bool enableSceneTransition = true;
    public TransitionTriggerType triggerType = TransitionTriggerType.SpecificTime;
    public float transitionTriggerTime = 8.0f; // Time in seconds when to trigger transition
    public float transitionDelay = 2f; // Additional delay after trigger
    
    [Header("Target Scene")]
    public string targetSceneName = "MainMenu"; 
    public bool useSceneIndex = false;
    public int targetSceneIndex = 0;
    
    [Header("Transition Effects")]
    public bool fadeOut = true;
    public float fadeOutDuration = 1f;
    public CanvasGroup fadeCanvasGroup;
    public Color fadeColor = Color.black;
    public bool destroyFadeImmediately = true; // NEW: Destroy fade right before scene loads
    
    [Header("Audio")]
    public AudioClip transitionSound;
    public bool stopAllAudio = true;
    
    public enum TransitionTriggerType
    {
        SpecificTime,       // Trigger at specific timeline time
        TimelineEnd,        // Wait for timeline to finish
        Manual              // Trigger manually via script
    }
    
    private bool fix = false;
    private bool hasTransitioned = false;
    private bool hasTriggeredTransition = false;
    private AudioSource audioSource;
    private GameObject createdFadeOverlay; // Track created fade overlay

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (director == null)
        {
            director = GetComponent<PlayableDirector>();
        }
        
        // Subscribe to timeline completion event (backup)
        if (director != null)
        {
            director.stopped += OnTimelineFinished;
        }
    }

    void OnEnable()
    {
        if (playerAnimator != null)
        {
            playerAnim = playerAnimator.runtimeAnimatorController;
            playerAnimator.runtimeAnimatorController = null;
        }
    }

    void Update()
    {
        // Original animation fix logic
        if (director != null && director.state != PlayState.Playing && !fix)
        {
            fix = true;
            if (playerAnimator != null)
            {
                playerAnimator.runtimeAnimatorController = playerAnim;
            }
            Debug.Log("Timeline finished - Player animation restored");
        }
        
        // Check for time-based transition trigger
        if (triggerType == TransitionTriggerType.SpecificTime && 
            !hasTriggeredTransition && 
            enableSceneTransition &&
            director != null && 
            director.state == PlayState.Playing)
        {
            // Check if we've reached the trigger time
            if (director.time >= transitionTriggerTime)
            {
                hasTriggeredTransition = true;
                Debug.Log($"Timeline reached trigger time {transitionTriggerTime}s - Starting scene transition!");
                StartCoroutine(TransitionToNextScene());
            }
        }
    }
    
    // Called when timeline finishes completely (backup method)
    private void OnTimelineFinished(PlayableDirector finishedDirector)
    {
        if (finishedDirector == director && 
            !hasTransitioned && 
            triggerType == TransitionTriggerType.TimelineEnd)
        {
            Debug.Log("Timeline completed! Starting scene transition...");
            
            if (enableSceneTransition)
            {
                StartCoroutine(TransitionToNextScene());
            }
        }
    }
    
    private IEnumerator TransitionToNextScene()
    {
        if (hasTransitioned) yield break;
        hasTransitioned = true;
        
        // Play transition sound
        if (audioSource != null && transitionSound != null)
        {
            audioSource.PlayOneShot(transitionSound);
        }
        
        // Wait for delay
        if (transitionDelay > 0)
        {
            Debug.Log($"Waiting {transitionDelay} seconds before scene transition...");
            yield return new WaitForSeconds(transitionDelay);
        }
        
        // Fade out effect
        if (fadeOut)
        {
            yield return StartCoroutine(FadeOut());
        }
        
        // Stop all audio if requested
        if (stopAllAudio)
        {
            AudioListener.pause = true;
        }
        
        // CRITICAL: Destroy fade overlay before loading scene
        if (destroyFadeImmediately && createdFadeOverlay != null)
        {
            Debug.Log("Destroying fade overlay before scene load");
            Destroy(createdFadeOverlay);
            createdFadeOverlay = null;
        }
        
        // Load the target scene
        LoadTargetScene();
    }
    
    private IEnumerator FadeOut()
    {
        if (fadeCanvasGroup != null)
        {
            // Use existing canvas group
            float elapsedTime = 0f;
            float startAlpha = fadeCanvasGroup.alpha;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / fadeOutDuration);
                fadeCanvasGroup.alpha = alpha;
                yield return null;
            }
            
            fadeCanvasGroup.alpha = 1f;
        }
        else
        {
            // Create a temporary fade overlay
            createdFadeOverlay = CreateFadeOverlay();
            CanvasGroup cg = createdFadeOverlay.GetComponent<CanvasGroup>();
            
            float elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeOutDuration);
                cg.alpha = alpha;
                yield return null;
            }
            
            cg.alpha = 1f;
            
            // Optional: Destroy immediately after fade completes
            if (destroyFadeImmediately)
            {
                Debug.Log("Destroying fade overlay after fade completes");
                Destroy(createdFadeOverlay);
                createdFadeOverlay = null;
                
                // Brief pause to ensure scene loads properly
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    
    private GameObject CreateFadeOverlay()
    {
        // Create a canvas for fade overlay
        GameObject canvasGO = new GameObject("FadeCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        
        CanvasGroup canvasGroup = canvasGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        
        // Create fade panel
        GameObject panelGO = new GameObject("FadePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        
        UnityEngine.UI.Image image = panelGO.AddComponent<UnityEngine.UI.Image>();
        image.color = fadeColor;
        
        // Make it fullscreen
        RectTransform rectTransform = panelGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // IMPORTANT: Only use DontDestroyOnLoad if we're NOT destroying immediately
        if (!destroyFadeImmediately)
        {
            DontDestroyOnLoad(canvasGO);
            
            // Schedule destruction after scene loads
            StartCoroutine(DestroyFadeAfterDelay(canvasGO, 2f));
        }
        
        Debug.Log("Created fade overlay");
        return canvasGO;
    }
    
    // Coroutine to destroy fade overlay after a delay (backup cleanup)
    private IEnumerator DestroyFadeAfterDelay(GameObject fadeOverlay, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (fadeOverlay != null)
        {
            Debug.Log("Destroying fade overlay after delay (backup cleanup)");
            Destroy(fadeOverlay);
        }
    }
    
    private void LoadTargetScene()
    {
        try
        {
            if (useSceneIndex)
            {
                Debug.Log($"Loading scene by index: {targetSceneIndex}");
                SceneManager.LoadScene(targetSceneIndex);
            }
            else
            {
                Debug.Log($"Loading scene by name: {targetSceneName}");
                SceneManager.LoadScene(targetSceneName);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene: {e.Message}");
            try
            {
                SceneManager.LoadScene(0);
            }
            catch
            {
                Debug.LogError("Failed to load fallback scene!");
            }
        }
    }
    
    // Public methods for external control
    public void TriggerSceneTransitionNow()
    {
        if (!hasTransitioned)
        {
            Debug.Log("Manual scene transition triggered!");
            StartCoroutine(TransitionToNextScene());
        }
    }
    
    public void SetTransitionTime(float timeInSeconds)
    {
        transitionTriggerTime = timeInSeconds;
        hasTriggeredTransition = false; // Reset to allow new trigger time
        Debug.Log($"Transition trigger time set to {timeInSeconds} seconds");
    }
    
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
        useSceneIndex = false;
    }
    
    public void SetTargetScene(int sceneIndex)
    {
        targetSceneIndex = sceneIndex;
        useSceneIndex = true;
    }
    
    public void CancelTransition()
    {
        enableSceneTransition = false;
        StopAllCoroutines();
        
        // Clean up any existing fade overlay
        if (createdFadeOverlay != null)
        {
            Destroy(createdFadeOverlay);
            createdFadeOverlay = null;
        }
        
        Debug.Log("Scene transition cancelled");
    }
    
    // Debug method to show current timeline time
    void OnGUI()
    {
        if (director != null && director.state == PlayState.Playing)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"Timeline Time: {director.time:F2}s / {director.duration:F2}s");
            GUI.Label(new Rect(10, 30, 300, 20), $"Trigger Time: {transitionTriggerTime:F2}s");
            
            if (triggerType == TransitionTriggerType.SpecificTime)
            {
                float timeLeft = transitionTriggerTime - (float)director.time;
                if (timeLeft > 0)
                {
                    GUI.Label(new Rect(10, 50, 300, 20), $"Transition in: {timeLeft:F2}s");
                }
            }
        }
    }
    
    void OnDestroy()
    {
        if (director != null)
        {
            director.stopped -= OnTimelineFinished;
        }
        
        // Clean up fade overlay when this component is destroyed
        if (createdFadeOverlay != null)
        {
            Destroy(createdFadeOverlay);
            createdFadeOverlay = null;
        }
    }
}
