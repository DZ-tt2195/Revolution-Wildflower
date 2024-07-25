using TMPro;

public class TutorialRenderer : ITextRenderer
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
    public TutorialRenderer(TextMeshProUGUI gui, ITextRenderStyle style)
    {
        _gui = gui;
        _style = style;
    }

    public void DisplayLine(string content)
    {
        _style.Render(content);
    }
}
