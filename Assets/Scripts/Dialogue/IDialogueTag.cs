public interface IDialogueTag
{
    string Tag { get; set; }
    void OnTagMentioned(object content);
}
