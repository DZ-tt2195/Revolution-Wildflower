using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTriggerAuto : MonoBehaviour
{
    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;

    private void Start()
    {
        if (!DialogueManager.GetInstance().dialogueIsPlaying)
        {
            DialogueManager.GetInstance().StartStory(inkJSON);
            DialogueManager.GetInstance().EnterDialogueMode();
        }
    }
}
