using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; set; }

    public List<bool> levelCompleted;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        if (levelCompleted == null || levelCompleted.Count == 0)
        {
            levelCompleted = new List<bool>();

            for (int i = 0; i < 3; i++)
                levelCompleted.Add(false);
        }
    }
}
