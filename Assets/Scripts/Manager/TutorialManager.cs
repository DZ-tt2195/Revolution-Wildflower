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
    [SerializeField] private GameObject[] levelStartUI;
    private static List<UIState> levelUIStates = new();
    public static List<UIState> focusedUIElements = new();
    public static Vector2Int forcedMovementTile;
    public static Vector2Int forcedCardSelectionTile;
    public static string forcedCard;

    public static List<Vector2Int> forcedTiles = new();

    private void Awake()
    {
        instance = this;
        levelUIStates.Clear();
        foreach (GameObject ui in levelStartUI)
        {
            levelUIStates.Add(new UIState(ui));
        }
    }

    private void Start()
    {

    }

    public static UIState GetUIState(string name)
    {
        return levelUIStates.Find(x => x.gameObject.name == name);
    }

    public static void TrySetActive(string element, bool active)
    {

        if (instance == null)
        {
            Debug.Log("TrySetActive: No Tutorial Manager instance found");
            return;
        }

        foreach (UIState ui in levelUIStates)
        {
            if (ui.gameObject.name == element)
            {
                // If the UI can spawn, you can freely spawn/despawn it. 
                if (ui.canSpawn)
                {
                    ui.gameObject.SetActive(active);
                }

                else
                {
                    //  If the UI cannot spawn, and we want to turn it off, we can. 
                    if (ui.gameObject.activeInHierarchy && active == false)
                    {
                        ui.gameObject.SetActive(active);
                    }

                    //  Otherwise, intentionally do nothing. 
                    else
                    {
                        continue;
                    }
                }
            }
        }
    }

    public static void TrySetActiveAll(bool active)
    {
        if (instance == null)
        {
            Debug.Log("TrySetActiveAll: No Tutorial Manager instance found");
            return;
        }

        foreach(UIState ui in levelUIStates)
        {
            TrySetActive(ui.gameObject.name, active);
        }
    }

    public void EnableUI(string[] elements)
    {
        foreach(string element in elements)
        {
            UIState ui = levelUIStates.Find(x => x.gameObject.name == element);
            if (ui == null)
            {
                Debug.LogError("TutorialManager, EnableUI: Couldn't find element of name " + element);
                continue;
            }    
            ui.canSpawn = true;
            TrySetActive(element, true);
        }
    }

    public void EnableAllUI(string[] exceptions = null)
    {
        foreach (UIState ui in levelUIStates)
        {
            if (exceptions == null)
            {
                ui.canSpawn = true;
                TrySetActive(ui.gameObject.name, true);
            }

            else
            {
                foreach (string exception in exceptions)
                {
                    if (exception == ui.gameObject.name)
                    {
                        continue;
                    }

                    else
                    {
                        ui.canSpawn = true;
                        TrySetActive(ui.gameObject.name, true);
                    }
                }
            }
        }
    }

    public void DisableUI(string[] elements)
    {
        foreach (string element in elements)
        {
            UIState ui = levelUIStates.Find(x => x.gameObject.name == element);
            if (ui == null)
            {
                Debug.LogError("TutorialManager, EnableUI: Couldn't find element of name " + element);
                continue;
            }
            ui.canSpawn = false;
            TrySetActive(element, false);
        }
    }

    public void DisableAllUI(string[] exceptions = null)
    {
        foreach (UIState ui in levelUIStates)
        {
            if (exceptions == null)
            {
                ui.canSpawn = false;
                TrySetActive(ui.gameObject.name, false);
            }

            else
            {
                foreach (string exception in exceptions)
                {
                    if (exception == ui.gameObject.name)
                    {
                        continue;
                    }

                    else
                    {
                        ui.canSpawn = false;
                        TrySetActive(ui.gameObject.name, false);
                    }
                }
            }
        }
    }

    public void FocusUI(string[] elements)
    {
        foreach (string element in elements)
        {
            UIState ui = levelUIStates.Find(x => x.gameObject.name == element);
            if (ui == null)
            {
                Debug.LogError("TutorialManager, FocusUI: Couldn't find element of name " + element);
                continue;
            }

            focusedUIElements.Add(ui);
            ui.gameObject.transform.SetParent(GameObject.Find("Dialogue Panel").transform);
            ui.gameObject.transform.SetAsLastSibling();

            if (ui.gameObject.TryGetComponent(out Button button))
            {
                button.enabled = false;
            }
        }
    }

    public void UnfocusUI(string[] elements)
    {
        foreach (string element in elements)
        {
            UIState ui = focusedUIElements.Find(x => x.gameObject.name == element);
            if (ui == null)
            {
                Debug.LogError("TutorialManager, UnfocusUI: Couldn't find element of name " + element);
                continue;
            }

            focusedUIElements.Remove(ui);
            ui.gameObject.transform.SetParent(ui.parent);
            ui.gameObject.transform.SetSiblingIndex(ui.siblingIndex);

            if (ui.gameObject.TryGetComponent(out Button button))
            {
                button.enabled = true;
            }
        }
    }

    public static void UnfocusAllUI()
    {
        if (instance == null)
        {
            Debug.Log("TrySetActive: No Tutorial Manager instance found");
            return;
        }

        foreach (UIState ui in focusedUIElements)
        {
            ui.gameObject.transform.SetParent(ui.parent);
            ui.gameObject.transform.SetSiblingIndex(ui.siblingIndex);

            if (ui.gameObject.TryGetComponent(out Button button))
            {
                button.enabled = true;
            }
        }

        focusedUIElements.Clear();
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
            DialogueManager.GetInstance().dialoguePanel.SetActive(false);
            NewManager.instance.UpdateStats(null);
            NewManager.instance.StartCoroutine(NewManager.instance.StartPlayerTurn());
        }
    }

    public static void ChainTutorial(string className, string eventName, string fileName)
    {

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

[System.Serializable]
public class UIState
{
    public GameObject gameObject;
    public Transform parent;
    public bool canSpawn;
    public int siblingIndex;

    public UIState(GameObject gameObject)
    {
        this.gameObject = gameObject;
        this.canSpawn = true;
        this.parent = gameObject.transform.parent; 
        this.siblingIndex = gameObject.transform.GetSiblingIndex();
    }
}