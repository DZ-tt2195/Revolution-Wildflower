
/// <summary>
/// Classes that inherit from <c>ITextFunction</c> are responsible for functions run at the end of certain <c>TextAdvancer</c>s. Ideal for universal functions like moving scenes.
/// </summary>
public interface ITextFunction
{
    /// <summary>
    /// What to do when the advancer is complete.
    /// </summary>
    /// <param name="advancer">The <c>ITextAdvancer</c> that has just finished running.</param>
    public void OnTextComplete(ITextAdvancer advancer);
}
