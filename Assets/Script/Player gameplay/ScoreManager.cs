using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager I;
    public static event Action<int> OnScoreChanged;

    int _score;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public int Score => _score;

    public void Add(int amount)
    {
        _score = Mathf.Max(0, _score + amount);
        OnScoreChanged?.Invoke(_score);
    }

    public void ResetScore()
    {
        _score = 0;
        OnScoreChanged?.Invoke(_score);
    }
}
