﻿using TMPro;

public interface ITextRenderer
{
    public TextMeshProUGUI GUI { get; }
    public ITextRenderStyle Style { get; }
    public void DisplayLine(string content);
}