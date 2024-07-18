﻿using ES3Types;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueTriggerOnSceneStart : MonoBehaviour, ITextTrigger
{
    public TextMeshProUGUI TextMeshPro
    {
        get => _textGUI;
    }

    public TextAsset InkJSON
    {
        get => _textAsset;
    }

    public List<DialogueTagAnimation> TagAnimations
    {
        get => _tagAnimations;
    }

    public MonoBehaviour Mono
    {
        get => _mono;
    }

    [SerializeField] private TextAsset _textAsset;
    [SerializeField] private TextMeshProUGUI _textGUI;
    [SerializeField] private DialogueTagSpeaker _speaker = new DialogueTagSpeaker("speaker", null);
    [SerializeField] private List<DialogueTagAnimation> _tagAnimations;
    [SerializeField] private GameObject _dialogueObject;
    [SerializeField] private Animator _animator;
    private MonoBehaviour _mono;

    private Dialogue _dialogue;

    public void Start()
    {
        _mono = this;
        StartText();
    }

    public void StartText()
    {
        _dialogue = new Dialogue(_textGUI, _textAsset, _speaker, _tagAnimations, this, _dialogueObject, _animator);
        StartCoroutine(_dialogue.StartStory());
    }

    private void Update()
    {
        if (_dialogue != null)
        {
            _dialogue.Update();
        }
    }
}
