using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;

public class DialogTapToContinue : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup group;          // CanvasGroup on the full-screen panel
    [SerializeField] private GameObject container;       // The same panel (root to enable/disable)
    [SerializeField] private TMP_Text text;              // Your main dialog text

    [Header("Content")]
    [TextArea] [SerializeField] private string message = "It's time to go.";
    [SerializeField] private bool showOnStart = true;

    [Header("FX")]
    [SerializeField] private bool fade = true;
    [SerializeField] private float fadeDuration = 0.25f;

    public UnityEvent onContinue;                        // Hook anything you want to run after closing

    private bool showing;

    private void Awake()
    {
        if (text) text.text = message;

        if (container) container.SetActive(false);
        if (group)
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }

    private void Start()
    {
        if (showOnStart) Show();
    }

    public void Show()
    {
        if (container) container.SetActive(true);
        if (group)
        {
            group.blocksRaycasts = true;   // blocks gameplay clicks beneath
            group.interactable = true;
        }

        showing = true;
        if (fade) StartCoroutine(FadeTo(1f, fadeDuration));
        else if (group) group.alpha = 1f;
    }

    public void Hide()
    {
        showing = false;

        if (fade) StartCoroutine(FadeTo(0f, fadeDuration, () =>
        {
            if (container) container.SetActive(false);
        }));
        else
        {
            if (group) group.alpha = 0f;
            if (container) container.SetActive(false);
        }

        if (group)
        {
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        onContinue?.Invoke();
    }

    private void Update()
    {
        if (!showing) return;
        if (PressedThisFrame()) Hide();
    }

    private bool PressedThisFrame()
    {
        // Mouse / Pointer / Touch
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame) return true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;

        // Keyboard
        if (Keyboard.current != null &&
            (Keyboard.current.anyKey.wasPressedThisFrame ||
             Keyboard.current.enterKey.wasPressedThisFrame ||
             Keyboard.current.spaceKey.wasPressedThisFrame))
            return true;

        // Gamepad
        if (Gamepad.current != null &&
            (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame))
            return true;

        return false;
    }

    private System.Collections.IEnumerator FadeTo(float target, float duration, System.Action onDone = null)
    {
        float start = group ? group.alpha : 1f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;   // not affected by pause
            if (group) group.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        if (group) group.alpha = target;
        onDone?.Invoke();
    }
}
