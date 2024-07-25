using System.Collections.Generic;
using TMPro;

public class DialogueTagParser : ITextTagParser
{
    public List<ITextTag> TextTags { get => _textTags; }
    private List<ITextTag> _textTags = new List<ITextTag>();
    private DialogueTagSpeaker _tagSpeaker;
    private List<TextTagAnimation> _animations;

    public DialogueTagParser(DialogueTagSpeaker speaker, List<TextTagAnimation> animations = null)
    {
        _tagSpeaker = speaker;
        _textTags.Add(_tagSpeaker);
        _animations = animations;
        _textTags.AddRange(animations);
    }

    public void ParseTags(List<string> tags)
    {
        foreach (string tag in tags)
        {
            string[] s = SplitTag(tag);
            string tagKey = s[0].Trim();
            object tagValue = s[1].Trim();

            ITextTag _textTag = _textTags.Find(x => x.Tag == tagKey);
            if (_textTag == null)
            {
                continue;
            }
            _textTag.OnTagMentioned(tagValue);
        }
    }

    public string[] SplitTag(string tag)
    {
        string[] result = new string[2];
        string[] splitTag = tag.Split(':');
        result[0] = splitTag[0].Trim();
        result[1] = splitTag[1].Trim();
        return result;
    }
}
