using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHUD : MonoBehaviour
{
    [Header("Health")]
    public Health playerHealth;      // drag the Player's Health component here
    public Image healthFill;         // UI Image with Fill Method = Horizontal
    public Text  healthText;         // optional (legacy Text)
    public TMP_Text healthTMP;       // optional (TMP)

    [Header("Score")]
    public Text  scoreText;          // optional
    public TMP_Text scoreTMP;        // optional

    [Header("Timer")]
    public bool pauseWithGame = true;  // true = timer stops when paused
    public Text  timerText;            // optional
    public TMP_Text timerTMP;          // optional

    float _time;

    void OnEnable()
    {
        ScoreManager.OnScoreChanged += UpdateScore;
    }

    void OnDisable()
    {
        ScoreManager.OnScoreChanged -= UpdateScore;
    }

    void Start()
    {
        UpdateScore(ScoreManager.I ? ScoreManager.I.Score : 0);
        UpdateHealthUI();
    }

    void Update()
    {
        // Timer (count-up)
        float dt = pauseWithGame ? Time.deltaTime : Time.unscaledDeltaTime;
        _time += dt;
        SetText(timerText, timerTMP, FormatTime(_time));

        // Health can change at runtime; poll or subscribe (poll = fine for jam)
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (!playerHealth) return;

        int cur = playerHealth.Current;
        int max = playerHealth.maxHealth;

        if (healthFill)
            healthFill.fillAmount = max > 0 ? (float)cur / max : 0f;

        SetText(healthText, healthTMP, $"{cur} / {max}");
    }

    void UpdateScore(int s)
    {
        SetText(scoreText, scoreTMP, s.ToString());
    }

    static void SetText(Text t, TMP_Text tmp, string v)
    {
        if (t) t.text = v;
        if (tmp) tmp.text = v;
    }

    static string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }
}
