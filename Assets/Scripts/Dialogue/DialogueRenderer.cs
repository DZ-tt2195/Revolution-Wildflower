using TMPro;
using UnityEngine;
/// <summary>
/// The <c>DialogueRenderer</c> class takes a line of text and prints it to a GUI. 
/// </summary>
public class DialogueRenderer : ITextRenderer
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
    public DialogueRenderer(TextMeshProUGUI gui, ITextRenderStyle style)
    {
        _gui = gui;
        _style = style;
    }

    public void DisplayLine(string content)
    {
        _style.Render(content); 
    }
}
