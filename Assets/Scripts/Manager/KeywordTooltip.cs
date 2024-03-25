using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
//using System.Drawing;

[Serializable]
public class KeywordHover
{
    public string keyword;
    public string description;
    public Color color = Color.white;
}

public class KeywordTooltip : MonoBehaviour
{
    public static KeywordTooltip instance;
    float XCap;
    float Ydisplace;
    [SerializeField] List<KeywordHover> linkedKeywords = new();
    [SerializeField] List<KeywordHover> spriteKeywords = new();
    [SerializeField] TMP_Text tooltipText;

    private void Awake()
    {
        instance = this;
        XCap = tooltipText.rectTransform.sizeDelta.x / 2f;
        Ydisplace = tooltipText.rectTransform.sizeDelta.y * 1.25f;
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

    public KeywordHover SearchForKeyword(string target)
    {
        foreach (KeywordHover link in linkedKeywords)
        {
            if (link.keyword.Equals(target))
                return link;
        }
        foreach (KeywordHover link in spriteKeywords)
        {
            if (link.keyword.Equals(target))
                return link;
        }
        Debug.LogError($"{target} couldn't be found");
        return null;
    }

    private void Update()
    {
        tooltipText.transform.parent.gameObject.SetActive(false);
    }

    Vector2 CalculatePosition(Vector3 mousePosition)
    {
        return new Vector3
            (Mathf.Clamp(mousePosition.x, XCap, Screen.width - XCap),
            mousePosition.y > Ydisplace ? mousePosition.y + (-0.5f * Ydisplace) : mousePosition.y + (0.5f * Ydisplace));
    }

    void SetPosition(string description, Vector3 mousePosition, bool screenOverlay)
    {
        if (screenOverlay)
        {
            tooltipText.text = description;
            Vector2 newPosition = CalculatePosition(mousePosition);
            tooltipText.transform.parent.position = new Vector3(newPosition.x, newPosition.y, 100);
        }
        else
        {
            tooltipText.text = description;
            Vector2 newPosition = (CalculatePosition(Camera.main.ScreenToWorldPoint(mousePosition)));
            tooltipText.transform.parent.position = (new Vector3(newPosition.x, newPosition.y, 100));
        }
    }

    public void ActivateTextBox(string target, Vector3 mousePosition, bool screenOverlay)
    {
        tooltipText.transform.parent.gameObject.SetActive(true);
        this.transform.SetAsLastSibling();

        foreach (KeywordHover entry in linkedKeywords)
        {
            if (entry.keyword.Equals(target))
            {
                SetPosition(entry.description, mousePosition, screenOverlay);
                return;
            }
        }
        foreach (KeywordHover entry in spriteKeywords)
        {
            if (entry.keyword.Equals(target))
            {
                SetPosition(entry.description, mousePosition, screenOverlay);
                return;
            }
        }
    }
}