using System.Collections.Generic;
using TMPro;

public class DialogueParser : ITextTagParser
{
    private DialogueTagSpeaker _tagSpeaker;
    private List<DialogueTagAnimation> _animations;

    public DialogueParser(TextMeshProUGUI speakerGUI, List<DialogueTagAnimation> animations)
    {
        _animations = animations;
        _tagSpeaker = new DialogueTagSpeaker(speakerGUI);
    }

    public void ParseTags(List<string> tags)
    {

    }
}
