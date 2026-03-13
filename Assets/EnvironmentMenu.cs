using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentMenu : MonoBehaviour
{
    [System.Serializable]
    public class ENV
    {
        public GameObject environment;
        public GameObject _1;
        public GameObject _2;
        public GameObject _3;
    }

    public MyLevelManager levelManager;
    public SmartObstacleSpawner[] smartObstacleSpawner;
    public RectTransform moving;

    public ENV[] envs;


    public environ[] Environs;

    public Button[] buttons;

    public GameObject lockedPanel;

    public float move;
    public int selection;

    public void Select()
    {
        if(Environs[selection].unlocked)
        {
            for (int i = 0; i < envs.Length; i++)
            {
                envs[i].environment.SetActive(false);
                smartObstacleSpawner[i].gameObject.SetActive(false);
                smartObstacleSpawner[i].EnableSpawning = false;
            }

            envs[selection].environment.SetActive(true);
            levelManager.ChangeLevel(envs[selection]._1, envs[selection]._2, envs[selection]._3);

            smartObstacleSpawner[selection].gameObject.SetActive(true);
            levelManager.spawner = smartObstacleSpawner[selection];
        }
    }
    public void TryUnlock()
    {
        Environs[selection].TryUnlock();
        if (Environs[selection].unlocked)
        {
            buttons[0].gameObject.SetActive(true);
            buttons[1].gameObject.SetActive(false);
        }
        else
        {
            lockedPanel.SetActive(true);
        }
    }

    public void MoveRight()
    {
        Environs[selection].DeSelectIt();
        selection++;
        if (selection > 2)
        {
            selection = 2;
        }
        else
        {
            float with = moving.rect.width/2;
            moving.DOAnchorPosX(-with, 0.2f).SetRelative(true);
        }
            Environs[selection].SelectIt();

        if(Environs[selection].unlocked)
        {
            buttons[0].gameObject.SetActive(true);
            buttons[1].gameObject.SetActive(false);
        }
        else
        {
            buttons[0].gameObject.SetActive(false);
            buttons[1].gameObject.SetActive(true);
        }
    }
    public void MoveLeft()
    {
        Environs[selection].DeSelectIt();
        selection--;
        if (selection < 0)
        {
            selection = 0;
        }
        else
        {
            float with = moving.rect.width/2;
            moving.DOAnchorPosX(with, 0.2f).SetRelative(true);
        }
        Environs[selection].SelectIt();

        if (Environs[selection].unlocked)
        {
            buttons[0].gameObject.SetActive(true);
            buttons[1].gameObject.SetActive(false);
        }
        else
        {
            buttons[0].gameObject.SetActive(false);
            buttons[1].gameObject.SetActive(true);
        }
    }
}
