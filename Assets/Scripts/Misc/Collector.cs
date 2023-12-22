using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;

public class Collector : MonoBehaviour
{
    [SerializeField] TMP_Text textbox;
    RectTransform imageWidth;
    Canvas canvas;

    [SerializeField] Button textButton;
    List<Button> buttonsInCollector = new List<Button>();
    [ReadOnly] public int chosenButton = -1;

    void Awake()
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        imageWidth = this.transform.GetChild(0).GetComponent<RectTransform>();
    }

    public void StatsSetup(string header, Vector3 position)
    {
        this.textbox.text = header;
        this.transform.SetParent(canvas.transform);
        this.transform.localPosition = position;
        this.transform.localScale = new Vector3(1, 1, 1);
    }

    public void DestroyButton(int sibling)
    {
        Button toDestroy = this.transform.GetChild(2).transform.GetChild(sibling).GetComponent<Button>();
        buttonsInCollector.Remove(toDestroy);
        Destroy(toDestroy.gameObject);

        if (this.transform.GetChild(2).transform.childCount <= 1)
            Destroy(this.gameObject);
    }

    public void AddTextButton(string text)
    {
        Button nextButton = Instantiate(textButton, this.transform.GetChild(2));
        nextButton.transform.GetChild(0).GetComponent<TMP_Text>().text = text;

        nextButton.enabled = true;
        int buttonNumber = buttonsInCollector.Count;
        nextButton.onClick.AddListener(() => ReceiveChoice(buttonNumber));
        buttonsInCollector.Add(nextButton);

        if (this.transform.GetChild(2).childCount <= 2)
            imageWidth.sizeDelta = new Vector2(500, 240);
        else
            imageWidth.sizeDelta = new Vector2(250 * (this.transform.GetChild(2).childCount), 240);
    }

    void ReceiveChoice(int buttonNumber)
    {
        chosenButton = buttonNumber;
    }

    public void DisableAll()
    {
        foreach (Button x in buttonsInCollector)
        {
            try{x.enabled = false;}
            catch (NullReferenceException){continue;}
        }
    }

    public void EnableAll()
    {
        foreach (Button x in buttonsInCollector)
        {
            try{x.enabled = true;}
            catch (NullReferenceException){continue;}
        }
    }
}
