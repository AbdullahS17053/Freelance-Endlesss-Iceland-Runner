using TMPro;
using UnityEngine;

// This is the "brain" of the upgrade system.
// Every other script reads from this to know what the player has bought.
// It's a Singleton so you can access it from anywhere with UpgradeManager.Instance

public class UpgradeManager : MonoBehaviour
{

    // ── Singleton Setup ──────────────────────────────────────────────────────
    public static UpgradeManager Instance;

    public GameObject upgradePanel; // Drag in from Inspector
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI DescriptionText;

    public ItemData[] upgradeButtons; // Drag in from Inspector

    private int currentUpgradeIndex = 0; // To track which upgrade is currently being displayed

    // DoubleAuroraTime
    // TripleAuroraTime
    // Shield
    // MegaShield
    // DoubleCoins
    // DoubleGems
    // ScoreMultiplier2x
    // ScoreMultiplier3x

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Only ever one copy
        }
    }

    public void ShowUpgradePanel(int upgradeIndex)
    {
        currentUpgradeIndex = upgradeIndex;
        nameText.text = upgradeButtons[upgradeIndex].itemName;
        DescriptionText.text = upgradeButtons[upgradeIndex].itemDescription;
        upgradePanel.SetActive(true);
    }

    public void buy()
    {
        if(EconomyManager.instance.RemoveGems(upgradeButtons[currentUpgradeIndex].cost))
        {
            upgradeButtons[currentUpgradeIndex].unlocked = true;
            upgradeButtons[currentUpgradeIndex].Refresh();
            upgradePanel.SetActive(false);
        }
        else
        {
            upgradePanel.SetActive(false);
        }
    }

    // Upgrades
    // ✅ CORRECT — always check stronger tier first
    public int AuroraTime()
    {
        if (upgradeButtons[1].unlocked) return 3;  // Triple first
        if (upgradeButtons[0].unlocked) return 2;  // then Double
        return 1;
    }

    public int Shield()
    {
        if (upgradeButtons[3].unlocked) return 5;  // MegaShield first
        if (upgradeButtons[2].unlocked) return 2;  // then Shield
        return 0;
    }

    // ScoreMultiplier has the same bug — fix it too
    public int ScoreMultiplier()
    {
        if (upgradeButtons[7].unlocked) return 3;  // 3x first
        if (upgradeButtons[6].unlocked) return 2;  // then 2x
        return 1;
    }
    public int CoinMultiplier()
    {
        if (upgradeButtons[4].unlocked)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }
    public int GemMultiplier()
    {
        if (upgradeButtons[5].unlocked)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }
}