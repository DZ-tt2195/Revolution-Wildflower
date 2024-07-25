using System;
/// <summary>
/// <para>Classes that inherit from <c>ITextRenderStyle</c> are responsible for determining how text is rendered beyond the initial parameters set inside of a <c>TextMeshProUGUI</c>.</para>
/// A common example of an ITextRenderStyle would be a typewriter effect, but it also opens the opportunity for character/word/line shaders. 
/// </summary>
public interface ITextRenderStyle
{
    /// <summary>
    /// Takes a line of text and renders it.
    /// </summary>
    /// <param name="content">The content being rendered.</param>
    public void Render(string content);
    /// <summary>
    /// An event that triggers when the rendering process begins.
    /// </summary>
    public event EventHandler RenderStart;
    /// <summary>
    /// An event that triggers when the rendering process completes.
    /// </summary>
    public event EventHandler RenderComplete;
}
