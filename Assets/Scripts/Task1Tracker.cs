using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Task1Tracker : MonoBehaviour
{
    public Transform excavator;
    public Slider slider;

    int task = 1;

    public float l0, l1, l2, l3;

    public float buffer = 0.1f;

    void Start()
    {
        slider.maxValue = 3;
        slider.minValue = 0;
        slider.value = 0;
    }

    void Update()
    {
        float y = excavator.position.y;

        int level = GetLevel(y);

        if(level != -1)
        {
            slider.value = level;
        }

        if(level == 3){
            GameManager.instance.levelCompleted[task - 1] = true;
            StartCoroutine(UI_Interaction.Instance.PlayExitText());
        }
    }

    int GetLevel(float y)
    {
        if(excavator.rotation.eulerAngles.z < 2 && excavator.rotation.eulerAngles.z > -2)
        {
            if (y < l1 - buffer) return 0;
            else if (y < l2 - buffer) return 1;
            else if (y < l3 - buffer) return 2;
            else return 3;
        }
        return -1;
    }
}
