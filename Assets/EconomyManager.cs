using System;
using TMPro;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager instance;

    [Header("Player Currency")]
    public int coins = 0;
    public int gems = 0;

    [Header("UI Elements")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI gemsText;
    public GameObject confirmPanel;
    public TextMeshProUGUI confirmItemText;

    public GameObject gameplayAdButton;
    public GameObject adButton;

    [Header("Ad-Free")]
    public bool isAdFree = false;

    private ItemData currentSelectedItem;

    private void Awake()
    {
        instance = this;
    }


    void Start()
    {
        LoadEconomyData();
        LoadAdFree();


        UpdateUI();
        CloseConfirmPanel();
    }

    private void LoadEconomyData()
    {
        coins = MySaveLoadManager.Instance.Coins;
        gems = MySaveLoadManager.Instance.Gems;
    }
    private void LoadAdFree()
    {
        isAdFree = MySaveLoadManager.Instance.AdsRemoved;

        if (isAdFree)
        {
            adButton.SetActive(true);
            gameplayAdButton.SetActive(false);
        }
    }

    public void GameFinishes(int c, int g)
    {
        coins += c;
        gems += g;
    
        UpdateUI();
    }
    void UpdateUI()
    {
        if (coinsText != null)
            coinsText.text = $"{coins}";

        if (gemsText != null)
            gemsText.text = $"{gems}";
    }

    // -------------------- CONFIRM PANEL --------------------
    public void OpenConfirmPanel(ItemData item)
    {
        currentSelectedItem = item;
        if (confirmPanel != null)
            confirmPanel.SetActive(true);

        if (item.coin)
        {
            confirmItemText.text = item.amount.ToString() + "x" + " Coins for " + item.price.ToString() + "Gems";
        }
        else
        {
            confirmItemText.text = item.amount.ToString() + "x" + " Gems for $" + item.price.ToString();
        }
            
    }
    public void OpenConfirmFreePanel(ItemData item)
    {
        currentSelectedItem = item;
        if (confirmPanel != null)
            confirmPanel.SetActive(true);

        if (item.coin)
        {
            confirmItemText.text = item.amount.ToString() + "x" + " Coins for Free";
        }
        else
        {
            confirmItemText.text = item.amount.ToString() + "x" + " Gems for Free";
        }
            
    }

    public void CloseConfirmPanel()
    {
        currentSelectedItem = null;
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    public void ConfirmPurchase()
    {
        if (currentSelectedItem == null) return;

        if (currentSelectedItem.free)
        {
            if(currentSelectedItem.price <= gems)
            {
                AddCoins(currentSelectedItem.amount);
                currentSelectedItem.claimed.SetActive(true);

                UpdateUI();
                CloseConfirmPanel();
            }
        }
        else
        {
            if (currentSelectedItem.coin)
            {
                if (currentSelectedItem.price <= gems)
                {
                    AddCoins(currentSelectedItem.amount);
                    gems -= (int)MathF.Ceiling(currentSelectedItem.price);


                    UpdateUI();
                    CloseConfirmPanel();
                }
            }
            else
            {
                AddGems(currentSelectedItem.amount);


                UpdateUI();
                CloseConfirmPanel();
            }
        }

        FakeAdsManager.Instance.ShowInterstitial();
    }

    public void BuyAds(ItemData _coinNgems)
    {

        isAdFree = true;
        adButton.SetActive(true);
        gameplayAdButton.SetActive(false);

        AddCoins(_coinNgems.amount);
        AddGems(Mathf.RoundToInt(_coinNgems.price));

        MySaveLoadManager.Instance.SetAdsRemoved(true);
        UpdateUI();
    }

    // -------------------- DEBUG ADDERS --------------------
    public void AddCoins(int amount)
    {
        coins += amount;
        MySaveLoadManager.Instance.AddCoins(coins);
        UpdateUI();
    }
    public void RemoveCoins(int amount)
    {
        coins -= amount;
        MySaveLoadManager.Instance.AddCoins(coins);
        UpdateUI();
    }

    public void AddGems(int amount)
    {
        gems += amount;
        MySaveLoadManager.Instance.AddGems(gems);
        UpdateUI();
    }
    public bool RemoveGems(int amount)
    {
        if(gems > amount)
        {
            gems -= amount;
            MySaveLoadManager.Instance.AddGems(gems);
            UpdateUI();
            return true;
        }
        else
        {
            return false;
        }
    }
}
