using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Task3Tracker : MonoBehaviour
{
    int task = 3;

    [Header("Tracking")]
    public int maxCount = 10;

    [Header("Tags")]
    public string tag1 = "FullBrick";
    public string tag2 = "HalfBrick";
    public int weight1 = 1;
    public int weight2 = 2;

    [Header("UI")]
    public Slider slider;
    public TextMeshProUGUI percentText;

    [Header("Billboard")]
    public Transform excavator;

    // Store object and its weight
    private Dictionary<GameObject, int> objectsInside = new Dictionary<GameObject, int>();

    private int currentWeight = 0;
    private bool levelCompleted = false;

    void Start()
    {
        slider.minValue = 0;
        slider.maxValue = maxCount;

        UpdateUI();
    }

    void OnTriggerEnter(Collider other)
    {
        int weight = GetWeight(other);

        if (weight == 0) return; // ignore unwanted objects

        if (!objectsInside.ContainsKey(other.gameObject))
        {
            objectsInside.Add(other.gameObject, weight);
            currentWeight += weight;

            UpdateUI();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (objectsInside.TryGetValue(other.gameObject, out int weight))
        {
            currentWeight -= weight;
            objectsInside.Remove(other.gameObject);

            UpdateUI();
        }
    }

    int GetWeight(Collider other)
    {
        if (other.CompareTag(tag1)) return weight1;
        if (other.CompareTag(tag2)) return weight2;

        return 0;
    }

    void UpdateUI()
    {
        int clamped = Mathf.Min(currentWeight, maxCount);

        slider.value = clamped;

        float percent = (maxCount == 0) ? 0 : (float)clamped / maxCount * 100f;
        percentText.text = Mathf.RoundToInt(percent) + "%";

        if (!levelCompleted && currentWeight >= maxCount)
        {
            levelCompleted = true;

            GameManager.instance.levelCompleted[task - 1] = true;
            StartCoroutine(UI_Interaction.Instance.PlayExitText());

            Debug.Log("Task Completed!");
        }
    }

    void LateUpdate()
    {
        if (excavator != null)
        {
            Vector3 direction = excavator.position - slider.transform.position;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                slider.transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}