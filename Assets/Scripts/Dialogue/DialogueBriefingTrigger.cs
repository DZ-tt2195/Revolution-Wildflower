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

    [Scene]
    [SerializeField] string scene;

    private void Start()
    {
        //SaveManage = GameObject.Find("SaveManager").GetComponent<SaveManager>();
        Debug.Log("hi");

        if (!DialogueManager.GetInstance().dialogueIsPlaying)
        {
            DialogueManager.GetInstance().StartStory(inkJSON[SaveManager.instance.currentSaveData.currentLevel]);
            DialogueManager.GetInstance().EnterDialogueMode();
            
        }
    }

    private void Update()
    {
    
        if (!DialogueManager.GetInstance().dialogueIsPlaying)
            {
                NextScene();
            }
    }

    public void NextScene()
    {
        MoveCamera.ClearLocks();
        StartCoroutine(SaveManager.instance.UnloadObjects(scene));
    }

}
