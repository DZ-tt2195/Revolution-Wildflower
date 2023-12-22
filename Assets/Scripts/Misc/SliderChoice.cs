using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;

public class SliderChoice : MonoBehaviour
{
    Canvas canvas;

    [SerializeField] TMP_Text textbox;
    [SerializeField] Button confirmButton;

    [SerializeField] Slider slider;
    [ReadOnly] internal int currentSliderValue = 0;
    [ReadOnly] internal bool makingDecision = true;

    [SerializeField] TMP_Text minimumText;
    [SerializeField] TMP_Text maximumText;
    [SerializeField] TMP_Text currentText;

    private void Awake()
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        slider.onValueChanged.AddListener(UpdateText);
        confirmButton.onClick.AddListener(ConfirmDecision);
    }

    public void StatsSetup(string header, int min, int max, Vector3 position)
    {
        this.textbox.text = header;
        this.transform.SetParent(canvas.transform);
        this.transform.localPosition = position;
        this.transform.localScale = new Vector3(1, 1, 1);

        minimumText.text = min.ToString();
     	slider.value = min;
   	slider.minValue = min;
        maximumText.text = max.ToString();
        slider.maxValue = max;
    }

    void UpdateText(float value)
    {
        currentText.text = $"{(int)value}";
        currentSliderValue = (int)value;
    }

    void ConfirmDecision()
    {
        makingDecision = false;
    }
}
