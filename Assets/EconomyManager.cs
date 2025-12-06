using System;
using TMPro;
using UnityEngine;
using static UnityEditor.Progress;

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
        UpdateUI();
        CloseConfirmPanel();
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

    public void CloseConfirmPanel()
    {
        currentSelectedItem = null;
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    public void ConfirmPurchase()
    {
        if (currentSelectedItem == null) return;

        if (currentSelectedItem.coin)
        {
            if(currentSelectedItem.price <= gems)
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

    public void BuyAds(ItemData _coinNgems)
    {
        isAdFree = true;
        adButton.SetActive(true);

        AddCoins(_coinNgems.amount);
        AddGems(Mathf.RoundToInt(_coinNgems.price));


        UpdateUI();
    }

    // -------------------- DEBUG ADDERS --------------------
    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateUI();
    }

    public void AddGems(int amount)
    {
        gems += amount;
        UpdateUI();
    }
}
