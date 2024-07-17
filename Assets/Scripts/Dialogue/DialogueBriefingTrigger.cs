using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MyBox;

public class DialogueBriefingTrigger : MonoBehaviour
{
    [Header("Ink JSON")]
    [SerializeField] private TextAsset[] inkJSON;
    [SerializeField] private DialogueBriefingData[] briefingData;

    [Scene]
    [SerializeField] string scene;

    private void Start()
    {
        //SaveManage = GameObject.Find("SaveManager").GetComponent<SaveManager>();
        Debug.Log("hi");

        if (!DialogueManager.GetInstance().dialogueIsPlaying)
        {
            DialogueBriefingData data = briefingData[SaveManager.instance.currentSaveData.currentLevel];
            DialogueManager.GetInstance().AnimatorSetup(data.backgroundAnimation, data.portraitAnimation, data.layoutAnimation);
            DialogueManager.GetInstance().StartStory(inkJSON[SaveManager.instance.currentSaveData.currentLevel]);

            SceneTransitionManager.OnTransitionInCompleted += EnterDialogueMode;
            DialogueManager.DialogueCompleted += LoadScene.NextScene;
        }
    }

    public void EnterDialogueMode()
    {
        DialogueManager.GetInstance().EnterDialogueMode();
        SceneTransitionManager.OnTransitionInCompleted -= EnterDialogueMode;
    }

    /*private void Update()
    {
    
        if (!DialogueManager.GetInstance().dialogueIsPlaying)
            {
                NextScene();
            }
    }*/

    [System.Serializable]
    public class DialogueBriefingData
    {
        public TextAsset inkJson;
        public string backgroundAnimation;
        public string layoutAnimation;
        public string portraitAnimation;
    }
}


