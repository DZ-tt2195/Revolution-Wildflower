using Ink.Runtime;
using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// <para>This class holds a default implementation of ITextAdvancer that should fit the needs of most advancers.</para>
/// </summary>
[Serializable]
public class TextAdvancer : ITextAdvancer
{
    protected TextMeshProUGUI _gui;
    protected TextAsset _textAsset;
    protected GameObject _object;
    protected Animator _animator;
    protected List<TextTagAnimation> _animations;
    protected Story _currentStory;
    protected bool _isPlaying;
    protected bool _runningFunction; 
    //  Keep in mind that since TextAdvancer isn't a MonoBehaviour, it needs a reference to one in order to execute coroutines like StartStory.
    //  If your text is triggered by something in the scene, you can cast the trigger as a MonoBehaviour to fulfill this! 
    protected MonoBehaviour _coroutineMono; 
    protected ITextTrigger _trigger;
    protected ITextRenderer _renderer;
    protected ITextTagParser _parser;

    /// <summary>
    /// An event that triggers immediately before the current Ink story is created.
    /// </summary>
    public event EventHandler OnStoryCreate;
    /// <summary>
    /// An event that triggers after the text box has finished animating in.
    /// </summary>
    public event EventHandler OnStoryCreateAnimationFinished;
    /// <summary>
    /// An event that triggers each time the text advances.
    /// </summary>
    public event EventHandler OnContinue;
    /// <summary>
    /// An event that triggers when the story can no longer continue and has reached its end.
    /// </summary>
    public event EventHandler OnStoryEnd;
    /// <summary>
    /// An event that triggers after the text box has finished animating out. 
    /// </summary>
    public event EventHandler OnStoryEndAnimationFinished;

    public TextAsset InkJSON { get => _textAsset; }
    public TextMeshProUGUI GUI { get => _gui; }
    public ITextRenderer Renderer { get => _renderer; }
    public ITextTrigger Trigger { get => _trigger; }

    protected TextAdvancer()
    {
        
    }

    protected virtual void OnRenderStart(object sender, EventArgs e)
    {

    }

    protected virtual void OnRenderComplete(object sender, EventArgs e)
    {
        
    }

    /// <summary>
    /// Creates an Ink <c>Story</c> from the given <c>TextAsset</c> and kicks it off.
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator StartStory()
    {
        OnStoryCreate?.Invoke(this, EventArgs.Empty);
        _currentStory = new Story(_textAsset.text);
        _object?.SetActive(true);
        _animator.Play("In");
        if (_animator)
        {
            while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1)
            {
                yield return null;
            }
        }
        OnStoryCreateAnimationFinished?.Invoke(this, EventArgs.Empty);
        _isPlaying = true;
        ContinueStory();
    }
    /// <summary>
    /// <c>Update</c> function. Should usually be run in whatever triggered this <c>TextAdvancer.</c>
    /// </summary>
    public virtual void Update()
    {
        if (!_isPlaying)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ContinueStory();
        }
    }

    protected virtual void ContinueStory()
    {
        if (_currentStory.canContinue)
        {
            OnContinue?.Invoke(this, EventArgs.Empty);
            _renderer.DisplayLine(_currentStory.Continue());
            _parser?.ParseTags(_currentStory.currentTags);
        }

        else
        {
            _coroutineMono.StartCoroutine(EndStory());
        }
    }

    protected virtual IEnumerator EndStory()
    {
        _isPlaying = false;
        OnStoryEnd?.Invoke(this, EventArgs.Empty);
        _renderer.DisplayLine("");
        _animator.SetTrigger("Out");
        if (_animator)
        {
            while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1)
            {
                yield return null;
            }
        }
        _object.SetActive(false);
        OnStoryEndAnimationFinished?.Invoke(this, EventArgs.Empty);
    }
}