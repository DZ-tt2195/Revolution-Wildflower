using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using System.Runtime.CompilerServices;
using System;
using System.Text.RegularExpressions;

public class DialogueManager : MonoBehaviour
{
    [Header("Params")]
    [SerializeField] private float typingSpeed = 0.02f;

    [Header("Load Globals JSON")]
    [SerializeField] private TextAsset loadGlobalsJSON;

    [Header("Dialouge UI")]

    [SerializeField] public GameObject dialoguePanel;
    [SerializeField] private GameObject continueIcon;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private Animator portraitAnimator;
    [SerializeField] private Animator layoutAnimator;
    [SerializeField] private Animator backgroundAnimator;

    [Header("Sounds")]
    [SerializeField] AK.Wwise.Event textCrawlSound;
    [SerializeField] AK.Wwise.Event textboxStopSound;

    public Story currentStory;

    public bool dialogueIsPlaying { get; private set; }
    private bool runningFunction = false;

    private int currentVisibleCharacterIndex;

    private bool canContinueToNextLine = false;

    private Coroutine displayLineCoroutine;

    private static DialogueManager instance;
    public static event Action DialogueCompleted;

    private const string SPEAKER_TAG = "speaker";
    private const string PORTRAIT_TAG = "portrait";
    private const string LAYOUT_TAG  = "layout";
    private const string BACKGROUND_TAG = "bg";


    [HideInInspector] public static DialogueVariables dialogueVariables;
   
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one Dialogue Manager");
        }
        instance = this;

        if (dialogueVariables == null)
        {
            dialogueVariables = new DialogueVariables(loadGlobalsJSON);
        }

        dialoguePanel.SetActive(false);
        dialogueIsPlaying = false;

    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {

        layoutAnimator.GetComponent<Animator>();
    }

    private void Update()
    {
        // return right away if dialogue isn't playing
        if (!dialogueIsPlaying)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (dialogueText.maxVisibleCharacters > 0 && dialogueText.maxVisibleCharacters < dialogueText.text.Length) 
            {
                dialogueText.maxVisibleCharacters = dialogueText.text.Length;
                continueIcon.SetActive(true);
                canContinueToNextLine = true;
            }

            else if (canContinueToNextLine && currentStory.currentChoices.Count == 0 && !runningFunction)
            {
                ContinueStory();
            }
        }

        // handle continuine to the next line in dialogue when submite is pressed
       /*if (canContinueToNextLine  
            && currentStory.currentChoices.Count == 0
            && !runningFunction
            && Input.GetKeyDown(KeyCode.Space))
       {
           ContinueStory();
       }*/
    }

    public void StartStory(TextAsset inkJSON)
    {
        Debug.Log(inkJSON.text);
        currentStory = new Story(inkJSON.text);
        Debug.Log(currentStory.currentText);
        dialogueVariables.StartListening(currentStory);

        currentStory.BindExternalFunction("CameraFocusPlayer",  (string playerName) =>      { CameraFocusPlayer(playerName); });
        currentStory.BindExternalFunction("CameraFocusGuard",   (int index) =>              { CameraFocusGuard(index); });
        currentStory.BindExternalFunction("CameraFocusTile",    (int x, int y) =>           { CameraFocusTile(x, y); });
        currentStory.BindExternalFunction("ForcePlayer",        (string playerName) =>      { ForcePlayer(playerName); });
        currentStory.BindExternalFunction("ForceCard",          (string cardName) =>        { ForceCard(cardName); });
        currentStory.BindExternalFunction("EnableUI",           (string elements) =>        { EnableUI(elements); });
        currentStory.BindExternalFunction("DisableAllUI",       (string exceptions) =>      { DisableAllUI(exceptions); });
        currentStory.BindExternalFunction("FocusUI",            (string elements) =>        { FocusUI(elements); });
        currentStory.BindExternalFunction("UnfocusUI",          (string elements) =>        { UnfocusUI(elements); });
        currentStory.BindExternalFunction("FocusPlayer",        (string name) =>            { FocusPlayer(name); });

        currentStory.BindExternalFunction("ForceTile",          (int x, int y) =>           { ForceTile(x, y); });


        //currentStory.BindExternalFunction("ForceCardDraw", (string playerName) => { CameraFocusPlayer(playerName); });
        //currentStory.BindExternalFunction("ToggleUIElement", (string playerName) => { CameraFocusPlayer(playerName); });
    }
    public void EnterDialogueMode()
    {
        dialogueVariables.VariablesToStory(currentStory);
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);
        TutorialManager.TrySetActiveAll(false);
        TutorialManager.forcedTiles.Clear();
        MoveCamera.AddLock("Dialogue");
        dialoguePanel.SetActive(true);
        Debug.Log("Set active true");

        // reset portrait, layout, background, and speaker
        //displayNameText.text = "???";
        //portraitAnimator.Play("Default");
        //layoutAnimator.Play("Left");
        //backgroundAnimator.Play("Default");

        ContinueStory();

    }

    private IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(0.2f);

        dialogueVariables.StopListening(currentStory);

        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        TutorialManager.TrySetActiveAll(true);
        TutorialManager.UnfocusAllUI();
        MoveCamera.RemoveLock("Dialogue");

        textboxStopSound.Post(gameObject);

        Debug.Log("Dialogue Completed");
        DialogueCompleted?.Invoke();
    }

    private IEnumerator DisplayLine(string line)
    {
        Debug.Log("displaying line " + line);
        // set the text to the full line, but set the visible characters to 0
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;

        // hide items while text is typing
        continueIcon.SetActive(false);

        canContinueToNextLine = false;

        bool isAddingRichTextTag = false;

        foreach (char letter in line.ToCharArray())
        {
            /*if (Input.GetKeyDown(KeyCode.Space) && dialogueText.maxVisibleCharacters > 0)
            {
                dialogueText.maxVisibleCharacters = line.Length;
                //dialogueText.text = line;
                break;
            }*/

            // check for rich text tag, if found, add it without warning
            if (letter == '<' || isAddingRichTextTag)
            {
                isAddingRichTextTag = true;
                if(letter == '>')
                {
                    isAddingRichTextTag = false;
                }
            }
            // if not rich text, add the next letter and wait a small time
            else
            {
                dialogueText.maxVisibleCharacters++;
                //dialogueText.text += letter;
                textCrawlSound.Post(gameObject);
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        // actions to take after the line has finished displaying
        continueIcon.SetActive(true);

        canContinueToNextLine = true;
    }

    private void ContinueStory()
    {
        Debug.Log(currentStory.currentText);
        if (currentStory.canContinue)
        {
            Debug.Log("Can continue");
            // set text for the current dialogue line
            if (displayLineCoroutine != null)
            {
                StopCoroutine(displayLineCoroutine);
            }
           
            displayLineCoroutine = StartCoroutine(DisplayLine(currentStory.Continue()));

            // handle tags
            HandleTags(currentStory.currentTags);
        }
        else
        {
            Debug.Log("Can't continue");
            StartCoroutine(ExitDialogueMode());
        }
    }

    private void HandleTags(List<string> currentTags)
    {
        // Loop through each tag and handle them accordingly
        foreach (string tag in currentTags)
        {
            // parse the tag
            string[] splitTag = tag.Split(':');
            if (splitTag.Length !=2)
            {
                Debug.LogError("Tag could not be approriately parsed: " + tag);
            }
            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();

            // handle the tag
            switch (tagKey)
            {
                case SPEAKER_TAG:
                    displayNameText.text = tagValue;
                    break;
                case PORTRAIT_TAG:
                    portraitAnimator.Play(tagValue);
                    break;
                case LAYOUT_TAG:
                    layoutAnimator.Play(tagValue);
                    break;
                case BACKGROUND_TAG:
                    backgroundAnimator.Play(tagValue);
                    break;
                default:
                    Debug.LogWarning("Tag came in but is not being handled: " + tag);
                    break;

            }
        }
    }

    public Ink.Runtime.Object GetVariableState(string variableName)
    {
        Ink.Runtime.Object variableValue = null;
        dialogueVariables.variables.TryGetValue(variableName, out variableValue);
        if (variableValue == null)
        {
            Debug.LogWarning("Ink Variable was found to be null: " + variableName);
        }
        return variableValue;
    }



    //  INK FUNCTIONS

    //  To be called in events ONLY; don't put this in ink files.
    public void FunctionFinished()
    {
        runningFunction = false;
        MoveCamera.OnFocusComplete -= FunctionFinished;
        ContinueStory();
    }

    public void CameraFocusPlayer(string playerName)
    {
        runningFunction = true;
        PlayerEntity player = NewManager.instance.listOfPlayers.Find(x => x.name == playerName);

        if (player == null)
        {
            Debug.LogError("DialogueManager, CameraFocusPlayer: Couldn't find player of name " +  playerName);
            return;
        }

        MoveCamera.Focus(player);
        MoveCamera.OnFocusComplete += FunctionFinished;
    }

    public void CameraFocusGuard(int index)
    {
        runningFunction = true;
        GuardEntity guard = NewManager.instance.listOfGuards[index];
        if (guard == null)
        {
            Debug.LogError("DialogueManager, CameraFocusGuard: Couldn't find guard with index " + index);
            return;
        }

        MoveCamera.Focus(guard);
        MoveCamera.OnFocusComplete += FunctionFinished;
    }

    public void CameraFocusTile(int x, int y)
    {
        runningFunction = true;
        TileData tile = NewManager.instance.listOfTiles[x, y];
        if (tile == null)
        {
            Debug.LogError("DialogueManager, CameraFocusTile: Couldn't find tile at position " + x + " " + y);
            return;
        }

        MoveCamera.Focus(tile.transform.position);
        MoveCamera.OnFocusComplete += FunctionFinished;
    }

    public void ForcePlayer(string playerName)
    {
        NewManager.instance.ForcePlayer(NewManager.instance.listOfPlayers.Find(x => x.name == playerName));
    }

    public void ForceTile(int x, int y)
    {
        TutorialManager.forcedTiles.Add(new Vector2Int(x, y));
    }

    public void ForceCard(string cardName)
    {
        List<Card> hand = NewManager.instance.lastSelectedPlayer.myHand;
        for (int i = 0; i < hand.Count; i++)
        {
            if (hand[i].textName.text == cardName)
            {
                hand[i].EnableCard();
            }

            else
            {
                hand[i].DisableCard();
            }
        }
    }

    //  This version takes in one string and splits it into an array for the TutorialManager version.
    //  Since ink is limited in the number of parameters it can take, we have to do it this way. 
    public void EnableUI(string elements)
    {
        Debug.Log("enabled ui");
        Debug.Log(elements);

        TutorialManager manager = FindObjectOfType<TutorialManager>();

        string[] elementsArray = Regex.Split(elements, ", ");
        Debug.Log(string.Join("\n", elementsArray));
        manager.EnableUI(elementsArray);
    }

    public void DisableAllUI(string exceptions)
    {
        Debug.Log("disabled all ui except " + exceptions);
        TutorialManager manager = FindObjectOfType<TutorialManager>();

        string[] exceptionsArray = Regex.Split(exceptions, ", ");
        if (manager)
        {
            manager.DisableAllUI(exceptionsArray);
        }
    }

    public void FocusUI(string elements)
    {
        TutorialManager manager = FindObjectOfType<TutorialManager>();

        string[] elementsArray = Regex.Split(elements, ", ");
        if (manager)
        {
            manager.FocusUI(elementsArray);
        }
    }

    public void UnfocusUI(string elements)
    {
        TutorialManager manager = FindObjectOfType<TutorialManager>();

        string[] elementsArray = Regex.Split(elements, ", ");
        if (manager)
        {
            manager.UnfocusUI(elementsArray);
        }
    }

    public void FocusPlayer(string name)
    {
        Debug.Log("focusing player");
        NewManager.instance.UpdateStats(NewManager.instance.listOfPlayers.Find(x => x.name == name));
    }
}
