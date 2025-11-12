using UnityEngine;
using TMPro;

public class DisplayScore : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    private void Start()
    {
    }

    private void Update()
    {
        if (ScoreCounter.Instance != null && scoreText != null)
        {
            scoreText.text = $"Floor: {ScoreCounter.Instance.CurrentScore}";
        }
    }
}