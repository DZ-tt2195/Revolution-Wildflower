using Ink.Runtime;
using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class Dialogue : TextAdvancer, ITextAdvancer
{
    private GameObject _continueIcon;
    private TextMeshProUGUI _speakerGUI;
    private TypewriterTextRenderStyle _typewriter;
    private float _typingSpeed;

    public Dialogue(TextMeshProUGUI gui, TextAsset textAsset, DialogueTagSpeaker speaker, List<TextTagAnimation> animations, ITextTrigger trigger, GameObject continueIcon = null, GameObject dialogueObject = null, Animator animator = null)
    {
        Debug.Log("Dialogue created");
        _gui = gui;
        _textAsset = textAsset;

        _trigger = trigger;

        _coroutineMono = trigger as MonoBehaviour;
        _renderer = new DialogueRenderer(_gui, new TypewriterTextRenderStyle(_gui, _coroutineMono));
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
