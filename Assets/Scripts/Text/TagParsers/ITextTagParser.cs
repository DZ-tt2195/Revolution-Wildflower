using System.Collections.Generic;
public interface ITextTagParser
{
    public List<ITextTag> TextTags { get; }

    public string[] SplitTag(string tag);
    public void ParseTags(List<string> tags);
}