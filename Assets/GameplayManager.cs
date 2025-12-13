using MoreMountains.InfiniteRunnerEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SocialPlatforms.Impl;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager instance;

    public LevelManager levelManager;

    [Header("Menu UI")]
    public GameObject mainMenuPanel;
    public GameObject top;

    [Header("Gameplay UI")]
    public GameObject HubPanel;
    public GameObject Timer;

    [Header("Crashed UI")]
    public GameObject CrashPanel;
    public TextMeshProUGUI crashScoreText;

    [Header("GameOver UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI FinalScoreText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI gemsText;
    public int inCoin;
    public int inGem;


    [Header("Gameplay Variables")]
    public bool inGame;
    public int currentScore = 0;
    public int scoreMultiplier = 1;
    public int highScore = 0;
    public int lasScore = 0;
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI coinsTextG;
    public TextMeshProUGUI gemsTextG;
    public MySpawner spawner;
    public SmartObstacleSpawner newMySpawner;


    [Header("Leaderboard")]
    public List<LeaderData> leaders;
    public LeaderData[] leaderMe;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        AudioManager.instance.PlayUITheme();
    }

    private void Update()
    {
        if (inGame)
        {
            currentScore += Mathf.RoundToInt(10 * scoreMultiplier * Time.deltaTime);
            currentScoreText.text = currentScore.ToString();
        }
    }

    public void PlayGame()
    {
        ResetGame();

        AudioManager.instance.PlayGameplayTheme();
        levelManager.StartGame();
        levelManager.ResetSpeed();

        mainMenuPanel.SetActive(false);
        top.SetActive(false);
        HubPanel.SetActive(true);
        inGame = true;

        spawner.StartGame();
        newMySpawner.EnableSpawning = true;
        coinsTextG.text = inCoin.ToString();
        gemsTextG.text = inGem.ToString();
    }



    public void Crashed()
    {
        AudioManager.instance.PlayLost();
        crashScoreText.text = currentScore.ToString();
        CrashPanel.SetActive(true);
        HubPanel.SetActive(false);
        crashScoreText.text = currentScore.ToString();
        inGame = false;
    }

    public void Revive()
    {
        AudioManager.instance.PlayRevive();
        CrashPanel.SetActive(false);
        HubPanel.SetActive(true);
        levelManager.ResetLevel();

        inGame = true;
    }

    public void GameOver()
    {
        newMySpawner.EnableSpawning = false;
        AudioManager.instance.PlayWin();
        spawner.StopGame();
        CrashPanel.SetActive(false);
        gameOverPanel.SetActive(true);

        FinalScoreText.text = currentScore.ToString();
        coinsText.text = inCoin.ToString();
        gemsText.text = inGem.ToString();
    }

    public void GameFinished()
    {
        lasScore = currentScore;

        if (currentScore > highScore)
        {
            // new high score
            highScore = currentScore;

            foreach(LeaderData me in leaderMe)
            {
                me.updateScore(highScore);
            }

            RearrangeLeaders();
        }

        HubPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        top.SetActive(true);

        levelManager.GameEnded();

        AudioManager.instance.PlayUITheme();
        EconomyManager.instance.GameFinishes(inCoin, inGem);
    }

    public void RearrangeLeaders()
    {
        leaders.Sort((a, b) => b.score.CompareTo(a.score));

        for (int i = 0; i < leaders.Count; i++)
        {
            leaders[i].transform.SetSiblingIndex(i);
        }
    }


    public void AddCoin()
    {
        inCoin++;   
        coinsTextG.text = inCoin.ToString();
    }
    public void AddGem()
    {
        inGem++;
        gemsTextG.text = inGem.ToString();
    }
    private void ResetGame()
    {
        currentScore = 0;
        inCoin = 0;
        inGem = 0;
    }
}
