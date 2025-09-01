using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // <- NEW INPUT SYSTEM
using UnityEngine.SceneManagement;

public class FirstSceneController : MonoBehaviour
{
    [Header("Next Scene")]
    [Tooltip("Type the scene name exactly as in Build Settings (e.g., level-1).")]
    public string nextScene;


    [Header("Required")]
    public Image slideImage;          // UI Image that shows the current slide
    public Sprite[] slides;           // Put your slide sprites here (order matters)

    [Header("Auto Advance (optional)")]
    [Tooltip("Seconds between slides. Set <= 0 to disable auto-advance.")]
    public float autoAdvanceSeconds = 0f;

    [Header("Transition (optional)")]
    public float fadeDuration = 0.25f; // seconds

    [Header("Optional: Caption (legacy UI Text)")]
    public Text captionText;           // Leave empty if not using captions
    public string[] captions;          // One caption per slide (optional)

    [Header("Optional: Audio per slide")]
    public AudioSource audioSource;    // Assign one on the same GameObject
    public AudioClip[] slideAudio;     // One clip per slide (optional)

    private int index = -1;
    private CanvasGroup cg;
    private bool isFading;

    void Awake()
    {
        if (!slideImage)
        {
            Debug.LogError("SlideshowController: Slide Image not assigned.");
            enabled = false;
            return;
        }

        // Ensure there is a CanvasGroup on the image for fading
        cg = slideImage.GetComponent<CanvasGroup>();
        if (!cg) cg = slideImage.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
    }

    void Start()
    {
        // Show first slide
        NextSlide();

        if (autoAdvanceSeconds > 0f)
            StartCoroutine(AutoAdvanceLoop());
    }

    void Update()
    {
        if (isFading) return;

        // --- NEW INPUT SYSTEM CHECKS ---
        bool pressed = false;

        if (Keyboard.current != null)
        {
            pressed |= Keyboard.current.enterKey.wasPressedThisFrame
                   || Keyboard.current.numpadEnterKey.wasPressedThisFrame
                   || Keyboard.current.spaceKey.wasPressedThisFrame;
        }

        if (Gamepad.current != null)
        {
            pressed |= Gamepad.current.buttonSouth.wasPressedThisFrame   // A / Cross
                   || Gamepad.current.startButton.wasPressedThisFrame;  // Start/Options
        }

        // Optional: tap to advance on touch devices
        // if (Touchscreen.current != null)
        //     pressed |= Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;

        if (pressed)
            NextSlide();
    }

    public void NextSlide()
    {
        index++;
        if (index >= slides.Length)
        {
            EndSlideshow();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ShowSlide(index));
        if (autoAdvanceSeconds > 0f)
            StartCoroutine(AutoAdvanceLoop());
    }

    IEnumerator ShowSlide(int i)
    {
        isFading = true;

        // Fade out
        yield return FadeTo(0f, fadeDuration);

        // Swap sprite
        slideImage.sprite = slides[i];
        slideImage.preserveAspect = true; // keeps proportions

        // Optional caption
        if (captionText && captions != null && i < captions.Length)
            captionText.text = captions[i];

        // Optional audio
        if (audioSource && slideAudio != null && i < slideAudio.Length)
        {
            audioSource.Stop();
            if (slideAudio[i]) audioSource.PlayOneShot(slideAudio[i]);
        }

        // Fade in
        yield return FadeTo(1f, fadeDuration);
        isFading = false;
    }

    IEnumerator FadeTo(float target, float duration)
    {
        float start = cg.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        cg.alpha = target;
    }

    IEnumerator AutoAdvanceLoop()
    {
        // wait then advance, unless we’re already fading or finished
        yield return new WaitForSeconds(autoAdvanceSeconds);
        if (!isFading) NextSlide();
    }

    void EndSlideshow()
    {
        //// What to do at the end:
        //// 1) Hide the UI
        //slideImage.canvasRenderer.SetAlpha(0f);
        //if (captionText) captionText.text = "";

        //// Or 2) Load next scene:
        //// SceneManager.LoadScene("GameScene");
        ///

        if (string.IsNullOrWhiteSpace(nextScene))
        {
            Debug.LogError("FirstSceneController: nextScene is empty. Set it in the Inspector.");
            return;
        }

        SceneManager.LoadScene(nextScene);
    }
}
