using TMPro;
using UnityEngine;

public class LeaderData : MonoBehaviour
{

    public TextMeshProUGUI scoreText;
    public int score;

    private void Start()
    {
        updateScore(score);
    }

    public void updateScore(int s)
    {
        score = s;
        scoreText.text = score.ToString("N0");
    }
}
