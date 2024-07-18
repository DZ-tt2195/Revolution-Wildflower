using Ink.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Dialogue : ITextHandler
{
    private TextAsset _inkJSON;
    private List<DialogueTagAnimation> _animations;
    private GameObject _dialogueObject;
    private GameObject _continueIcon;
    private Animator _animator;
    private ITextTrigger _trigger;


    private DialogueRenderer _renderer;
    private DialogueTagParser _parser;
    private TypewriterTextRenderStyle _typewriter;

    private Story _currentStory;

    private TextMeshProUGUI _speakerGUI; 
    private TextMeshProUGUI _gui;

    private float _typingSpeed;
    private bool _isPlaying;

    public TextAsset inkJSON => _inkJSON;
    public TextMeshProUGUI GUI => _gui;
    public ITextRenderer Renderer => _renderer;
    public ITextTagParser Parser => _parser;
    public ITextTrigger Trigger => _trigger;

    /// <summary>
    /// Constructs an instance of the <c>Dialogue</c> class. 
    /// </summary>
    /// <param name="gui">The <c>TextMeshProUGUI</c> where the text will be printed.</param>
    /// <param name="inkJSON">The Ink file whose contents are being displayed.</param>
    /// <param name="trigger">The <c>ITextTrigger</c> that called this constructor.</param>
    /// <param name="animations"></param>
    /// <param name="dialogueObject"></param>
    /// <param name="animator"></param>
    public Dialogue(TextMeshProUGUI gui, TextAsset inkJSON, DialogueTagSpeaker speaker, List<DialogueTagAnimation> animations, ITextTrigger trigger, GameObject dialogueObject = null, Animator animator = null)
    {
        Debug.Log("Dialogue created");
        _gui = gui;
        _inkJSON = inkJSON;

        _renderer = new DialogueRenderer(_gui, new TypewriterTextRenderStyle(_gui, trigger, _typingSpeed));
        _typewriter = _renderer.Style as TypewriterTextRenderStyle;
        _typewriter.RenderStart += OnRenderStart;
        _typewriter.RenderComplete += OnRenderComplete;

        _animations = animations;
        _parser = new DialogueTagParser(speaker, _animations);

        _dialogueObject = dialogueObject;
        _animator = animator;
    }

    private void OnRenderStart(object sender, EventArgs e)
    {
        _continueIcon?.SetActive(false);
    }

    public void OnRenderComplete(object sender, EventArgs e)
    {
        _continueIcon?.SetActive(true);
    }

    public void Update()
    {
        if (!_isPlaying)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!_typewriter.CanAdvance())
            {
                _typewriter.Skip();
            }

            else
            {
                ContinueStory();
            }
        }
    }

    public IEnumerator StartStory()
    {
        Debug.Log("Starting story");
        _currentStory = new Story(_inkJSON.text);
        _gui.text = "";

        //  Bind wanted functions.

        _dialogueObject.SetActive(true);
        _animator?.Play("In");
        if (_animator)
        {
            while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1)
            {
                yield return null;
            }
        }
        Debug.Log("Continuing story...");
        _isPlaying = true;
        ContinueStory();
    }

    private void ContinueStory()
    {
        if (_currentStory.canContinue)
        {
            _renderer.DisplayLine(_currentStory.Continue());
            _parser.ParseTags(_currentStory.currentTags);
        }

        else
        {
            EndStory();
        }
    }

    private void EndStory()
    {
        _isPlaying = false;
        _renderer.DisplayLine("");

        /*yield return new WaitForSeconds(0.2f);

        dialogueVariables.StopListening(currentStory);

        dialogueIsPlaying = false;
        //dialoguePanel.SetActive(false);
        dialogueText.text = "";
        TutorialManager.TrySetActiveAll(true);
        TutorialManager.UnfocusAllUI();
        MoveCamera.RemoveLock("Dialogue");
        textBoxAnimator.SetTrigger("Out");

        textboxStopSound.Post(gameObject);

        Debug.Log("Dialogue Completed");
        DialogueCompleted?.Invoke();*/
    }
}
