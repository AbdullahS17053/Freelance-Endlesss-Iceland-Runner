using TMPro;
using UnityEngine;

public class ItemData : MonoBehaviour
{
    public bool free;
    public string itemName;
    public string itemDescription;
    public float price;
    public bool coin;
    public int amount;
    public GameObject claimed;
    public int cost;
    public bool unlocked;
    public GameObject[] OnOffButtons;

    public void Refresh()
    {
        if (unlocked)
        {
            OnOffButtons[0].SetActive(true);
            OnOffButtons[1].SetActive(false);
        }
        else
        {
            OnOffButtons[0].SetActive(false);
            OnOffButtons[1].SetActive(true);
        }
    }
}
