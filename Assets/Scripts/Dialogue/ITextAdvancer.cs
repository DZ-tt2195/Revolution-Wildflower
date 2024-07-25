using TMPro;
using UnityEngine;

public interface ITextAdvancer
{
    public TextAsset InkJSON { get; }
    public TextMeshProUGUI GUI { get; }
    public ITextRenderer Renderer { get; }
    public ITextTrigger Trigger { get; }
}