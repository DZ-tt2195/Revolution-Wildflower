using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface ITextTrigger
{
    public TextMeshProUGUI TextMeshPro { get; }
    public TextAsset InkJSON { get; }
    public List<DialogueTagAnimation> TagAnimations { get; }
    public MonoBehaviour Mono { get; }
    public void StartText();
}
