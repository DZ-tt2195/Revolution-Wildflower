using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class TypewriterTextRenderStyle : ITextRenderStyle
{
    private TextMeshProUGUI _gui;
    public event EventHandler RenderStart;
    public event EventHandler RenderComplete;

    private float _typingSpeed = 0.02f;
    private bool _canContinueToNextLine;
    private ITextTrigger _trigger;
    private Coroutine _typingCoroutine;

    public TypewriterTextRenderStyle(TextMeshProUGUI gui, ITextTrigger trigger, float typingSpeed = 0.02f)
    {
        _gui = gui;
        _trigger = trigger;
        _typingSpeed = typingSpeed;
    }

    public void Render(string content)
    {
        _typingCoroutine = _trigger.Mono.StartCoroutine(DisplayLine(content));
    }

    public void Skip()
    {
        if (_typingCoroutine != null)
        {
            _trigger.Mono.StopCoroutine(_typingCoroutine);
            _gui.maxVisibleCharacters = _gui.text.Length;
            _canContinueToNextLine = true;
            RenderComplete?.Invoke(this, EventArgs.Empty); 
        }
    }

    public bool CanAdvance()
    {
        return _canContinueToNextLine;
    }

    public IEnumerator DisplayLine(string line)
    {
        RenderStart?.Invoke(this, EventArgs.Empty);

        _gui.text = line;
        _gui.maxVisibleCharacters = 0;

        _canContinueToNextLine = false;
        foreach (char letter in line.ToCharArray())
        {
            _gui.maxVisibleCharacters++;
            yield return new WaitForSeconds(_typingSpeed);
        }

        Debug.Log("Line complete");
        _canContinueToNextLine = true;
        RenderComplete?.Invoke(this, EventArgs.Empty);
    }
}
