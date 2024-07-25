using Ink.Runtime;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class Tutorial : TextAdvancer, ITextAdvancer
{

    private GameObject _continueIcon;
    private TypewriterTextRenderStyle _typewriter;

    public Tutorial(TextMeshProUGUI gui, TextAsset textAsset, MonoBehaviour coroutineMono,  GameObject continueIcon = null, GameObject tutorialObject = null, Animator animator = null)
    {
        _gui = gui;
        _textAsset = textAsset;
        _coroutineMono = coroutineMono;
        _renderer = new TextRenderer(_gui, new TypewriterTextRenderStyle(_gui, _coroutineMono, 0.04f));
        _typewriter = _renderer.Style as TypewriterTextRenderStyle;
        _typewriter.RenderStart += OnRenderStart;
        _typewriter.RenderComplete += OnRenderComplete;
        _continueIcon = continueIcon;
        _object = tutorialObject;
        _animator = animator;

        OnStoryCreate += TutorialSetup;
        OnStoryCreateAnimationFinished += BindTutorialFunctions;
        OnStoryEndAnimationFinished += TutorialComplete;
    }

    public override void Update()
    {
        if (!_isPlaying)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!_typewriter.CanAdvance())
            {
                _typewriter.Skip();
            }

            else
            {
                ContinueStory();
            }
        }
    }

    private void TutorialSetup(object sender, EventArgs e)
    {
        TutorialManager.SetTutorial(this);
        TutorialManager.TrySetActiveAll(false);
        TutorialManager.forcedMovementTile = null;
        TutorialManager.forcedSelectionTile = null;
        MoveCamera.AddLock("Tutorial");
        _continueIcon?.SetActive(false);
    }

    private void TutorialComplete(object sender, EventArgs e)
    {
        TutorialManager.TrySetActiveAll(true);
        TutorialManager.UnfocusAllUI();
        TutorialManager.SetTutorial(null);
        MoveCamera.RemoveLock("Tutorial");
        PhaseManager.instance.StartCoroutine(PhaseManager.instance.StartPlayerTurn());
    }

    protected override void OnRenderStart(object sender, EventArgs e)
    {
        _continueIcon?.SetActive(false);
    }

    protected override void OnRenderComplete(object sender, EventArgs e)
    {
        _continueIcon?.SetActive(true);
    }

    private void BindTutorialFunctions(object sender, EventArgs e)
    {
        _currentStory.BindExternalFunction("CameraFocusGuard", (int index) => { CameraFocusGuard(index); });
        _currentStory.BindExternalFunction("CameraFocusTile", (int x, int y) => { CameraFocusTile(x, y); });
        _currentStory.BindExternalFunction("ForcePlayer", (string playerName) => { ForcePlayer(playerName); });
        _currentStory.BindExternalFunction("CameraFocusPlayer", (string playerName) => { CameraFocusPlayer(playerName); });
        _currentStory.BindExternalFunction("ForceCard", (string cardName) => { ForceCard(cardName); });
        _currentStory.BindExternalFunction("EnableUI", (string elements) => { EnableUI(elements); });
        _currentStory.BindExternalFunction("DisableAllUI", (string exceptions) => { DisableAllUI(exceptions); });
        _currentStory.BindExternalFunction("FocusUI", (string elements) => { FocusUI(elements); });
        _currentStory.BindExternalFunction("UnfocusUI", (string elements) => { UnfocusUI(elements); });
        _currentStory.BindExternalFunction("FocusPlayer", (string name) => { FocusPlayer(name); });
        _currentStory.BindExternalFunction("ForceMovementTile", (int x, int y) => { ForceMovementTile(x, y); });
        _currentStory.BindExternalFunction("ForceSelectionTile", (int x, int y) => { ForceSelectionTile(x, y); });
        _currentStory.BindExternalFunction("ChainTutorial", (string fileName, string className, string eventName) => { ChainTutorial(fileName, className, eventName); });
    }


    #region INK FUNCTIONS
    //  INK FUNCTIONS

    //  To be called in events ONLY; don't put this in ink files.
    public void FunctionFinished()
    {
        _runningFunction = false;
        MoveCamera.OnFocusComplete -= FunctionFinished;
        ContinueStory();
    }

    public void ChainTutorial(string fileName, string className, string eventName)
    {
        TextAsset nextTutorial = Resources.Load<TextAsset>($"Dialogue/Tutorials/{fileName}");

        if (!nextTutorial)
        {
            Debug.LogError("ChainTutorial: Could not find asset at Dialogue/Tutorials/" +  fileName);
            return;
        }

        new TutorialTrigger(_gui, nextTutorial, eventName, className, new TutorialSceneData
        {
            CoroutineMono = _coroutineMono,
            ContinueObject = _continueIcon,
            TutorialObject = _object,
            Animator = _animator
        });
    }

    public void CameraFocusPlayer(string playerName)
    {
        _runningFunction = true;
        PlayerEntity player = LevelGenerator.instance.listOfPlayers.Find(x => x.name == playerName);

        if (player == null)
        {
            Debug.LogError("DialogueManager, CameraFocusPlayer: Couldn't find player of name " + playerName);
            return;
        }

        MoveCamera.Focus(player);
        MoveCamera.OnFocusComplete += FunctionFinished;
    }

    public void CameraFocusGuard(int index)
    {
        _runningFunction = true;
        GuardEntity guard = LevelGenerator.instance.listOfGuards[index];
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
        _runningFunction = true;
        TileData tile = LevelGenerator.instance.listOfTiles[x, y];
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
        LevelGenerator.instance.ForcePlayer(LevelGenerator.instance.listOfPlayers.Find(x => x.name == playerName));
    }

    public void ForceMovementTile(int x, int y)
    {
        Debug.Log("forced movement tile");
        TutorialManager.forcedMovementTile = new Vector2Int(x, y);
    }

    public void ForceSelectionTile(int x, int y)
    {
        TutorialManager.forcedSelectionTile = new Vector2Int(x, y);
    }
    public void ForceCard(string cardName)
    {
        List<Card> hand = PhaseManager.instance.lastSelectedPlayer.myHand;
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

        TutorialManager manager = MonoBehaviour.FindObjectOfType<TutorialManager>();

        string[] elementsArray = Regex.Split(elements, ", ");
        Debug.Log(string.Join("\n", elementsArray));
        manager.EnableUI(elementsArray);
    }

    public void DisableAllUI(string exceptions)
    {
        Debug.Log("disabled all ui except " + exceptions);
        TutorialManager manager = MonoBehaviour.FindObjectOfType<TutorialManager>();

        string[] exceptionsArray = Regex.Split(exceptions, ", ");
        if (manager)
        {
            manager.DisableAllUI(exceptionsArray);
        }
    }

    public void FocusUI(string elements)
    {
        TutorialManager manager = MonoBehaviour.FindObjectOfType<TutorialManager>();

        string[] elementsArray = Regex.Split(elements, ", ");
        if (manager)
        {
            manager.FocusUI(elementsArray);
        }
    }

    public void UnfocusUI(string elements)
    {
        TutorialManager manager = MonoBehaviour.FindObjectOfType<TutorialManager>();

        string[] elementsArray = Regex.Split(elements, ", ");
        if (manager)
        {
            manager.UnfocusUI(elementsArray);
        }
    }

    public void FocusPlayer(string name)
    {
        Debug.Log("focusing player");
        LevelUIManager.instance.UpdateStats(LevelGenerator.instance.listOfPlayers.Find(x => x.name == name));
    }
    #endregion
}

public struct TutorialSceneData
{
    public TextMeshProUGUI GUI;
    public MonoBehaviour CoroutineMono;
    public GameObject ContinueObject;
    public GameObject TutorialObject;
    public Animator Animator;
}