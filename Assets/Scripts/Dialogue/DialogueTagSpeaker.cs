using TMPro;
using System;

public class DialogueTagSpeaker : IDialogueTag
{
    public string Tag { get; set; }
    private TextMeshProUGUI _gui;
    
    public DialogueTagSpeaker(TextMeshProUGUI gui)
    {
        _gui = gui;
    }
    public void OnTagMentioned(object content)
    {
        string name = Convert.ToString(content);
        _gui.text = name; 
    }
}
