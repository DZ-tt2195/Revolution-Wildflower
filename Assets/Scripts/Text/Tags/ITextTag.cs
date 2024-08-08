using UnityEditor.Build;

/// <summary>
/// Classes that inherit from <c>ITextTag</c> are responsible for doing something with tags from an Ink file.
/// </summary>
public interface ITextTag
{
    /// <summary>
    /// The tag to look out for.
    /// </summary>
    string Tag { get; }
    /// <summary>
    /// What to do when the tag is mentioned.
    /// </summary>
    /// <param name="content">The matching content passed in through the Ink file (eg. the name of a speaker).</param>
    void OnTagMentioned(object content);
}
