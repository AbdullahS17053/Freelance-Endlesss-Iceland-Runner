using UnityEngine;

public class MySaveLoadManager : MonoBehaviour
{
    public static MySaveLoadManager Instance;

    // ==========================
    // DEFAULT / RUNTIME VALUES
    // (Set these in Inspector if needed)
    // ==========================
    [Header("Initial Values (First Launch Only)")]
    public int InitialCoins = 0;
    public int InitialGems = 0;
    public int InitialBestScore = 0;
    public bool InitialAdsRemoved = false;
    public bool KeepResetAll;

    // ==========================
    // PLAYER DATA
    // ==========================
    public int Coins { get; private set; }
    public int Gems { get; private set; }
    public int BestScore { get; private set; }
    public bool AdsRemoved { get; private set; }

    // ==========================
    // PLAYER PREFS KEYS
    // ==========================
    private const string COINS_KEY = "Coins";
    private const string GEMS_KEY = "Gems";
    private const string SCORE_KEY = "BestScore";
    private const string ADS_KEY = "AdsRemoved";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (KeepResetAll)
        {
            ResetAllData();
        }
        else
        {
            LoadOrCreate();
        }
    }

    // ==========================
    // LOAD OR CREATE
    // ==========================
    private void LoadOrCreate()
    {
        // Coins
        if (PlayerPrefs.HasKey(COINS_KEY))
            Coins = PlayerPrefs.GetInt(COINS_KEY);
        else
            SaveCoins(InitialCoins);

        // Gems
        if (PlayerPrefs.HasKey(GEMS_KEY))
            Gems = PlayerPrefs.GetInt(GEMS_KEY);
        else
            SaveGems(InitialGems);

        // Best Score
        if (PlayerPrefs.HasKey(SCORE_KEY))
            BestScore = PlayerPrefs.GetInt(SCORE_KEY);
        else
            SaveBestScore(InitialBestScore);

        // Ads Removed
        if (PlayerPrefs.HasKey(ADS_KEY))
            AdsRemoved = PlayerPrefs.GetInt(ADS_KEY) == 1;
        else
            SetAdsRemoved(InitialAdsRemoved);
    }

    // ==========================
    // SAVE
    // ==========================
    public void SaveCoins(int value)
    {
        Coins = value;
        PlayerPrefs.SetInt(COINS_KEY, Coins);
        PlayerPrefs.Save();
    }

    public void SaveGems(int value)
    {
        Gems = value;
        PlayerPrefs.SetInt(GEMS_KEY, Gems);
        PlayerPrefs.Save();
    }

    public void SaveBestScore(int score)
    {
        if (score < BestScore) return;

        BestScore = score;
        PlayerPrefs.SetInt(SCORE_KEY, BestScore);
        PlayerPrefs.Save();
    }

    public void SetAdsRemoved(bool removed)
    {
        AdsRemoved = removed;
        PlayerPrefs.SetInt(ADS_KEY, removed ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ==========================
    // HELPERS
    // ==========================
    public void AddCoins(int amount)
    {
        SaveCoins(amount);
    }

    public void AddGems(int amount)
    {
        SaveGems(amount);
    }

    // ==========================
    // RESET (OPTIONAL)
    // ==========================
    private void ResetAllData()
    {
        PlayerPrefs.DeleteKey(COINS_KEY);
        PlayerPrefs.DeleteKey(GEMS_KEY);
        PlayerPrefs.DeleteKey(SCORE_KEY);
        PlayerPrefs.DeleteKey(ADS_KEY);
        PlayerPrefs.Save();

        LoadOrCreate();
    }
}
