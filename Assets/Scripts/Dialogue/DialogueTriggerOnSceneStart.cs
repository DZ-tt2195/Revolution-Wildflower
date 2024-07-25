using ES3Types;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public interface ITextFunction
{
    public void OnTextComplete(ITextAdvancer advancer);
}

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

    public List<ITextFunction> TextCompleteOrders
    {
        get => _onComplete;
    }

    [Header("Text")]
    [SerializeField] private TextAsset _textAsset;
    [SerializeField] private TextMeshProUGUI _textGUI;

    [Header("Tags")]
    [SerializeField] private DialogueTagSpeaker _speaker = new DialogueTagSpeaker("speaker", null);
    [SerializeField] private List<TextTagAnimation> _tagAnimations;

    [Header("Objects")]
    [SerializeField] private GameObject _dialogueObject;
    [SerializeField] private GameObject _continueObject;
    [SerializeField] private Animator _animator;

    [SerializeReference, SubclassSelector] private List<ITextFunction> _onComplete;
    private Dialogue _dialogue;

    public void Start()
    {
        StartText();
    }

    public void StartText()
    {
        _dialogue = new Dialogue(_textGUI, _textAsset, _speaker, _tagAnimations, this, _continueObject, _dialogueObject, _animator);
        StartCoroutine(_dialogue.StartStory());
        _dialogue.OnStoryEndAnimationFinished += OnComplete;
    }

    private void OnComplete(object sender, EventArgs e)
    {
        foreach (ITextFunction func in _onComplete)
        {
            func.OnTextComplete(_dialogue);
        }
    }

    private void Update()
    {
        if (_dialogue != null)
        {
            _dialogue.Update();
        }
    }
}
