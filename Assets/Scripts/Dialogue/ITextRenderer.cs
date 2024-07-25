using TMPro;

/// <summary>
/// Classes that inherit from <c>ITextRenderer</c> are responsible for printing content to a <c>TextMeshProUGUI</c>. 
/// </summary>
public interface ITextRenderer
{
    /// <summary>
    /// The <c>TextMeshProUGUI</c> that is being printed to. 
    /// </summary>
    public TextMeshProUGUI GUI { get; }
    /// <summary>
    /// See interface definition for full details. Defines a style of revealing text. 
    /// </summary>
    public ITextRenderStyle Style { get; }
    /// <summary>
    /// Handles the printing of a line of text.
    /// </summary>
    /// <param name="content">The string that is being printed.</param>
    public void DisplayLine(string content);
}