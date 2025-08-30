using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    public float transitionDelay = 0.5f;
    public bool fadeOut = true;
    public float fadeOutDuration = 1f;
    
    private static SceneTransitionManager instance;
    
    public static SceneTransitionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SceneTransitionManager>();
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(TransitionCoroutine(sceneName, -1));
    }
    
    public void TransitionToScene(int sceneIndex)
    {
        StartCoroutine(TransitionCoroutine(null, sceneIndex));
    }
    
    private IEnumerator TransitionCoroutine(string sceneName, int sceneIndex)
    {
        // Wait for transition delay
        yield return new WaitForSeconds(transitionDelay);
        
        // Optional: Add fade out effect here
        if (fadeOut)
        {
            // You can implement a fade out effect here
            Debug.Log("Fading out...");
            yield return new WaitForSeconds(fadeOutDuration);
        }
        
        // Load the scene
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }
}
