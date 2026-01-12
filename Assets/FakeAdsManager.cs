using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FakeAdsManager : MonoBehaviour
{
    public static FakeAdsManager Instance;

    // ==========================
    // SETTINGS
    // ==========================
    [Header("Fake Interstitial")]
    public float InterstitialDuration = 2.5f;
    public float InterstitialCooldown = 20f;

    [Header("Fake Rewarded")]
    public float RewardedDuration = 5f;
    public float RewardedCooldown = 15f;

    // ==========================
    // EVENTS
    // ==========================
    [Header("Interstitial Events")]
    public UnityEvent OnInterstitialStarted;
    public UnityEvent OnInterstitialFinished;

    [Header("Rewarded Events")]
    public UnityEvent OnRewardedStarted;
    public UnityEvent OnRewardedFinished;
    public UnityEvent OnRewardGranted;
    public UnityEvent OnRewardSkipped;

    // ==========================
    // INTERNAL
    // ==========================
    private bool _showingAd;
    private float _lastInterstitialTime = -999f;
    private float _lastRewardedTime = -999f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        _showingAd = MySaveLoadManager.Instance.AdsRemoved;
    }

    // ==========================
    // INTERSTITIAL
    // ==========================
    public bool CanShowInterstitial()
    {
        if (_showingAd) return false;
        if (Time.unscaledTime - _lastInterstitialTime < InterstitialCooldown)
            return false;

        if (IsAdsRemoved()) return false;

        return true;
    }

    public void ShowInterstitial()
    {
        if (!CanShowInterstitial())
            return;

        StartCoroutine(InterstitialRoutine());
    }

    private IEnumerator InterstitialRoutine()
    {
        _showingAd = true;
        _lastInterstitialTime = Time.unscaledTime;

        OnInterstitialStarted?.Invoke();

        yield return new WaitForSecondsRealtime(InterstitialDuration);

        _showingAd = false;

        OnInterstitialFinished?.Invoke();
    }

    // ==========================
    // REWARDED
    // ==========================
    public bool CanShowRewarded()
    {
        if (_showingAd) return false;
        if (Time.unscaledTime - _lastRewardedTime < RewardedCooldown)
            return false;

        return true;
    }

    public void ShowRewarded()
    {
        if (IsAdsRemoved())
        {
            OnRewardGranted?.Invoke();
            return;
        }

        if (!CanShowRewarded())
        {
            OnRewardSkipped?.Invoke();
            return;
        }

        StartCoroutine(RewardedRoutine());
    }

    private IEnumerator RewardedRoutine()
    {
        _showingAd = true;
        _lastRewardedTime = Time.unscaledTime;

        Time.timeScale = 0f;
        OnRewardedStarted?.Invoke();

        yield return new WaitForSecondsRealtime(RewardedDuration);

        Time.timeScale = 1f;
        _showingAd = false;

        OnRewardedFinished?.Invoke();
        OnRewardGranted?.Invoke();
    }

    // ==========================
    // HELPERS
    // ==========================
    private bool IsAdsRemoved()
    {
        return MySaveLoadManager.Instance != null &&
               MySaveLoadManager.Instance.AdsRemoved;
    }

    public bool IsShowingAd() => _showingAd;

    public void ForceCloseAd()
    {
        if (!_showingAd) return;

        StopAllCoroutines();
        Time.timeScale = 1f;
        _showingAd = false;
        OnRewardSkipped?.Invoke();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && _showingAd)
            ForceCloseAd();
    }
}
