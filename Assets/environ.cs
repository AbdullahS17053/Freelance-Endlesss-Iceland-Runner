using Coffee.UIEffects;
using UnityEngine;

public class environ : MonoBehaviour
{
    public bool unlocked = false;
    public int coins;
    public int gems;
    public GameObject Costs;
    public UIEffect effect;


    public void TryUnlock()
    {
        if (EconomyManager.instance.CanAfford(coins, gems))
        {
            unlocked = true;

            Costs.SetActive(false);
        }
    }
    public void SelectIt()
    {
        effect.enabled = true;
    }
    public void DeSelectIt()
    {
        effect.enabled = false;
    }
}
