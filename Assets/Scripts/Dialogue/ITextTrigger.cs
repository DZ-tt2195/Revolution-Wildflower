using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface ITextTrigger
{
    public TextMeshProUGUI TextMeshPro { get; }
    public TextAsset InkJSON { get; }
    public List<ITextFunction> TextCompleteOrders { get; }

    public void StartText();
}
