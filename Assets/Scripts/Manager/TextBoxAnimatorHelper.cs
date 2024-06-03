using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextBoxAnimatorHelper : MonoBehaviour
{
    [SerializeField]
    private DialogueManager dialogueManager;

    /// <summary>
    /// To be called when the briefing dialogue box finishes fading into view.
    /// </summary>
    public void OnIntroAnimationComplete()
    {
        Debug.Log("Enter Dialogue Mode");
        dialogueManager.OnAnimationFinished();
    }

    /// <summary>
    /// To be called when the tutorial dialogue box finishes its text and should fade out.
    /// </summary>
    public void OnTutorialAnimationComplete()
    {
        TutorialManager.OnAnimationFinished();
    }
}
