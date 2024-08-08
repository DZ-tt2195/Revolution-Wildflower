using TMPro;
using UnityEngine;

/// <summary>
/// This class holds a default implementation of <c>ITextRenderer</c>, useful in most situations.
/// </summary>
public class TextRenderer : ITextRenderer
{
    public TextMeshProUGUI GUI
    {
        get => _gui;
    }

    public ITextRenderStyle Style
    {
        get => _style;
    }

    private TextMeshProUGUI _gui;
    private ITextRenderStyle _style;
    /// <summary>
    /// Constructor for <c>TextRenderer</c>
    /// </summary>
    /// <param name="gui">The <c>TextMeshProUGUI</c> that text will be printed to.</param>
    /// <param name="style">The <c>ITextRenderStyle</c> that affects how the text will be printed.</param>
    public TextRenderer(TextMeshProUGUI gui, ITextRenderStyle style)
    {
        _gui = gui;
        _style = style;
    }

    /// <summary>
    /// Renders the text in the style according to the <c>ITextRenderStyle</c>
    /// </summary>
    /// <param name="content"></param>
    public void DisplayLine(string content)
    {
        _style.Render(content);
    }
}

