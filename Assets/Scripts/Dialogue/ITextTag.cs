using UnityEditor.Build;

public interface ITextTag
{
    string Tag { get; }
    void OnTagMentioned(object content);
}
