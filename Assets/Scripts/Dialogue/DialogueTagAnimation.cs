using UnityEngine;
using System;

[System.Serializable]
public class DialogueTagAnimation : IDialogueTag
{
    public string Tag { 
        get
        {
            return _tag;
        }
        set 
        {

        } 
    }

    [SerializeField] private string _tag;
    [SerializeField] private Animator _animator; 

    public DialogueTagAnimation(string tag, Animator animator)
    {
        _tag = tag;
        _animator = animator;
    }

    public void OnTagMentioned(object content)
    {
        string animatorTag = Convert.ToString(content);
        _animator.Play(animatorTag);
    }
}
