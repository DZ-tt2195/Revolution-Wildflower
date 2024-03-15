using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    private LevelStartParameters parameters; 
    private static TutorialManager instance;
    [SerializeField] private string levelStartParametersFilePath;
    public GameObject[] levelStartUI;

    private void Awake()
    {
        instance = this;
    }

    public void DisableAllUI(string[] exceptions = null)
    {
        foreach(GameObject ui in levelStartUI)
        {
            ui.SetActive(false);
        }

        EnableUI(exceptions);
    }

    public void EnableAllUI(string[] exceptions = null)
    {
        foreach (GameObject ui in levelStartUI)
        {
            ui.SetActive(true);
            if (ui.TryGetComponent(out Button button))
            {
                button.enabled = true;
            }
        }

        DisableUI(exceptions);
    }

    public void DisableUI(string[] elements = null)
    {
        if (elements == null) { return;  }
        for (var i = 0; i < elements.Length; i++)
        {
            for (var j = 0; j < levelStartUI.Length; j++)
            {
                if (elements[i] == levelStartUI[j].name)
                {
                    levelStartUI[j].SetActive(false);
                }
            }
        }
    }

    public void EnableUI(string[] elements = null)
    {
        if (elements == null) { return; }
        for (var i = 0; i < elements.Length; i++)
        {
            bool found = false;
            for (var j = 0; j < levelStartUI.Length; j++)
            {
                if (elements[i] == levelStartUI[j].name)
                {
                    levelStartUI[j].SetActive(true);
                    Debug.Log(levelStartUI[j].name);
                    found = true;
                    if (levelStartUI[j].TryGetComponent(out Button button))
                    {
                        button.enabled = false;
                    }
                }
            }
            if (!found)
            {
                Debug.LogError("TutorialManager, EnableUI: Could not find " + elements[i]);
            }
        }
    }

    public static void SetLevelStartParameters(string levelName)
    {
        LevelStartParameters parameters = Resources.Load<LevelStartParameters>(instance.levelStartParametersFilePath + "/" + levelName);
        if (parameters == null)
        {
            Debug.LogError("TutorialManager, SetLevelStartParameters: Could not find LevelStartParameters at " + instance.levelStartParametersFilePath + "/" + levelName);
            return;
        }

        instance.ForceCharacterHand(parameters);

        if (parameters.dialogueOnStart)
        {
            instance.DisableAllUI();
            MoveCamera.AddLock("Tutorial");
            DialogueManager.GetInstance().StartStory(parameters.dialogueAsset);
            foreach (LevelStartDialogueVariable dialogueVariable in parameters.dialogueVariables)
            {
                //  Though we store the values as strings in the ScriptableObject, we can convert them to ints as necessary. 
                float numValue;
                if (float.TryParse(dialogueVariable.value, out numValue))
                {
                    DialogueManager.dialogueVariables.globalVariablesStory.variablesState[dialogueVariable.name] = numValue;
                }

                else
                {
                    DialogueManager.dialogueVariables.globalVariablesStory.variablesState[dialogueVariable.name] = dialogueVariable.value;
                }
            }
            DialogueManager.GetInstance().EnterDialogueMode();
            Debug.Log("Entering dialogue mode");
            DialogueManager.DialogueCompleted += instance.ExitTutorial;
        }

        else
        {
            NewManager.instance.UpdateStats(null);
            NewManager.instance.StartCoroutine(NewManager.instance.StartPlayerTurn());
        }
    }

    public void ExitTutorial()
    {
        DialogueManager.DialogueCompleted -= ExitTutorial;

        MoveCamera.RemoveLock("Tutorial");

        foreach(GameObject ui in levelStartUI)
        {
            if (ui.TryGetComponent(out Button button))
            {
                button.enabled = true;
            }
        }


        NewManager.instance.UpdateStats(null);
        NewManager.instance.StartCoroutine(NewManager.instance.StartPlayerTurn());

    }

    public void ForceCharacterHand(LevelStartParameters parameters)
    {
        foreach (ForceCharacterHand hand in parameters.forcedHands)
        {
            PlayerEntity player = NewManager.instance.listOfPlayers.Find(x => x.name == hand.CharacterName);
            if (player != null)
                player.ForceHand(hand.CardNames);
        }
    }
}

[System.Serializable]
public class ForceCharacterHand
{
    public string CharacterName;
    public bool ForceHand;
    public string[] CardNames; 
}