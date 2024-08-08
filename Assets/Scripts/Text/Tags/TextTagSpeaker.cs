using TMPro;
using UnityEngine;
using System;

/// <summary>
/// This <c>ITextTag</c> displays who is speaking. 
/// </summary>
[Serializable]
public class TextTagSpeaker : ITextTag
{
    /// <summary>
    /// The tag to look out for. Passed in through constructor. Typically "speaker". 
    /// </summary>
    public string Tag { get => _tag; }
    public TextMeshProUGUI GUI { get => _gui; }
    [SerializeField] private string _tag;
    [SerializeField] private TextMeshProUGUI _gui;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="tag">The tag to look out for. Typically "speaker". </param>
    /// <param name="gui">The <c>TextMeshProUGUI</c> to print to.</param>
    public TextTagSpeaker(string tag, TextMeshProUGUI gui)
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
