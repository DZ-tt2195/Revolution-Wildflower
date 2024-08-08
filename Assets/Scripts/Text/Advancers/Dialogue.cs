using Ink.Runtime;
using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// The <c>Dialogue</c> class handles typical dialogue output. 
/// </summary>
[Serializable]
public class Dialogue : TextAdvancer, ITextAdvancer
{
    private GameObject _continueIcon;
    private TextMeshProUGUI _speakerGUI;
    private TypewriterTextRenderStyle _typewriter;
    private float _typingSpeed;

    /// <summary>
    /// <c>Dialogue</c> class constructor.
    /// </summary>
    /// <param name="gui">The <c>TextMeshProUGUI</c> this class will be printing text to.</param>
    /// <param name="textAsset">The asset to pull text from.</param>
    /// <param name="speaker">An instance of the <c>DialogueSpeakerClass</c> to print the speaker's name to.</param>
    /// <param name="animations">A list of <c>TextTagAnimation</c>s to check for tags within the text asset and play the correct animation.</param>
    /// <param name="trigger">The <c>ITextTrigger</c> that triggered this <c>Dialogue</c>.</param>
    /// <param name="continueIcon">The <c>GameObject</c> that should appear when it is safe to progress text and disappear otherwise.</param>
    /// <param name="dialogueObject">The <c>GameObject</c> that contains the entire dialogue panel. Enabled/disabled on start/finish respectively.</param>
    /// <param name="animator">The <c>Animator</c> this class will wait to finish before executing post-animation functions.</param>
    public Dialogue(TextMeshProUGUI gui, TextAsset textAsset, TextTagSpeaker speaker, List<TextTagAnimation> animations, ITextTrigger trigger, GameObject continueIcon = null, GameObject dialogueObject = null, Animator animator = null)
    {
        _gui = gui;
        _textAsset = textAsset;

        _trigger = trigger;

        _coroutineMono = trigger as MonoBehaviour;
        _renderer = new TextRenderer(_gui, new TypewriterTextRenderStyle(_gui, _coroutineMono));
        _typewriter = _renderer.Style as TypewriterTextRenderStyle;
        _typewriter.RenderStart += OnRenderStart;
        _typewriter.RenderComplete += OnRenderComplete;

        _animations = animations;
        _speakerGUI = speaker.GUI;
        _parser = new DialogueTagParser(speaker, _animations);

        _continueIcon = continueIcon;
        _object = dialogueObject;
        _animator = animator;

        OnStoryCreate += SetUpAnimators;
    }

    protected override void OnRenderStart(object sender, EventArgs e)
    {
        _continueIcon?.SetActive(false);
    }

    protected override void OnRenderComplete(object sender, EventArgs e)
    {
        _continueIcon?.SetActive(true);
    }

    /// <summary>
    /// In addition to checking for input, this function should also skip the typewriter.
    /// </summary>
    public override void Update()
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

    private void SetUpAnimators(object sender, EventArgs e)
    {
        _speakerGUI.text = "";
        foreach(TextTagAnimation animation in _animations)
        {
            animation.Setup();
        }
    }
}
