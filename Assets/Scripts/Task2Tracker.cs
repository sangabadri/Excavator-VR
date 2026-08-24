using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Task2Tracker : MonoBehaviour
{
    public static Task2Tracker Instance { get; set; }

    [Header("UI Settings")]
    private List<GameObject> sliderUIs;
    public List<Slider> sliders;
    public List<TextMeshProUGUI> percentTexts;
    public List<int> maxValues;

    int task = 2;

    [Header("Billboard Settings")]
    public Transform excavator;

    int activeSlider = -1;

    private List<int> values = new List<int>();

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        // Initialize the list so it isn't null
        sliderUIs = new List<GameObject>();

        for (int i = 0; i < sliders.Count; i++)
        {
            values.Add(0);

            sliders[i].minValue = 0;
            sliders[i].maxValue = maxValues[i];
            sliders[i].value = 0;

            // Use .Add() instead of [i] = ...
            // This takes the GameObject the slider is attached to
            sliderUIs.Add(sliders[i].gameObject);

            sliderUIs[i].SetActive(false);
            UpdateUI(i);
        }
    }

    public void UpdateSlider(int index)
    {
        if (index < 0 || index >= values.Count) return;

        values[index]++;
        UpdateUI(index);

        // Scene Transition Logic
        if (values[index] >= maxValues[index])
        {
            GameManager.instance.levelCompleted[task-1] = true;
            StartCoroutine(UI_Interaction.Instance.PlayExitText());
            return; // Stop here if we are changing scenes
        }

        // Show UI and handle independent Coroutine
        if (activeSlider != index && activeSlider != -1)
        {
            sliderUIs[activeSlider].SetActive(false);
        }
        sliderUIs[index].SetActive(true);
        activeSlider = index;

    }

    void UpdateUI(int index)
    {
        int clamped = Mathf.Min(values[index], maxValues[index]);
        sliders[index].value = clamped;

        float percent = (maxValues[index] == 0) ? 0 : (float)clamped / maxValues[index] * 100f;
        percentTexts[index].text = $"{Mathf.RoundToInt(percent)}%"; // Cleaner string formatting
    }

    void LateUpdate()
    {
        if (excavator == null) return;

        for (int i = 0; i < sliderUIs.Count; i++)
        {
            // Only calculate rotation if the UI is actually visible
            if (sliderUIs[i].activeInHierarchy)
            {
                RotateToFaceTarget(sliderUIs[i].transform);
            }
        }
    }

    void RotateToFaceTarget(Transform uiTransform)
    {
        Vector3 direction = excavator.position - uiTransform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            uiTransform.rotation = Quaternion.LookRotation(direction);
        }
    }
}