using Ink.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

[Obsolete]
public class DialogueManager : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float typingSpeed = 0.02f;
    [SerializeField] AK.Wwise.Event textCrawlSound;
    [SerializeField] AK.Wwise.Event textboxStopSound;

    [Header("Load Globals JSON")]
    [SerializeField] private TextAsset loadGlobalsJSON;

    [Header("Dialogue UI")]
    [SerializeField] public GameObject dialoguePanel;
    [SerializeField] private GameObject continueIcon;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private Animator textBoxAnimator;
    [SerializeField] private Animator portraitAnimator;
    [SerializeField] private Animator layoutAnimator;
    [SerializeField] private Animator backgroundAnimator;


    public Story currentStory;

    public bool dialogueIsPlaying { get; private set; }
    private bool runningFunction = false;
    private bool canContinueToNextLine = false;

    private Coroutine displayLineCoroutine;

    [Obsolete]
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

        //dialoguePanel.SetActive(false);
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
    }
    
    public void StartStory(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        dialogueVariables.StartListening(currentStory);

        //currentStory.BindExternalFunction("CameraFocusPlayer",  (string playerName) =>      { CameraFocusPlayer(playerName); });
        //currentStory.BindExternalFunction("CameraFocusGuard",   (int index) =>              { CameraFocusGuard(index); });
        //currentStory.BindExternalFunction("CameraFocusTile",    (int x, int y) =>           { CameraFocusTile(x, y); });
        //currentStory.BindExternalFunction("ForcePlayer",        (string playerName) =>      { ForcePlayer(playerName); });
        //currentStory.BindExternalFunction("ForceCard",          (string cardName) =>        { ForceCard(cardName); });
        //currentStory.BindExternalFunction("EnableUI",           (string elements) =>        { EnableUI(elements); });
        //currentStory.BindExternalFunction("DisableAllUI",       (string exceptions) =>      { DisableAllUI(exceptions); });
        //currentStory.BindExternalFunction("FocusUI",            (string elements) =>        { FocusUI(elements); });
        //currentStory.BindExternalFunction("UnfocusUI",          (string elements) =>        { UnfocusUI(elements); });
        //currentStory.BindExternalFunction("FocusPlayer",        (string name) =>            { FocusPlayer(name); });
        //currentStory.BindExternalFunction("ForceMovementTile",  (int x, int y) =>           { ForceMovementTile(x, y); });
        //currentStory.BindExternalFunction("ForceSelectionTile", (int x, int y) =>           { ForceSelectionTile(x, y); });
        //currentStory.BindExternalFunction("ChainTutorial",      (string fileName, string className, string eventName) => { ChainTutorial(fileName, className, eventName); });
    }

    public void EnterDialogueMode()
    {
        Debug.Log("Entered dialogue mode");
        dialoguePanel.SetActive(true);
        dialogueVariables.VariablesToStory(currentStory);
        dialogueIsPlaying = true;
        //dialoguePanel.SetActive(true);

        TutorialManager.TrySetActiveAll(false);
        TutorialManager.forcedMovementTile = null;
        TutorialManager.forcedSelectionTile = null; 

        MoveCamera.AddLock("Dialogue");

        if (textBoxAnimator)
        {
            textBoxAnimator.SetTrigger("In");
        }

        else
        {
            OnAnimationFinished();
        }
        //ContinueStory();
    }

    public void OnAnimationFinished()
    {
        ContinueStory();
    }

    public void AnimatorSetup(string backgroundAnimationName, string portraitAnimationName, string layoutAnimationName)
    {
        backgroundAnimator.Play(backgroundAnimationName);
        portraitAnimator.Play(portraitAnimationName);
        layoutAnimator.Play(layoutAnimationName);

        displayNameText.text = "";
        dialogueText.text = "";
        continueIcon.SetActive(false);
    }

    private IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(0.2f);

        dialogueVariables.StopListening(currentStory);

        dialogueIsPlaying = false;
        //dialoguePanel.SetActive(false);
        dialogueText.text = "";
        TutorialManager.TrySetActiveAll(true);
        TutorialManager.UnfocusAllUI();
        MoveCamera.RemoveLock("Dialogue");
        textBoxAnimator.SetTrigger("Out");

        textboxStopSound.Post(gameObject);

        Debug.Log("Dialogue Completed");
        DialogueCompleted?.Invoke();
    }

    private IEnumerator DisplayLine(string line)
    {
        //Debug.Log("displaying line " + line);
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
            //if (letter == '<' || isAddingRichTextTag)
           // {
            //    isAddingRichTextTag = true;
             //   if(letter == '>')
              //  {
               //     isAddingRichTextTag = false;
                //}
            //}
            // if not rich text, add the next letter and wait a small time
            //else
            //{
                dialogueText.maxVisibleCharacters++;
                //dialogueText.text += letter;
                textCrawlSound.Post(gameObject);
                yield return new WaitForSeconds(typingSpeed);
            //}
        }

        // actions to take after the line has finished displaying
        continueIcon.SetActive(true);

        canContinueToNextLine = true;
    }

    private void ContinueStory()
    {
        //Debug.Log(currentStory.currentText);
        if (currentStory.canContinue)
        {
            //Debug.Log("Can continue");
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
            //Debug.Log("Can't continue");
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



 
}
