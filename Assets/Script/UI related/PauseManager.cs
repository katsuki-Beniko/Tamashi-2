using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuRoot; // a Panel (inactive by default)
    public GameObject firstSelected; // optional: a Button to auto-select when paused

    bool _paused;

    void Start()
    {
        SetPaused(false);
    }

    void Update()
    {
        // Press Esc to toggle pause
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetPaused(!_paused);
        }
    }

    public void SetPaused(bool pause)
    {
        _paused = pause;
        Time.timeScale = _paused ? 0f : 1f;
        if (pauseMenuRoot) pauseMenuRoot.SetActive(_paused);

        // Pause/unpause audio
        AudioListener.pause = _paused;

        // Optional (UI selection for controller)
        if (_paused && firstSelected && EventSystem.current)
            EventSystem.current.SetSelectedGameObject(firstSelected);
            
        Debug.Log(_paused ? "Game Paused" : "Game Resumed");
    }

    // Hook these to your UI Buttons:
    public void OnResumeButton() => SetPaused(false);
    public void OnCloseButton() => SetPaused(false);  // New method for X button
    public void OnRestartButton() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    public void OnQuitButton() => Application.Quit();
}
