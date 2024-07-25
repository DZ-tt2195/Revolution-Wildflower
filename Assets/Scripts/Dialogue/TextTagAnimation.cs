using UnityEngine;
using System;

[System.Serializable]
public class TextTagAnimation : ITextTag
{
    public string Tag 
    {
        get => _tag;
    }

    [SerializeField] private string _tag;
    [SerializeField] private string _startingValue;
    [SerializeField] private Animator _animator; 

    public TextTagAnimation(string tag, Animator animator)
    {
        _tag = tag;
        _animator = animator;
    }

    public void Setup()
    {
        if (_startingValue != null)
        {
            _animator.Play(_startingValue);
        }
    }

    public void OnTagMentioned(object content)
    {
        string animatorTag = Convert.ToString(content);
        _animator.Play(animatorTag);
    }
}
