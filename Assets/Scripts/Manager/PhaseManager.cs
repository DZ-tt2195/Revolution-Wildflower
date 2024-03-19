using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PhaseManager : MonoBehaviour
{

#region Variables

    public static PhaseManager instance;

    [Foldout("Misc", true)]
    [Tooltip("Text that gets displayed when you game over")] [SerializeField] TMP_Text gameOverText;
    [Tooltip("Number of violent cards used")][ReadOnly] public int violentCards;
    [Tooltip("Amount of turns before a game over")] [ReadOnly] public int turnCount = 10;
    [Tooltip("Effects to do on future turns")][ReadOnly] public List<Card> futureEffects = new();

    public enum TurnSystem { WaitingOnPlayer, ResolvingAction, Environmentals, WaitingOnGuard };
    [Foldout("Phases in a turn", true)]
    [Tooltip("last selected player")][ReadOnly] public PlayerEntity lastSelectedPlayer;
    [Tooltip("if a player is being moved or not")][ReadOnly] public bool movingPlayer = false;
    [Tooltip("Mouse position")] Vector3 lastClickedMousePosition;
    TurnSystem _CurrentPhase;
    [Tooltip("What's happening in the game")][ReadOnly] public TurnSystem CurrentPhase
    {
        get { return _CurrentPhase; }
        set { _CurrentPhase = value; Debug.Log($"changed turn to {value}"); }
    }

    [Foldout("Turn buttons", true)]
    [Tooltip("Complete an objective you're next to")][ReadOnly] public Button objectiveButton;
    [Tooltip("Exit the level if you're on the right tile")][ReadOnly] Button exitButton;
    [Tooltip("Spend 3 energy to draw a card")] Button spendToDrawButton;
    [Tooltip("End the turn")] Button endTurnButton;
    [Tooltip("End turn button's image")] Image endTurnImage;

    [Foldout("Choices", true)]
    [Tooltip("A tile that the player chose as part of a decision")][ReadOnly] public TileData chosenTile;
    [Tooltip("A card that the player chose as part of a decision")][ReadOnly] public Card chosenCard;
    [Tooltip("Current Selected Tile")][ReadOnly] public TileData selectedTile;
    [Tooltip("Confirm your decisions")] public Collector confirmationCollector;

    [Foldout("Sound Effects", true)]
    [SerializeField] AK.Wwise.Event buttonSound;
    [SerializeField] AK.Wwise.Event endTurnSound;
    [SerializeField] AK.Wwise.Event footsteps;
    [SerializeField] AK.Wwise.Event characterSelectSound;
    [SerializeField] AK.Wwise.Event beginTurnSound;

    #endregion

#region Setup

    private void Awake()
    {
        instance = this;
        gameOverText.transform.parent.gameObject.SetActive(false);

        endTurnButton = GameObject.Find("End Turn Button").GetComponent<Button>();
        endTurnButton.onClick.AddListener(EndPlayerTurn);
        endTurnImage = endTurnButton.GetComponent<Image>();
        endTurnButton.gameObject.SetActive(false);

        spendToDrawButton = GameObject.Find("Spend Energy Button").GetComponent<Button>();
        spendToDrawButton.onClick.AddListener(SpendToDraw);
        spendToDrawButton.gameObject.SetActive(false);

        objectiveButton = GameObject.Find("Objective Button").GetComponent<Button>();
        objectiveButton.onClick.AddListener(DoObjective);
        objectiveButton.gameObject.SetActive(false);

        exitButton = GameObject.Find("Exit Button").GetComponent<Button>();
        exitButton.gameObject.SetActive(false);
        exitButton.onClick.AddListener(ExitCharacter);
    }

    /// <summary>
    /// game is finished
    /// </summary>
    /// <param name="cause">game over message</param>
    /// <param name="won">win or loss</param>
    public void GameOver(string cause, bool won)
    {
        gameOverText.text = cause;
        gameOverText.transform.parent.gameObject.SetActive(true);
        MoveCamera.AddLock("Game Over");

        TMP_Text endStats = GameObject.Find("End Stats").GetComponent<TMP_Text>();
        endStats.text = $"Violent Cards Used: {violentCards}";

        GameObject.Find("Debrief Button").SetActive(won);
        if (won)
        {
            SaveManager.instance.currentSaveData.currentLevel++;
            ES3.Save("saveData", SaveManager.instance.currentSaveData, $"{Application.persistentDataPath}/{SaveManager.instance.saveFileName}.es3");
        }
    }

    private void Update()
    {
        TutorialManager.TrySetActive(endTurnButton.gameObject.name, CurrentPhase == TurnSystem.WaitingOnPlayer);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            lastClickedMousePosition = Input.mousePosition;
        }
        else if (movingPlayer && Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (Input.mousePosition.Equals(lastClickedMousePosition) && CurrentPhase == TurnSystem.WaitingOnPlayer && !EventSystem.current.IsPointerOverGameObject())
            {
                StopCoroutine(ChooseMovePlayer(lastSelectedPlayer, 0));
                StopCoroutine(ChooseCardPlay(lastSelectedPlayer));
                Debug.LogError("deselect");

                lastSelectedPlayer = null;
                movingPlayer = false;
                BackToStart(false);
            }
        }
    }

    #endregion

#region Turn Control

    /// <summary>
    /// start the player turns
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartPlayerTurn()
    {
        LevelUIManager.instance.UpdateStats(null);

        foreach (Card card in futureEffects)
            yield return card.NextRoundEffect();
        futureEffects.Clear();

        selectedTile = null;
        beginTurnSound.Post(gameObject);
        BackToStart(true);
    }

    /// <summary>
    /// return to waiting on the player
    /// </summary>
    /// <param name="startTurn">if this is the first time this happened this turn</param>
    void BackToStart(bool startTurn)
    {
        if (LevelGenerator.instance.listOfPlayers.Count > 0)
        {
            CurrentPhase = TurnSystem.WaitingOnPlayer;
            StopCoroutine(ChooseMovePlayer(lastSelectedPlayer, 0));
            StopCoroutine(ChooseCardPlay(lastSelectedPlayer));

            LevelGenerator.instance.DisableAllTiles();
            LevelGenerator.instance.DisableAllCards();

            if (lastSelectedPlayer != null)
                LevelUIManager.instance.UpdateStats(lastSelectedPlayer);

            objectiveButton.gameObject.SetActive(false);
            endTurnImage.color = AnythingLeftThisTurn() ? Color.gray : Color.white;

            if (startTurn)
            {
                StartCoroutine(LevelUIManager.instance.FadeTurnBar("Player Turn"));
                try
                {
                    selectedTile = lastSelectedPlayer.currentTile;
                    FocusOnTile(lastSelectedPlayer.currentTile, true);
                }
                catch
                {
                    /*do nothing*/
                }
            }
            else if (lastSelectedPlayer != null)
            {
                selectedTile = lastSelectedPlayer.currentTile;
                ControlCharacter(lastSelectedPlayer);
            }
            LevelGenerator.instance.EnablePlayers();
        }
    }

    /// <summary>
    /// player turn has ended
    /// </summary>
    void EndPlayerTurn()
    {
        StopAllCoroutines();
        objectiveButton.gameObject.SetActive(false);
        spendToDrawButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);

        foreach (PlayerEntity player in LevelGenerator.instance.listOfPlayers)
        {
            LevelUIManager.instance.SetEnergy(player, player.maxEnergy);
            LevelUIManager.instance.SetMovement(player, player.movesPerTurn);
            player.damageTaken = 0;
            player.cardsPlayed.Clear();
        }

        LevelUIManager.instance.UpdateStats(null);
        StartCoroutine(EnvironmentalPhase());
    }

    /// <summary>
    /// resolve all environmentals
    /// </summary>
    /// <returns></returns>
    IEnumerator EnvironmentalPhase()
    {
        selectedTile = null;
        CurrentPhase = TurnSystem.Environmentals;
        LevelGenerator.instance.DisableAllTiles();
        LevelGenerator.instance.DisableAllCards();

        foreach (PlayerEntity player in LevelGenerator.instance.listOfPlayers)
        {
            if (player.stunned > 0)
                player.stunChange(-1);
        }

        foreach (EnvironmentalEntity environment in LevelGenerator.instance.listOfEnvironmentals)
        {
            if (environment != null)
            {
                FocusOnTile(environment.currentTile, false);
                yield return environment.EndOfTurn();
            }
        }
        StartCoroutine(GuardTurn());
    }

    /// <summary>
    /// guards take their turn
    /// </summary>
    /// <returns></returns>
    IEnumerator GuardTurn()
    {
        //erases current visible path
        foreach (TileData tile in Pathfinder.instance.FullPath)
            tile.directionIndicator.enabled = false;
        endTurnSound.Post(gameObject);
        foreach (PlayerEntity player in LevelGenerator.instance.listOfPlayers)
            yield return player.EndOfTurn();

        CurrentPhase = TurnSystem.WaitingOnGuard;
        yield return LevelUIManager.instance.FadeTurnBar("Company Turn");
        foreach (GuardEntity guard in LevelGenerator.instance.listOfGuards)
        {
            FocusOnTile(guard.currentTile, false);
            yield return guard.EndOfTurn();
            guard.movementLeft = guard.movesPerTurn;
            guard.DetectionRangePatrol = guard.DetectionRangeMax;
        }

        turnCount--;
        if (turnCount == 0)
        {
            GameOver("You ran out of time.", false);
        }
        else
        {
            StartCoroutine(StartPlayerTurn());
        }
    }

    #endregion

#region Controlling Player

    /// <summary>
    /// allow the player to move or play cards
    /// </summary>
    /// <param name="currentPlayer"></param>
    public void ControlCharacter(PlayerEntity currentPlayer)
    {
        if (CurrentPhase == TurnSystem.WaitingOnPlayer)
        {
            StopCoroutine(ChooseCardPlay(currentPlayer));
            StopCoroutine(ChooseMovePlayer(currentPlayer, 0));

            if (lastSelectedPlayer != currentPlayer)
            {
                characterSelectSound.Post(currentPlayer.gameObject);
            }

            lastSelectedPlayer = currentPlayer;
            selectedTile = currentPlayer.currentTile;
            AkSoundEngine.SetState("Character", currentPlayer.name);
            LevelUIManager.instance.UpdateStats(currentPlayer);

            StartCoroutine(ChooseMovePlayer(currentPlayer, currentPlayer.movementLeft));
            StartCoroutine(ChooseCardPlay(currentPlayer));

            if (TutorialManager.GetUIState("Spend Energy Button").canSpawn)
                spendToDrawButton.gameObject.SetActive(currentPlayer.myHand.Count < 5 && currentPlayer.myEnergy >= 3);
            else
                spendToDrawButton.gameObject.SetActive(false);

            exitButton.gameObject.SetActive(CanExit(currentPlayer.currentTile));

            var foundCanvasObjects = FindObjectsOfType<Collector>();
            foreach (Collector collector in foundCanvasObjects)
                Destroy(collector.gameObject);

            objectiveButton.gameObject.SetActive(currentPlayer.CheckForObjectives());
            if (currentPlayer.adjacentObjective != null)
                objectiveButton.GetComponentInChildren<TMP_Text>().text = currentPlayer.adjacentObjective.name;
        }
    }

    /// <summary>
    /// move this player around
    /// </summary>
    /// <param name="currentPlayer">the player to control</param>
    /// <param name="possibleMoves">how far they can move</param>
    /// <param name="freeMoves">if the movement will cost movement points</param>
    /// <returns></returns>
    public IEnumerator ChooseMovePlayer(PlayerEntity currentPlayer, int possibleMoves, bool freeMoves = false)
    {
        foreach (TileData tile in Pathfinder.instance.FullPath)
            tile.directionIndicator.enabled = false;
        yield return new WaitForSeconds(0.15f);

        List<TileData> possibleTiles = Pathfinder.instance.CalculateReachableGrids(currentPlayer.currentTile, possibleMoves, true);
        WaitForDecisionMove(possibleTiles);
        LevelGenerator.instance.EnablePlayers();

        if (!freeMoves)
        {
            CurrentPhase = TurnSystem.WaitingOnPlayer;
        }

        while (chosenTile == null)
        {
            movingPlayer = true;
            if (lastSelectedPlayer != currentPlayer)
            {
                lastSelectedPlayer = currentPlayer;
                LevelUIManager.instance.UpdateStats(lastSelectedPlayer);
            }

            if (selectedTile != currentPlayer.currentTile || (!freeMoves && CurrentPhase != TurnSystem.WaitingOnPlayer))
            {
                movingPlayer = false;
                yield break;
            }
            else
            {
                yield return null;
            }
        }

        movingPlayer = false;
        CurrentPhase = TurnSystem.ResolvingAction;
        Collector confirmDecision = ConfirmDecision("Confirm movement?", new Vector2(0, 200));
        if (confirmDecision != null)
        {
            selectedTile = chosenTile;
            yield return confirmDecision.WaitForChoice();
            int decision = confirmDecision.chosenButton;
            Destroy(confirmDecision.gameObject);

            if (decision == 1)
            {
                yield return (ChooseMovePlayer(currentPlayer, possibleMoves, freeMoves));
                yield break;
            }
        }

        LevelGenerator.instance.DisableAllTiles();
        LevelGenerator.instance.DisableAllCards();

        footsteps.Post(currentPlayer.gameObject);
        foreach (TileData nextTile in Pathfinder.instance.FullPath)
        {
            yield return new WaitForSeconds(PlayerPrefs.GetFloat("Animation Speed"));
            yield return currentPlayer.MoveTile(nextTile);
            if (!freeMoves)
            {
                LevelUIManager.instance.ChangeMovement(currentPlayer, -1);
            }
            if (currentPlayer.movementLeft == -1)
            {
                currentPlayer.movementLeft = 0;
                break;
            }
        }

        if (!freeMoves)
            BackToStart(false);
    }

    /// <summary>
    /// this player can play cards
    /// </summary>
    /// <param name="currentPlayer">the player to control</param>
    /// <returns></returns>
    IEnumerator ChooseCardPlay(PlayerEntity currentPlayer) //choose a card to play
    {
        CurrentPhase = TurnSystem.WaitingOnPlayer;
        List<Card> canBePlayed = new List<Card>();
        foreach (Card card in currentPlayer.myHand)
        {
            if (card.CanPlay(currentPlayer))
                canBePlayed.Add(card);
        }
        WaitForDecision(canBePlayed);

        while (chosenCard == null)
        {
            if (CurrentPhase != TurnSystem.ResolvingAction)
            {
                yield break;
            }
            else
            {
                yield return null;
            }
        }

        CurrentPhase = TurnSystem.ResolvingAction;
        Collector confirmDecision = ConfirmDecision($"Play {chosenCard.name}?", new Vector2(0, -85));
        if (confirmDecision != null)
        {
            yield return confirmDecision.WaitForChoice();
            int decision = confirmDecision.chosenButton;
            Destroy(confirmDecision.gameObject);

            if (decision == 1)
            {
                yield return ChooseCardPlay(currentPlayer);
                yield break;
            }
        }

        yield return currentPlayer.PlayCard(chosenCard, true);
        BackToStart(false);
    }


    #endregion

#region Other Actions

    /// <summary>
    /// move the camera to a tile
    /// </summary>
    /// <param name="tile">the tile to move to</param>
    /// <param name="moveMe">start controlling the player on that tile</param>
    public void FocusOnTile(TileData tile, bool moveMe)
    {
        if (tile != null)
        {
            MoveCamera.Focus(tile.transform.position);
            //Camera.main.transform.position = new Vector3(tile.transform.position.x, Camera.main.transform.position.y, tile.transform.position.z);
            if (moveMe && CurrentPhase == TurnSystem.WaitingOnPlayer)
                ControlCharacter(tile.myEntity.GetComponent<PlayerEntity>());
        }
    }

    /// <summary>
    /// check if anything's left this turn for the end turn button
    /// </summary>
    /// <returns></returns>
    bool AnythingLeftThisTurn()
    {
        foreach (PlayerEntity player in LevelGenerator.instance.listOfPlayers)
        {
            bool movementCheck = player.movementLeft > 0;
            bool handCheck = false;

            foreach (Card card in player.myHand)
            {
                if (card.CanPlay(player))
                    handCheck = true;
            }

            if (handCheck || movementCheck)
                return true;
        }
        return false;
    }

    /// <summary>
    /// if this tile can be used to exit the level
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    bool CanExit(TileData tile)
    {
        if (LevelGenerator.instance.listOfObjectives.Count > 0 || tile.myType == TileData.TileType.Regular)
            return false;

        if (tile.myType == TileData.TileType.Exit)
            return true;

        if (tile.myType == TileData.TileType.AllExit)
        {
            foreach (PlayerEntity player in LevelGenerator.instance.listOfPlayers)
            {
                if (player.currentTile.myType == TileData.TileType.Regular)
                    return false;
            }
            foreach (GuardEntity guard in LevelGenerator.instance.listOfGuards)
            {
                if (guard.currentTile.myType == TileData.TileType.Regular)
                    return false;
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// the current player leaves the level
    /// </summary>
    void ExitCharacter()
    {
        foreach (GuardEntity guard in LevelGenerator.instance.listOfGuards)
        {
            if (guard.CurrentTarget == lastSelectedPlayer)
                guard.resetAlert();
        }

        if (lastSelectedPlayer.currentTile.myType == TileData.TileType.AllExit)
        {
            GameOver("You won!", true);
        }
        else
        {
            LevelGenerator.instance.listOfPlayers.Remove(lastSelectedPlayer);
            Destroy(lastSelectedPlayer.myBar.gameObject);
            Destroy(lastSelectedPlayer.gameObject);

            if (LevelGenerator.instance.listOfPlayers.Count == 0)
            {
                GameOver("You won!", true);
            }
            else
            {
                lastSelectedPlayer = null;
                BackToStart(false);
            }
        }
    }

    /// <summary>
    /// resolve the objective the current player is next to
    /// </summary>
    void DoObjective()
    {
        if (CurrentPhase == TurnSystem.WaitingOnPlayer)
            StartCoroutine(ResolveObjective());
    }

    /// <summary>
    /// resolve the objective
    /// </summary>
    /// <returns></returns>
    IEnumerator ResolveObjective()
    {
        TutorialManager.TrySetActive(objectiveButton.gameObject.name, true);
        CurrentPhase = TurnSystem.ResolvingAction;

        if (lastSelectedPlayer != null && lastSelectedPlayer.adjacentObjective != null)
        {
            Collector confirmDecision = ConfirmDecision($"Complete this objective?", new Vector2(0, -85));
            if (confirmDecision != null)
            {
                yield return confirmDecision.WaitForChoice();
                int decision = confirmDecision.chosenButton;
                Destroy(confirmDecision.gameObject);

                if (decision == 1)
                {
                    BackToStart(false);
                    yield break;
                }
            }

            yield return lastSelectedPlayer.adjacentObjective.ObjectiveComplete(lastSelectedPlayer);
        }
        BackToStart(false);
    }

    /// <summary>
    /// spend 3 energy to draw a card
    /// </summary>
    void SpendToDraw()
    {
        if (CurrentPhase == TurnSystem.WaitingOnPlayer)
            StartCoroutine(ResolveDraw());
    }

    IEnumerator ResolveDraw()
    {
        CurrentPhase = TurnSystem.ResolvingAction;

        Collector confirmDecision = ConfirmDecision($"Spend 3 energy to draw a card?", new Vector2(0, -85));
        if (confirmDecision != null)
        {
            yield return confirmDecision.WaitForChoice();
            int decision = confirmDecision.chosenButton;
            Destroy(confirmDecision.gameObject);

            if (decision == 1)
            {
                BackToStart(false);
                yield break;
            }
        }

        LevelUIManager.instance.ChangeEnergy(lastSelectedPlayer, -3);
        lastSelectedPlayer.PlusCards(1);
        BackToStart(false);
    }

    #endregion

#region Decisions

    /// <summary>
    /// decision confirmation/undo
    /// </summary>
    /// <param name="header">the text in the box</param>
    /// <param name="position">the position of the box</param>
    /// <returns></returns>
    public Collector ConfirmDecision(string header, Vector3 position)
    {
        if (PlayerPrefs.GetInt("Confirm Choices") == 1)
        {
            LevelGenerator.instance.DisableAllCards();
            LevelGenerator.instance.DisableAllTiles();
            MoveCamera.AddLock("Confirmation");

            Collector collector = Instantiate(confirmationCollector);
            collector.StatsSetup(header, position);

            collector.AddTextButton("Confirm");
            collector.AddTextButton("Go Back");
            return collector;
        }
        else
        {
            return null;
        }
    }

    public void ReceiveChoice(Card card)
    {
        chosenCard = card;
    }

    public void ReceiveChoice(TileData tile)
    {
        chosenTile = tile;
    }

    /// <summary>
    /// disable all cards, except for everything in the list
    /// </summary>
    /// <param name="canBeChosen"></param>
    public void WaitForDecision(List<Card> canBeChosen)
    {
        chosenTile = null;
        chosenCard = null;
        LevelGenerator.instance.DisableAllCards();

        foreach (Card card in canBeChosen)
        {
            card.EnableCard();
        }
    }

    /// <summary>
    /// disable all tiles, except for everything in the list
    /// </summary>
    /// <param name="canBeChosen"></param>
    public void WaitForDecision(List<TileData> canBeChosen)
    {
        chosenTile = null;
        chosenCard = null;
        LevelGenerator.instance.DisableAllTiles();

        if (TutorialManager.forcedTiles.Count > 0)
        {
            foreach (Vector2Int tilePos in TutorialManager.forcedTiles)
            {
                foreach (TileData tile in canBeChosen)
                {
                    if (tile.gridPosition == tilePos)
                    {
                        tile.CardSelectable = true;
                        tile.clickable = true;
                        tile.choosable = true;
                        tile.indicatorArrow = true;
                    }

                    else
                    {
                        tile.CardSelectable = true;
                        tile.clickable = false;
                        tile.choosable = true;
                    }
                }
            }
        }

        else
        {
            foreach (TileData tile in canBeChosen)
            {
                tile.CardSelectable = true;
                tile.clickable = true;
                tile.choosable = true;

            }
        }
    }

    public void WaitForDecisionMove(List<TileData> canBeChosen)
    {
        chosenTile = null;
        chosenCard = null;
        LevelGenerator.instance.DisableAllTiles();

        if (TutorialManager.forcedTiles.Count > 0)
        {
            foreach (Vector2Int tilePos in TutorialManager.forcedTiles)
            {
                foreach (TileData tile in canBeChosen)
                {
                    if (tile.gridPosition == tilePos)
                    {
                        tile.moveable = true;
                        tile.clickable = true;
                        tile.choosable = true;
                        tile.indicatorArrow = true;
                    }

                    else
                    {
                        tile.moveable = true;
                        tile.clickable = false;
                        tile.choosable = false;
                    }
                }
            }
        }

        else
        {
            foreach (TileData tile in canBeChosen)
            {
                tile.moveable = true;
                tile.clickable = true;
                tile.choosable = true;
            }
        }
    }

#endregion

}
