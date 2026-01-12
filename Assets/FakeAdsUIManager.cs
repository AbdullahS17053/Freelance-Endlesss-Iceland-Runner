using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FakeAdsUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject InterstitialPanel;
    public GameObject RewardedPanel;

    [Header("Rewarded UI")]
    public Slider RewardedProgressBar;
    public Button RewardedCloseButton;

    [Header("Interstitial UI")]
    public Button InterstitialCloseButton;

    private void Start()
    {
        // Hide panels at start
        InterstitialPanel.SetActive(false);
        RewardedPanel.SetActive(false);

        // Hook events
        FakeAdsManager.Instance.OnInterstitialStarted.AddListener(ShowInterstitialUI);
        FakeAdsManager.Instance.OnInterstitialFinished.AddListener(HideInterstitialUI);

        FakeAdsManager.Instance.OnRewardedStarted.AddListener(ShowRewardedUI);
        FakeAdsManager.Instance.OnRewardedFinished.AddListener(HideRewardedUI);

        FakeAdsManager.Instance.OnRewardGranted.AddListener(() =>
        {
            Debug.Log("Reward granted!");
            // Add coins, gems, revive, etc
        });

        FakeAdsManager.Instance.OnRewardSkipped.AddListener(() =>
        {
            Debug.Log("Reward skipped");
        });

        // Buttons
        InterstitialCloseButton.onClick.AddListener(() =>
        {
            FakeAdsManager.Instance.ForceCloseAd();
        });

        RewardedCloseButton.onClick.AddListener(() =>
        {
            FakeAdsManager.Instance.ForceCloseAd();
        });
    }

    // ==========================
    // INTERSTITIAL
    // ==========================
    private void ShowInterstitialUI()
    {
        InterstitialPanel.SetActive(true);
        InterstitialCloseButton.interactable = true; // Allow immediate skip if desired
    }

    private void HideInterstitialUI()
    {
        InterstitialPanel.SetActive(false);
    }

    // ==========================
    // REWARDED
    // ==========================
    private void ShowRewardedUI()
    {
        RewardedPanel.SetActive(true);
        RewardedCloseButton.interactable = false; // Disable close until timer done
        StartCoroutine(RewardedTimerCoroutine());
    }

    private void HideRewardedUI()
    {
        RewardedPanel.SetActive(false);
    }

    private IEnumerator RewardedTimerCoroutine()
    {
        float duration = FakeAdsManager.Instance.RewardedDuration;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            if (RewardedProgressBar != null)
                RewardedProgressBar.value = timer / duration;
            yield return null;
        }

        // Timer done → enable close button
        if (RewardedCloseButton != null)
            RewardedCloseButton.interactable = true;
    }
}
