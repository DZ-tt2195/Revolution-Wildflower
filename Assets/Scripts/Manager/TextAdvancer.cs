using Ink.Runtime;
using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class TextAdvancer : ITextAdvancer
{
    protected TextAsset _textAsset;
    protected TextMeshProUGUI _gui;
    protected List<TextTagAnimation> _animations;
    protected GameObject _object;
    protected Animator _animator;
    protected ITextTrigger _trigger;
    protected ITextRenderer _renderer;
    protected ITextTagParser _parser;
    protected Story _currentStory;
    protected bool _isPlaying;
    protected bool _runningFunction; 
    protected MonoBehaviour _coroutineMono;

    public event EventHandler OnStoryCreate;
    public event EventHandler OnStoryCreateAnimationFinished;
    public event EventHandler OnContinue;
    public event EventHandler OnStoryEnd;
    public event EventHandler OnStoryEndAnimationFinished;

    public TextAsset InkJSON { get => _textAsset; }
    public TextMeshProUGUI GUI { get => _gui; }
    public ITextRenderer Renderer { get => _renderer; }
    public ITextTagParser Parser {  get => _parser; }
    public ITextTrigger Trigger { get => _trigger; }

    protected TextAdvancer()
    {
        
    }

    protected virtual void OnRenderStart(object sender, EventArgs e)
    {
        Debug.Log("Render start");
    }

    protected virtual void OnRenderComplete(object sender, EventArgs e)
    {
        Debug.Log("Render finished");
    }

    public virtual IEnumerator StartStory()
    {
        Debug.Log("Starting Story");
        OnStoryCreate?.Invoke(this, EventArgs.Empty);
        _currentStory = new Story(_textAsset.text);
        _gui.text = "";
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
        Debug.Log("Continuing story...");
        _isPlaying = true;
        ContinueStory();
    }

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