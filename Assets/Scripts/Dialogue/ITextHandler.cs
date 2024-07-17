using TMPro;
using UnityEngine;

public interface ITextHandler
{
    public TextAsset inkJSON { get; }
    public TextMeshProUGUI GUI { get; }
    public ITextRenderer Renderer { get; }
    public ITextTagParser Parser { get; }
    public ITextTrigger Trigger { get; }
}