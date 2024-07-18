using System.Collections.Generic;
public interface ITextTagParser
{
    public string[] SplitTag(string tag);
    public void ParseTags(List<string> tags);
}