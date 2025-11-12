using UnityEngine;

public class ScoreCounter : MonoBehaviour
{
    public static ScoreCounter Instance { get; private set; }

    private int currentScore = 0;

    public int CurrentScore
    {
        get => currentScore;
        set => currentScore = value;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
    }

    public void SubtractScore(int amount)
    {
        currentScore -= amount;
    }

    public void ResetScore()
    {
        currentScore = 0;
    }
}