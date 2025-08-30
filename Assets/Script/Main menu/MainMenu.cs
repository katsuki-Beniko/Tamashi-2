using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Start Game")]
    [Tooltip("Pick which scene to load when pressing Start. Must be in Build Settings.")]
    public int startSceneIndex = 0;   // dropdown index handled by custom editor

    public void OnStartButton()
    {
        if (startSceneIndex < 0 || startSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("[MainMenu] Invalid scene index. Check Build Settings.");
            return;
        }

        string scenePath = SceneUtility.GetScenePathByBuildIndex(startSceneIndex);
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        SceneManager.LoadScene(startSceneIndex);
        Debug.Log($"[MainMenu] Loading scene: {sceneName}");
    }

    public void OnSettingsButton()
    {
        Debug.Log("[MainMenu] Settings clicked (not implemented yet).");
    }

    public void OnQuitButton()
    {
        Debug.Log("[MainMenu] Quit Game.");
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}
