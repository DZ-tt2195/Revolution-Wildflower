using TMPro;
using UnityEngine;

/// <summary>
/// Classes that inherit from <c>ITextAdvancer</c> are responsible for handling the advancement of text through a text file. 
/// </summary>
public interface ITextAdvancer
{
    /// <summary>
    /// The <c>TextAsset</c> file to read from.
    /// </summary>
    public TextAsset InkJSON { get; }
    /// <summary>
    /// The <c>TextMeshProUGUI</c> that will be passed to the <c>ITextRenderer</c> to print. 
    /// </summary>
    public TextMeshProUGUI GUI { get; }
    /// <summary>
    /// See interface definition for full details. Responsible for printing text.
    /// </summary>
    public ITextRenderer Renderer { get; }
    /// <summary>
    /// See interface definition for full details. This is a reference to the class that triggered the text.
    /// </summary>
    public ITextTrigger Trigger { get; }
}