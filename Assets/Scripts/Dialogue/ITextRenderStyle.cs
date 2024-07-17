using System;

public interface ITextRenderStyle
{
    public void Render(string content);
    public event EventHandler RenderStart;
    public event EventHandler RenderComplete;
}
