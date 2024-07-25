using System.Collections;
using UnityEngine;
using TMPro;
using System;

/// <summary>
/// This <c>ITextRenderStyle</c> reveals its content in short intervals per letter. 
/// </summary>
public class TypewriterTextRenderStyle : ITextRenderStyle
{
    private MonoBehaviour _coroutineMono; 
    private TextMeshProUGUI _gui;
    public event EventHandler RenderStart;
    public event EventHandler RenderComplete;

    private float _typingSpeed = 0.04f;
    private bool _canContinueToNextLine;
    private Coroutine _typingCoroutine;

    /// <summary>
    /// Constructor for <c>TypewriterTextRenderStyle</c>.
    /// </summary>
    /// <param name="gui">The <c>TextMeshProUGUI</c> to print text to.</param>
    /// <param name="mono">The <c>MonoBehaviour</c> responsible for starting/stopping coroutines on this class.</param>
    /// <param name="typingSpeed">How quickly you want text to be revealed in seconds.</param>
    public TypewriterTextRenderStyle(TextMeshProUGUI gui, MonoBehaviour coroutineMono, float typingSpeed = 0.02f)
    {
        _gui = gui;
        _gui.text = "";
        _coroutineMono = coroutineMono;
        _typingSpeed = typingSpeed;
    }

    public void Render(string content)
    {
        _typingCoroutine = _coroutineMono.StartCoroutine(DisplayLine(content));
    }

    /// <summary>
    /// Instantly renders the entirety of a line. 
    /// </summary>
    public void Skip()
    {
        if (_typingCoroutine != null)
        {
            _coroutineMono.StopCoroutine(_typingCoroutine);
            _gui.maxVisibleCharacters = _gui.text.Length;
            _canContinueToNextLine = true;
            RenderComplete?.Invoke(this, EventArgs.Empty); 
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Whether or not the typewriter can move onto the next line.</returns>
    public bool CanAdvance()
    {
        return _canContinueToNextLine;
    }
    
    /// <summary>
    /// Displays a line in intervals by letter.
    /// </summary>
    /// <param name="line">The content to be displayed.</param>
    /// <returns></returns>
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

        _canContinueToNextLine = true;
        RenderComplete?.Invoke(this, EventArgs.Empty);
    }
}
