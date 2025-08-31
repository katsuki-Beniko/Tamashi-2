using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviour
{
    [Header("Start Game")]
    [Tooltip("Pick which scene to load when pressing Start. Must be in Build Settings.")]
    public int startSceneIndex = 0;   // dropdown index handled by custom editor

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        // Load saved values into sliders when opening settings
        if (PlayerPrefs.HasKey("MusicVolume"))
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");

        if (PlayerPrefs.HasKey("SFXVolume"))
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
    }
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
        SceneManager.LoadScene("Setting");
        // Debug.Log("[MainMenu] Settings clicked (not implemented yet).");
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

    // Setting scene
    public void SaveSettings()
    {
        float musicVolume = musicSlider.value;
        float sfxVolume = sfxSlider.value;

        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();

        // Apply immediately to AudioMixer
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);

        // Also tell SoundManager to re-apply (if you want)
        if (SoundManager.instance != null)
            SoundManager.instance.ApplySavedVolumeSettings();

        // Go back to Main Menu
        SceneManager.LoadScene("Main Menu");
    }

    public void CancelSettings()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
