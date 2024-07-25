using ES3Types;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class TutorialManager : MonoBehaviour
{
    private LevelStartParameters parameters; 
    private static TutorialManager instance;

    [SerializeField] private TextMeshProUGUI _gui;
    [SerializeField] private GameObject _object;
    [SerializeField] private GameObject _continueIcon;
    [SerializeField] private Animator _animator;
    [SerializeField] private Tutorial _tutorial;

    [SerializeField] private string levelStartParametersFilePath;
    [SerializeField] private GameObject[] levelStartUI;



    private static List<UIState> levelUIStates = new();
    public static List<UIState> focusedUIElements = new();
    public static Vector2Int? forcedMovementTile;
    public static Vector2Int? forcedSelectionTile;
    public static string forcedCard;

    //public static List<Vector2Int> forcedTiles = new();

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

    private void Update()
    {
        if (_tutorial != null)
        {
            _tutorial.Update();
        }
    }

    public static void SetTutorial(Tutorial tutorial = null)
    {
        instance._tutorial = tutorial;
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
            if (!ui.gameObject)
            {
                return;
            }

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
        LevelStartParameters parameters = Resources.Load<LevelStartParameters>($"{instance.levelStartParametersFilePath}/{levelName}");
        if (parameters == null)
        {
            PhaseManager.instance.StartCoroutine(PhaseManager.instance.StartPlayerTurn());
            Debug.LogWarning($"TutorialManager, SetLevelStartParameters: Could not find LevelStartParameters at {instance.levelStartParametersFilePath}/{levelName}");
            return;
        }

        ForceCharacterHand(parameters);
        ForceCharacterDeck(parameters);

        if (parameters.textAsset != null)
        {
            instance._tutorial = new Tutorial(instance._gui, parameters.textAsset, instance as MonoBehaviour, instance._continueIcon, instance._object, instance._animator);
            instance.StartCoroutine(instance._tutorial.StartStory());
        }

        /*if (parameters.dialogueOnStart)
        {
            MoveCamera.AddLock("Tutorial");
            //DialogueManager.GetInstance().StartStory(parameters.dialogueAsset);
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
        }*/

        /*else
        {
            DialogueManager.GetInstance().dialoguePanel.SetActive(false);
            LevelUIManager.instance.UpdateStats(null);
            PhaseManager.instance.endTurnButton.gameObject.SetActive(true);
            PhaseManager.instance.StartCoroutine(PhaseManager.instance.StartPlayerTurn());
        }*/
    }

    public void ExitTutorial()
    {
        DialogueManager.DialogueCompleted -= ExitTutorial;
    }

    public static void OnAnimationFinished()
    {
        Debug.Log("bleh");
        DialogueManager.GetInstance().dialoguePanel.SetActive(false);
        MoveCamera.RemoveLock("Tutorial");

        foreach (GameObject ui in instance.levelStartUI)
        {
            if (ui.TryGetComponent(out Button button))
            {
                button.enabled = true;
            }
        }

        LevelUIManager.instance.UpdateStats(null);
        PhaseManager.instance.StartCoroutine(PhaseManager.instance.StartPlayerTurn());
    }

    public static void ForceCharacterHand(LevelStartParameters parameters)
    {
        if (parameters.forcedHands is null)
        {
            return;
        }
        foreach (ForceCharacterHand hand in parameters.forcedHands)
        {
            PlayerEntity player = LevelGenerator.instance.listOfPlayers.Find(x => x.name == hand.CharacterName);
            if (player != null)
            {
                player.ForceHand(hand.CardNames);
            }
        }
    }

    public static void ForceCharacterDeck(LevelStartParameters parameters)
    {
        if (parameters.forcedDecks is null)
        {
            return; 
        }
         
        foreach (ForceCharacterDeck deck in  parameters.forcedDecks)
        {
            PlayerEntity player = LevelGenerator.instance.listOfPlayers.Find(x => x.name == deck.CharacterName);
            if (player != null)
            {
                player.ForceTopDeck(deck.CardNames);
            }
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
public class ForceCharacterDeck
{
    public string CharacterName;
    public bool ForceDeck;
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

public class TutorialTrigger : ITextTrigger
{
    private TextMeshProUGUI _gui;
    private TextAsset _textAsset;
    private List<ITextFunction> _textFunctions;
    private Tutorial _tutorial;
    private GameObject _continueObject;
    private MonoBehaviour _coroutineMono;
    private GameObject _tutorialObject;
    private Animator _animator;

    private object obj;
    private EventInfo eventInfo;
    private Delegate handler;

    public TextMeshProUGUI TextMeshPro { get => _gui; }
    public TextAsset InkJSON { get => _textAsset; }
    public List<ITextFunction> TextCompleteOrders { get => _textFunctions; }

    public TutorialTrigger(TextMeshProUGUI gui, TextAsset textAsset, string eventName, string eventClass, MonoBehaviour coroutineMono, GameObject continueObject = null, GameObject tutorialObject = null, Animator animator = null)
    {
        Type T = Type.GetType(eventClass);
        MethodInfo method = GetType().GetMethod("Load", BindingFlags.Public | BindingFlags.Instance);
        EventInfo eventInfo = T.GetEvent(eventName, BindingFlags.Public | BindingFlags.Static);
        Type eventHandlerType = eventInfo.EventHandlerType;
        Delegate handler = Delegate.CreateDelegate(eventHandlerType, this, method);
        eventInfo.AddEventHandler(obj, handler);

        //this.obj = obj;
        this.handler = handler;
        this.eventInfo = eventInfo;

        _gui = gui;
        _textAsset = textAsset;
        _coroutineMono = coroutineMono;
        _continueObject = continueObject;
        _tutorialObject = tutorialObject;
        _animator = animator;
    }

    public void StartText(object sender, EventArgs e)
    {
        StartText();
    }

    public void StartText()
    {
        _tutorial = new Tutorial(_gui, _textAsset, _coroutineMono, _continueObject, _tutorialObject, _animator);
    }
}