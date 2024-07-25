using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MyBox;
using TMPro;
using System;

public class DialogueBriefingTrigger : MonoBehaviour, ITextTrigger
{
    public TextMeshProUGUI TextMeshPro
    {
        get => _textMeshPro;
    }
    
    public TextAsset InkJSON
    {
        get => _textData[SaveManager.instance.currentSaveData.currentLevel].InkJSON;
    }

    public List<ITextFunction> TextCompleteOrders
    {
        get => _textData[SaveManager.instance.currentSaveData.currentLevel].TextFunctions;
    }

    [SerializeField] private TextMeshProUGUI _textMeshPro;
    [SerializeField] private GameObject _object;
    [SerializeField] private GameObject _continueIcon; 
    [SerializeField] private Animator _animator;
    [SerializeField] DialogueBriefingData[] _textData;

    private Dialogue _dialogue;
    [Serializable]
    private class DialogueBriefingData
    {
        public TextAsset InkJSON;
        [SubclassSelector, SerializeReference] public List<ITextFunction> TextFunctions;
        public DialogueTagSpeaker Speaker = new DialogueTagSpeaker("speaker", null);
        public List<TextTagAnimation> TagAnimations;
    }

    private void Start()
    {
        StartText();
    }

    private void OnComplete(object sender, EventArgs e)
    {
        DialogueBriefingData data = _textData[SaveManager.instance.currentSaveData.currentLevel];
        foreach (ITextFunction func in data.TextFunctions) 
        {
            func.OnTextComplete(_dialogue);
        }
    }


    public void StartText()
    {
        DialogueBriefingData data = _textData[SaveManager.instance.currentSaveData.currentLevel];
        _dialogue = new Dialogue(_textMeshPro, data.InkJSON, data.Speaker, data.TagAnimations, this, _continueIcon, _object, _animator);
        StartCoroutine(_dialogue.StartStory());
        _dialogue.OnStoryEnd += OnComplete;
    }

    private void Update()
    {
        if (_dialogue != null)
        {
            _dialogue.Update();
        }
    }
}


