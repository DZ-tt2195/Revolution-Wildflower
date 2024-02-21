using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[Serializable]
public class KeywordHover
{
    public string keyword;
    public string description;
}

public class KeywordTooltip : MonoBehaviour
{
    public static KeywordTooltip instance;
    [SerializeField] List<KeywordHover> listOfKeywords = new();
    [SerializeField] TMP_Text tooltipText;

    private void Awake()
    {
        instance = this;
    }

    public void ActivateTextBox(string keyword, Vector3 position)
    {
        foreach (KeywordHover entry in listOfKeywords)
        {
            if (entry.keyword == keyword)
            {
                tooltipText.text = entry.description;
                this.gameObject.SetActive(true);
                tooltipText.transform.parent.gameObject.SetActive(true);
                this.transform.SetAsLastSibling();
                tooltipText.transform.parent.position = position + new Vector3(0, -150, 0);
                return;
            }
        }

        this.gameObject.SetActive(false);
        Debug.LogError($"no description for {keyword}");
    }
}
