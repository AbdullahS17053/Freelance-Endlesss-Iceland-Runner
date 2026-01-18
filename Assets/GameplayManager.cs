using AssetKits.ParticleImage;
using DG.Tweening;
using MoreMountains.InfiniteRunnerEngine;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SocialPlatforms.Impl;
using Random = UnityEngine.Random;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager instance;

    public MyLevelManager levelManager;

    [Header("Menu UI")]
    public GameObject mainMenuPanel;
    public GameObject top;

    [Header("Gameplay UI")]
    public GameObject HubPanel;


    [Header("Timer UI")]
    public GameObject TimerPanel;
    public GameObject TimerIntro;
    public GameObject TimerAurora;
    public GameObject TimerAurora1;
    public TextMeshProUGUI TimerTimeText;
    public TextMeshProUGUI TimerTimeMulti;
    public int TimerBonusMultiplayer;
    public Vector2 TimerTime;
    public Vector2 TimerSpawn;
    private float timerRemaining;
    public bool timerActive;
    public event Action<bool> OnTimerActiveChanged;

    #region

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
    public ParticleImage coinParticle;
    public ParticleImage gemParticle;


    [Header("Gameplay Variables")]
    public int revives = 3;
    public bool inGame;
    public int currentScore = 0;
    public int scoreMultiplier = 1;
    public int scoreMultiplierBoaster = 1;
    public int highScore = 0;
    public int lasScore = 0;
    public TextMeshProUGUI revivesText;
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI coinsTextG;
    public TextMeshProUGUI gemsTextG;
    public SmartObstacleSpawner newMySpawner;


    [Header("Leaderboard")]
    public List<LeaderData> leaders;
    public LeaderData[] leaderMe;

    private void Awake()
    {
        instance = this;
        timerRemaining = Random.Range(TimerSpawn.x, TimerSpawn.y);
    }

    private void Start()
    {
        AudioManager.instance.PlayUITheme();
        TimerTimeMulti.text = TimerBonusMultiplayer.ToString() + "X";

        LoadScore();
    }

    private void LoadScore()
    {
        currentScore = MySaveLoadManager.Instance.BestScore;

        lasScore = currentScore;

        if (currentScore > highScore)
        {
            // new high score
            highScore = currentScore;

            foreach (LeaderData me in leaderMe)
            {
                me.updateScore(highScore);
            }

            RearrangeLeaders();
        }
    }

    private void Update()
    {
        if (inGame)
        {
            currentScore += Mathf.RoundToInt(10 * scoreMultiplier * scoreMultiplierBoaster * Time.deltaTime);
            currentScoreText.text = currentScore.ToString();
        }
    }

    public void PlayGame()
    {
        ResetGame();

        AudioManager.instance.PlayGameplayTheme();
        levelManager.StartGame();

        mainMenuPanel.SetActive(false);
        top.SetActive(false);
        HubPanel.SetActive(true);
        inGame = true;

        newMySpawner.EnableSpawning = true;
        coinsTextG.text = inCoin.ToString();
        gemsTextG.text = inGem.ToString();

        coinParticle.emitterConstraintTransform = levelManager._playerInstance.transform;
        gemParticle.emitterConstraintTransform = levelManager._playerInstance.transform;
    }



    public void Crashed()
    {
        AudioManager.instance.PlayLost();
        crashScoreText.text = currentScore.ToString();
        CrashPanel.SetActive(true);
        HubPanel.SetActive(false);
        crashScoreText.text = currentScore.ToString();
        levelManager.CrashLevel();
        inGame = false;
    }

    public void Revive()
    {
        if (revives == 0) return;

        FakeAdsManager.Instance.ShowRewarded();
        revives--;
        revivesText.text = revives.ToString();

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
        CrashPanel.SetActive(false);
        gameOverPanel.SetActive(true);

        FinalScoreText.text = currentScore.ToString();
        coinsText.text = inCoin.ToString();
        gemsText.text = inGem.ToString();
    }

    public void GameFinished()
    {
        StopTimerBooster();
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

            MySaveLoadManager.Instance.SaveBestScore(highScore);
        }

        HubPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        top.SetActive(true);

        levelManager.EndGame();

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
        inCoin += scoreMultiplierBoaster;
        coinParticle.DOPlay();


        coinsTextG.text = inCoin.ToString();
    }
    public void AddGem()
    {
        inGem++;
        gemParticle.DOPlay();
        gemsTextG.text = inGem.ToString();
    }

    #endregion

    private void FixedUpdate()
    {
        if (!inGame && !timerActive) return;


        timerRemaining -= Time.fixedDeltaTime;


        if (timerRemaining <= 0f)
        {
            if (!timerActive)
            {
                // StartTimerBooster();
            }
            else
            {
                StopTimerBooster();
            }
        }

        if (!inGame || !timerActive) return;

        TimerTimeText.text = Mathf.CeilToInt(timerRemaining) + "s";
    }



    public void StartTimerBooster()
    {
        timerActive = true;
        OnTimerActiveChanged?.Invoke(timerActive);

        timerRemaining = Random.Range(TimerTime.x, TimerTime.y);

        TimerPanel.SetActive(true);
        TimerIntro.SetActive(true);
        TimerAurora.SetActive(true);
        Debug.Log("Timer Started");

        scoreMultiplierBoaster = TimerBonusMultiplayer;

        PlayTimerIntro();
    }


    private void StopTimerBooster()
    {
        timerActive = false;
        OnTimerActiveChanged?.Invoke(timerActive);

        timerRemaining = Random.Range(TimerSpawn.x, TimerSpawn.y);

        TimerPanel.SetActive(false);
        TimerIntro.SetActive(false);
        TimerAurora.SetActive(false);
        TimerAurora1.SetActive(false);
        Debug.Log("Timer End");

        scoreMultiplierBoaster = 1;
    }
    public void AddTimer()
    {
        if (timerActive)
        {
            timerRemaining += Random.Range(TimerTime.x, TimerTime.y);

            TimerAurora1.SetActive(true);
        }
        else
        {
            StartTimerBooster();
        }
    }


    public void PlayTimerIntro()
    {
        RectTransform rt = TimerIntro.GetComponent<RectTransform>();

        // Kill previous tweens (important if reused)
        rt.DOKill();

        // Start position (left off-screen)
        rt.anchoredPosition = new Vector2(-1000f, rt.anchoredPosition.y);


        // Create sequence
        Sequence seq = DOTween.Sequence();

        seq.Append(
            rt.DOAnchorPosX(0f, 0.4f)
            .SetRelative(true)
              .SetEase(Ease.OutQuad)
        );

        seq.AppendInterval(0.6f); // stay in center

        seq.Append(
            rt.DOAnchorPosX(1000f, 0.4f)
            .SetRelative(true)
              .SetEase(Ease.InQuad)
        );

        seq.OnComplete(() =>
        {
            TimerIntro.SetActive(false); // optional
        });
    }

    private void ResetGame()
    {
        revives = 3;
        revivesText.text = revives.ToString();
        currentScore = 0;
        inCoin = 0;
        inGem = 0;
    }
}
