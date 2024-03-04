using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
//using System.Drawing;

[Serializable]
class KeywordHover
{
    public string keyword;
    public string description;
    public Color color = Color.white;
}

public class KeywordTooltip : MonoBehaviour
{
    public static KeywordTooltip instance;
    float displace;
    [SerializeField] List<KeywordHover> linkedKeywords = new();
    [SerializeField] List<KeywordHover> spriteKeywords = new();
    [SerializeField] TMP_Text tooltipText;

    private void Awake()
    {
        instance = this;
        displace = tooltipText.rectTransform.sizeDelta.y * 1.5f;
    }

    public string EditText(string text)
    {
        string answer = text.Replace("HP", "Health").Replace("MP", "Movement").Replace("EP", "Energy");
        foreach (KeywordHover link in linkedKeywords)
        {
            string pattern = $@"\b{Regex.Escape(link.keyword)}\b";
            answer = Regex.Replace(answer, pattern, $"<link=\"{link.keyword}\"><u><color=#{ColorUtility.ToHtmlStringRGB(link.color)}>{link.keyword}<color=#000000></u></link>");
        }
        foreach (KeywordHover link in spriteKeywords)
        {
            answer = answer.Replace(link.keyword, $"<link=\"{link.keyword}\"><sprite=\"Symbols\" name=\"{link.keyword}\"></link>");
        }
        return answer;
    }

    private void Update()
    {
        tooltipText.transform.parent.gameObject.SetActive(false);
    }

    public void ActivateTextBox(string keyword, Vector3 mousePosition)
    {
        tooltipText.transform.parent.gameObject.SetActive(true);
        this.transform.SetAsLastSibling();

        foreach (KeywordHover entry in linkedKeywords)
        {
            if (entry.keyword == keyword)
            {
                tooltipText.text = entry.description;
                tooltipText.transform.parent.position = mousePosition + (mousePosition.y > (displace)
                    ? new Vector3(0, -0.5f*displace, 0)
                    : new Vector3(0, 0.5f*displace, 0));
                return;
            }
        }
        foreach (KeywordHover entry in spriteKeywords)
        {
            if (entry.keyword == keyword)
            {
                tooltipText.text = entry.description;
                tooltipText.transform.parent.position = mousePosition + (mousePosition.y > (displace)
                    ? new Vector3(0, -0.5f * displace, 0)
                    : new Vector3(0, 0.5f * displace, 0));
                return;
            }
        }

    }
}
