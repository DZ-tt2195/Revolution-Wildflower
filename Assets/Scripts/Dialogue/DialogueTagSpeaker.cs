using TMPro;
using UnityEngine;
using System;

[Serializable]
public class DialogueTagSpeaker : ITextTag
{
    public string Tag { get => _tag; }
    [SerializeField] private string _tag;
    [SerializeField] private TextMeshProUGUI _gui;

    public DialogueTagSpeaker(string tag, TextMeshProUGUI gui)
    {
       _tag = tag;
        _gui = gui;
    }

    public void OnTagMentioned(object content)
    {
        string name = Convert.ToString(content);
        _gui.text = name; 
    }
}
