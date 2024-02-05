using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using UnityEngine.SceneManagement;
//using UnityEditor.U2D;
//using Unity.VisualScripting;

public class AStarNode
{
    public TileData ATileData;
    public AStarNode Parent;

    public int GCost; //travel distance from node to start node
    public int HCost; //travel distance from node to target node
    public int FCost => GCost + HCost; //Astar value of this tile, the lower it is, the better for the pathfinder.
}

public class NewManager : MonoBehaviour
{

#region Variables

    public static NewManager instance;

    [Foldout("Storing Things", true)]
        [Tooltip("Reference to players")][ReadOnly] public List<PlayerEntity> listOfPlayers = new List<PlayerEntity>();
        [Tooltip("Reference to walls")][ReadOnly] public List<WallEntity> listOfWalls = new List<WallEntity>();
        [Tooltip("Reference to guards")][ReadOnly] public List<GuardEntity> listOfGuards = new List<GuardEntity>();
        [Tooltip("Reference to active environmental objects")][ReadOnly] public List<EnvironmentalEntity> listOfEnvironmentals = new List<EnvironmentalEntity>();
        [Tooltip("Reference to objectives")] [ReadOnly] public List<ObjectiveEntity> listOfObjectives = new List<ObjectiveEntity>();

    [Foldout("Player decisions", true)]
        [Tooltip("A tile that the player chose")][ReadOnly] public TileData chosenTile;
        [Tooltip("A acard that the player chose")][ReadOnly] public Card chosenCard;
        [Tooltip("Current Selected Tile")][ReadOnly] public TileData selectedTile;
        [Tooltip("Quick reference to current movable tile")][ReadOnly] public TileData CurrentAvailableMoveTarget;
        [Tooltip("The last path traced. This only is filled if (singleMovement) is disabled")][ReadOnly] public List<TileData> FullPath = new();
        [Tooltip("Confirm your decisions")] public Collector confirmationCollector;

    [Foldout("UI Elements", true)]
        [Tooltip("Hazardbox to give to players")][SerializeField] public CanvasGroup ManagerHazardBox;
        [Tooltip("Speed of turn fade")][SerializeField] float turnFadeSpeed = 0.05f;
        [Tooltip("How much time the turn banner appears")][SerializeField] float turnHangDuration = 0.5f;
        [Tooltip("pop up alerting player of turn")][SerializeField] private CanvasGroup turnAlertBar;
        [Tooltip("text on turn banner")][SerializeField] private TMP_Text turnText;
        [Tooltip("Spritesheet of player character faces")][SerializeField] private Sprite [] facesSpritesheet;
        [Tooltip("Blank character face")][SerializeField] private Sprite emptyFace;
        [Tooltip("Your hand in the canvas")] [ReadOnly] public Transform handContainer;
        [Tooltip("The bar in the bottom center of the screen")] Transform playerStats;
        [Tooltip("Image section containing objective, actions, and debug popups")] Transform informationImage;
        [Tooltip("All the player's stats in text form")] TMP_Text stats;
        [Tooltip("Current player selected")] TMP_Text currentCharacter;
        [Tooltip("Selected player's health")] TMP_Text health;
        [Tooltip("Selected player's moves left")] TMP_Text moves;
        [Tooltip("Selected player's energy")] TMP_Text energy;
        [Tooltip("Face of selected character")] Image characterFace;
        [Tooltip("Instructions for what the player is allowed to do right now")] TMP_Text instructions;
        [Tooltip("End the turn")] Button endTurnButton;
        [Tooltip("End turn button's image")] Image endTurnImage;
        [Tooltip("Complete an objective you're next to")] [ReadOnly] public Button objectiveButton;
        [Tooltip("Info on entities")] [ReadOnly] public EntityToolTip toolTip;
        [Tooltip("Text that gets displayed when you game over")] TMP_Text gameOverText;
        [Tooltip("Tracks number of cards in deck and discard pile")] TMP_Text deckTracker;

    [Foldout("Grid", true)]
        [Tooltip("Tiles in the inspector")] Transform gridContainer;
        [Tooltip("Storage of tiles")][ReadOnly] public TileData[,] listOfTiles;
        [Tooltip("Spacing between tiles")][SerializeField] float tileSpacing;

    [Foldout("Prefabs", true)]
        [Tooltip("Floor tile prefab")] [SerializeField] TileData floorTilePrefab;
        [Tooltip("Player prefab")][SerializeField] PlayerEntity playerPrefab;
        [Tooltip("Wall prefab")][SerializeField] WallEntity wallPrefab;
        [Tooltip("Wall T prefab")][SerializeField] WallEntity wallTSplitPrefab;
        [Tooltip("Wall Corner prefab")][SerializeField] WallEntity wallCornerPrefab;
        [Tooltip("Wall Plus prefab")][SerializeField] WallEntity wallPlusPrefab;
        [Tooltip("Guard prefab")][SerializeField] GuardEntity guardPrefab;
        [Tooltip("Objective prefab")][SerializeField] ObjectiveEntity objectivePrefab;
        [Tooltip("Toggle prefab")][SerializeField] ToggleEntity togglePrefab;
        [Tooltip("Guard prefab")][SerializeField] ExitEntity exitPrefab;
        [Tooltip("Environmental prefab")][SerializeField] EnvironmentalEntity environmentPrefab;

    [Foldout("Setup", true)]
        [Tooltip("Amount of turns before a game over")] public int turnCount;
        [Tooltip("The level number (starts at 0")] [SerializeField] int levelToLoad;

    [Foldout("Flashing", true)]
        [Tooltip("the transparancy of card/tile borders")][ReadOnly] public float opacity = 1;
        [Tooltip("whether the borders are turning white or black")][ReadOnly] public bool decrease = true;

    public enum TurnSystem { You, Resolving, Environmentals, Enemy };
    [Foldout("Turn System", true)]
        [Tooltip("last selected player")] [ReadOnly] public PlayerEntity lastSelectedPlayer;
        [Tooltip("What's happening in the game")][ReadOnly] public TurnSystem currentTurn;
        [Tooltip("Effects to do on future turns")][ReadOnly] public List<Card> futureEffects = new List<Card>();
        [Tooltip("Num violent cards used")][ReadOnly] public int violentCards;

    [Foldout("Sound Effects", true)]
        [SerializeField] AK.Wwise.Event buttonSound;
        [SerializeField] AK.Wwise.Event endTurnSound;
        [SerializeField] AK.Wwise.Event footsteps;
        [SerializeField] AK.Wwise.Event characterSelectSound;
        [SerializeField] AK.Wwise.Event beginTurnSound;

    #endregion

#region Setup

    void Awake()
    {
        instance = this;

        turnAlertBar.alpha = 0;
        playerStats = GameObject.Find("Player Stats").transform;
        currentCharacter = playerStats.GetChild(0).GetComponent<TMP_Text>();
        health = playerStats.GetChild(1).GetComponent<TMP_Text>();
        moves = playerStats.GetChild(2).GetComponent<TMP_Text>();
        energy = playerStats.GetChild(3).GetComponent<TMP_Text>();
        characterFace = playerStats.GetChild(4).GetComponentInChildren<Image>();

        facesSpritesheet = Resources.LoadAll<Sprite>("Sprites/portrait_spritesheet");
        emptyFace = Resources.Load<Sprite>("Sprites/characterSill");

        informationImage = GameObject.Find("Information Image").transform;
        stats = informationImage.GetChild(0).GetComponent<TMP_Text>();
        instructions = informationImage.GetChild(1).GetComponent<TMP_Text>();
        deckTracker = GameObject.Find("Deck Tracker").GetComponent<TMP_Text>();

        endTurnButton = GameObject.Find("End Turn Button").GetComponent<Button>();
        endTurnButton.onClick.AddListener(Regain);
        endTurnImage = endTurnButton.GetComponent<Image>();
        objectiveButton = GameObject.Find("Objective Button").GetComponent<Button>();
        objectiveButton.onClick.AddListener(DoObjective);

        handContainer = GameObject.Find("Hand Container").transform;
        gridContainer = GameObject.Find("Grid Container").transform;
    }

    void Start()
    {
        if (turnCount <= 0)
            throw new Exception("Didn't set turn count in NewManager (has to be > 0");

        gameOverText = GameObject.Find("Game Over").transform.GetChild(0).GetComponent<TMP_Text>();
        gameOverText.transform.parent.gameObject.SetActive(false);

        GetTiles();
        StartCoroutine(StartPlayerTurn());
    }

    void GetTiles()
    {
        handContainer.transform.localPosition = new Vector3(10000, 10000, 0);
        string[,] newGrid = LevelLoader.LoadLevelGrid(SaveManager.instance.levelSheets[levelToLoad]);
        listOfTiles = new TileData[newGrid.GetLength(0), newGrid.GetLength(1)];
        Transform playerBars = GameObject.Find("Player Bars").transform;

        for (int i = 0; i < listOfTiles.GetLength(0); i++) //generate all tiles
        {
            for (int j = 0; j < listOfTiles.GetLength(1); j++)
            {
                try{newGrid[i, j] = newGrid[i, j].Trim().Replace("\"", "").Replace("]","");}
                catch (NullReferenceException){continue;}

                if (newGrid[i,j] != "")
                {
                    TileData nextTile = Instantiate(floorTilePrefab);
                    nextTile.transform.SetParent(gridContainer);
                    nextTile.transform.position = new Vector3(i * -tileSpacing, 0, j * -tileSpacing);
                    nextTile.name = $"Tile {i},{j}";
                    listOfTiles[i, j] = nextTile;
                    nextTile.gridPosition = new Vector2Int(i, j);

                    Entity thisTileEntity = null;
                    string[] numberPlusAddition = newGrid[i, j].Split("/");

                    switch (int.Parse(numberPlusAddition[0]))
                    {
                        case 1: //create player
                            //print("player");
                            thisTileEntity = Instantiate(playerPrefab, nextTile.transform);
                            PlayerEntity player = thisTileEntity.GetComponent<PlayerEntity>();
                            player.myPosition = listOfPlayers.Count;
                            player.myBar = playerBars.GetChild(listOfPlayers.Count).GetComponent<PlayerBar>();
                            SetEnergy(player, 3);
                            listOfPlayers.Add(player);
                            player.PlayerSetup(numberPlusAddition[1], handContainer.GetChild(player.myPosition).GetChild(0));
                            break;

                        case 2: //create exit
                            //print("exit");
                            thisTileEntity = Instantiate(exitPrefab, nextTile.transform);
                            thisTileEntity.name = "Exit";
                            ObjectiveEntity exitObjective = thisTileEntity.GetComponent<ExitEntity>();
                            exitObjective.objective = "Exit";
                            listOfObjectives.Add(exitObjective);
                            break;

                        case 3: //create objective
                            //print("objective");
                            thisTileEntity = Instantiate(objectivePrefab, nextTile.transform);
                            thisTileEntity.name = numberPlusAddition[1];
                            ObjectiveEntity defaultObjective = thisTileEntity.GetComponent<ObjectiveEntity>();
                            defaultObjective.objective = numberPlusAddition[2];
                            listOfObjectives.Add(defaultObjective);
                            break;

                        case 4: //create toggle
                            //print("toggle");
                            thisTileEntity = Instantiate(togglePrefab, nextTile.transform);
                            thisTileEntity.name = numberPlusAddition[2];
                            ToggleEntity defaultToggle = thisTileEntity.GetComponent<ToggleEntity>();
                            defaultToggle.interactCondition = numberPlusAddition[1].ToUpper();
                            defaultToggle.interactInstructions = numberPlusAddition[2].ToUpper();
                            defaultToggle.toggledOn = (numberPlusAddition[3] != "true");

                            try
                            {
                                string[] pointList = numberPlusAddition[4].Split('|');
                                foreach (string patrol in pointList)
                                {
                                    string[] points = patrol.Split(",");
                                    int.TryParse(points[0], out int curX);
                                    int.TryParse(points[1], out int curY);
                                    defaultToggle.targetPoints.Add(new Vector2Int(curX, curY));
                                }
                            }
                            catch (IndexOutOfRangeException)
                            {
                                Debug.Log("failed to get target points");
                                continue;
                            }

                            StartCoroutine(defaultToggle.ObjectiveComplete(null));

                            break;

                        case 10: //create wall
                            //print("wall");
                            thisTileEntity = Instantiate(wallPrefab, nextTile.transform);
                            thisTileEntity.name = "Wall";
                            WallEntity wall = thisTileEntity.GetComponent<WallEntity>();
                            listOfWalls.Add(wall);
                            wall.health = int.Parse(numberPlusAddition[1]);
                            wall.WallDirection(numberPlusAddition[2]);
                            break;

                        case 30: //create +
                            thisTileEntity = Instantiate(wallPlusPrefab, nextTile.transform);
                            thisTileEntity.name = "PlusWall";
                            WallEntity PlusWall = thisTileEntity.GetComponent<WallEntity>();
                            listOfWalls.Add(PlusWall);
                            PlusWall.health = int.Parse(numberPlusAddition[1]);
                            PlusWall.WallDirection(numberPlusAddition[2]);
                            break;

                        case 40: //create T
                            thisTileEntity = Instantiate(wallTSplitPrefab, nextTile.transform);
                            thisTileEntity.name = "T-Wall";
                            WallEntity Twall = thisTileEntity.GetComponent<WallEntity>();
                            listOfWalls.Add(Twall);
                            Twall.health = int.Parse(numberPlusAddition[1]);
                            Twall.WallDirection(numberPlusAddition[2]);
                            break;

                        case 50: //create corner
                            thisTileEntity = Instantiate(wallCornerPrefab, nextTile.transform);
                            thisTileEntity.name = "CornerWall";
                            WallEntity CornerWall = thisTileEntity.GetComponent<WallEntity>();
                            listOfWalls.Add(CornerWall);
                            CornerWall.health = int.Parse(numberPlusAddition[1]);
                            CornerWall.WallDirection(numberPlusAddition[2]);
                            break;

                        case 20: //create guard
                            thisTileEntity = Instantiate(guardPrefab, nextTile.transform);
                            thisTileEntity.name = "Guard";
                            GuardEntity theGuard = thisTileEntity.GetComponent<GuardEntity>();
                            theGuard.movementLeft = theGuard.movesPerTurn;
                            theGuard.direction = StringToDirection(numberPlusAddition[1]);
                            listOfGuards.Add(theGuard);

                            try
                            {
                                string[] patrolList = numberPlusAddition[2].Split('|');
                                foreach (string patrol in patrolList)
                                {
                                    string[] points = patrol.Split(",");
                                    int.TryParse(points[0], out int curX);
                                    int.TryParse(points[1], out int curY);
                                    theGuard.PatrolPoints.Add(new Vector2Int(curX, curY));
                                }
                            }
                            catch (IndexOutOfRangeException){ continue; }
                            break;
                    }

                    try{thisTileEntity.MoveTile(nextTile);}
                    catch (NullReferenceException){continue;}
                }
            }
        }

        for (int i = 0; i < listOfTiles.GetLength(0); i++) //then each tile finds adjacent tiles
        {
            for (int j = 0; j < listOfTiles.GetLength(1); j++)
            {
                try{FindAdjacent(listOfTiles[i, j]);}
                catch (NullReferenceException){continue;}
            }
        }
    }

    Vector2Int StringToDirection(string direction)
    {
        return direction[..1] switch
        {
            "u" => new Vector2Int(0, 1),
            "d" => new Vector2Int(0, -1),
            "l" => new Vector2Int(-1, 0),
            "r" => new Vector2Int(1, 0),
            _ => new Vector2Int(0, 0),
        };
    }

    void FindAdjacent(TileData tile) //check each adjacent tile; if it's not null, add it to the list
    {
        if (tile != null)
        {
            tile.adjacentTiles.Add(FindTile(new Vector2(tile.gridPosition.x + 1, tile.gridPosition.y)));
            tile.adjacentTiles.Add(FindTile(new Vector2(tile.gridPosition.x - 1, tile.gridPosition.y)));
            tile.adjacentTiles.Add(FindTile(new Vector2(tile.gridPosition.x, tile.gridPosition.y + 1)));
            tile.adjacentTiles.Add(FindTile(new Vector2(tile.gridPosition.x, tile.gridPosition.y - 1)));
            tile.adjacentTiles.RemoveAll(item => item == null); //delete all tiles that are null
        }
    }

#endregion

#region Changing Stats

    public void SetEnergy(PlayerEntity player, int n) //if you want to set energy to 2, type SetEnergy(2);
    {
        ChangeEnergy(player, n - (int)player.myEnergy);
    }

    public void ChangeEnergy(PlayerEntity player, int n) //if you want to subtract 3 energy, type ChangeEnergy(-3);
    {
        player.myEnergy += n;
        UpdateStats(player);
    }

    public void SetHealth(PlayerEntity player, int n) //if you want to set health to 2, type SetHealth(2);
    {
        ChangeHealth(player, n - (int)player.health);
    }

    public void ChangeHealth(PlayerEntity player, int n) //if you want to subtract 3 health, type ChangeHealth(-3);
    {
        player.health += n;
        if (player.health >= 4)
            player.health = 4;
        if (player.health <= 0)
            GameOver($"{player.name} lost all their HP.", false);
        UpdateStats(player);
    }

    public void SetMovement(PlayerEntity player, int n) //if you want to set movement to 2, type SetMovement(2);
    {
        ChangeMovement(player, n - (int)player.movementLeft);
    }

    public void ChangeMovement(PlayerEntity player, int n) //if you want to subtract 3 movement, type ChangeMovement(-3);
    {
        player.movementLeft += n;
        UpdateStats(player);
    }

    public void UpdateStats(PlayerEntity player)
    {
        int facesIndex = 0;

        if (player != null)
        {
            stats.text = $"{player.name} | <color=#ffc73b>{player.health} Health <color=#ffffff>" +
                $"| <color=#ecff59>{player.movementLeft} Movement <color=#ffffff>" +
                $"| <color=#59fff4>{player.myEnergy} Energy <color=#ffffff>";

            currentCharacter.text = $"{player.name}";
            health.text = $"Health: {player.health}";
            moves.text = $"Moves: {player.movementLeft}";
            energy.text = $"Energy: {player.myEnergy}";
            
            if(player.name == "Gail")
            {
                facesIndex = 2;
            }
            else if (player.name == "Frankie")
            {
                facesIndex = 0;
            }
            else if (player.name == "WK")
            {
                facesIndex = 1;
            }
            characterFace.sprite = facesSpritesheet[facesIndex];

            deckTracker.text = $"<color=#70f5ff>Draw Pile <color=#ffffff>/ <color=#ff9670>Discard Pile " +
                $"\n\n<color=#70f5ff>{player.myDrawPile.Count} <color=#ffffff>/ <color=#ff9670>{player.myDiscardPile.Count}" +
                $"\n({player.myExhaust.Count} exhausted)";

            if (player.myPosition * -2000 != handContainer.transform.localPosition.x)
            {
                player.MyTurn();
                handContainer.transform.localPosition = new Vector3(player.myPosition * -2000, 0, 0);
            }
        }
        else
        {
            stats.text = "";
            currentCharacter.text = "Character";
            health.text = "Health:";
            moves.text = "Moves:";
            energy.text = "Energy:";
            characterFace.sprite = emptyFace;

            deckTracker.text = "";
            handContainer.transform.localPosition = new Vector3(10000, 10000, 0);
        }

        stats.text += $"\n<color=#75ff59>{listOfObjectives.Count} Objectives Left" +
            $"| {turnCount} Turns Left";

        foreach (PlayerEntity nextPlayer in listOfPlayers)
        {
            nextPlayer.myBar.ChangeText($"{nextPlayer.myHand.Count} Cards; {nextPlayer.health} HP; \n{nextPlayer.movementLeft} Moves; {nextPlayer.myEnergy} Energy");
        }
    }

    public void UpdateInstructions(string instructions)
    {
        this.instructions.text = instructions;
    }

#endregion

#region Misc

    private void Update()
    {
        endTurnButton.gameObject.SetActive(currentTurn == TurnSystem.You);

        if (Input.GetKeyDown(KeyCode.Escape))
            GameOver("You quit.", false);
    }

    private void FixedUpdate()
    {
        if (decrease)
            opacity -= 0.05f;
        else
            opacity += 0.05f;
        if (opacity < 0 || opacity > 1)
            decrease = !decrease;
    }

    public void GameOver(string cause, bool won)
    {
        gameOverText.text = cause;
        gameOverText.transform.parent.gameObject.SetActive(true);

        TMP_Text endStats = GameObject.Find("End Stats").GetComponent<TMP_Text>();
        endStats.text = $"Violent Cards Used: {violentCards}";

        StopAllCoroutines();
        GameObject.Find("Debrief Button").SetActive(won);
    }

    public TileData FindTile(Vector2 vector) //find a tile based off Vector2
    {
        try { return listOfTiles[(int)vector.x, (int)vector.y]; }
        catch (IndexOutOfRangeException) { return null; }
    }

    public TileData FindTile(Vector2Int vector) //find a tile based off Vector2Int
    {
        return FindTile(new Vector2(vector.x, vector.y));
    }

    public static IEnumerator Wait(float timer)
    {
        float wait = timer;
        while (wait > 0)
        {
            wait -= Time.deltaTime;
            yield return null;
        }
    }

    public void FocusOnTile(TileData tile, bool moveMe)
    {
        if (tile != null)
        {
            Camera.main.transform.position = new Vector3(tile.transform.position.x, Camera.main.transform.position.y, tile.transform.position.z);
            if (moveMe)
                StartCoroutine(ChooseMovePlayer(tile.myEntity.GetComponent<PlayerEntity>()));
        }
    }

    public EnvironmentalEntity CreateEnvironmental()
    {
        EnvironmentalEntity newEnviro = Instantiate(environmentPrefab);
        return newEnviro;
    }

#endregion

#region Decision-making

    public void DisableAllTiles()
    {
        for (int i = 0; i < listOfTiles.GetLength(0); i++)
        {
            for (int j = 0; j < listOfTiles.GetLength(1); j++)
            {
                try
                {
                    listOfTiles[i, j].clickable = false;
                    listOfTiles[i, j].moveable = false;
                    listOfTiles[i, j].choosable = false;
                }
                catch (NullReferenceException)
                {
                    continue;
                }
            }
        }
    }

    public void DisableAllCards()
    {
        foreach (Card card in SaveManager.instance.allCards)
        {
            card.DisableCard();
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

    public void WaitForDecision(List<Card> canBeChosen)
    {
        chosenTile = null;
        chosenCard = null;
        DisableAllCards();

        foreach (Card card in canBeChosen)
        {
            card.EnableCard();
        }
    }

    public void WaitForDecision(List<TileData> canBeChosen)
    {
        chosenTile = null;
        chosenCard = null;
        DisableAllTiles();

        foreach (TileData tile in canBeChosen)
        {
            tile.moveable = true;
            tile.clickable = true;
            tile.choosable = true;
        }
    }

    public Collector ConfirmDecision(string header, Vector3 position)
    {
        if (PlayerPrefs.GetInt("Confirm Choices") == 1)
        {
            Debug.LogError($"confirm decision");
            DisableAllCards();
            DisableAllTiles();

            Collector collector = Instantiate(confirmationCollector);
            collector.StatsSetup(header, position);

            collector.AddTextButton("Confirm");
            collector.AddTextButton("Rechoose");
            return collector;
        }
        else
        {
            return null;
        }
    }

#endregion

#region Turn System

    IEnumerator StartPlayerTurn()
    {
        UpdateStats(null);

        foreach(Card card in futureEffects)
            yield return card.NextRoundEffect();
        futureEffects.Clear();

        selectedTile = null;
        beginTurnSound.Post(gameObject);
        BackToStart(true);
    }

    IEnumerator FadeTurnBar(string message)
    {
        DisableAllCards();
        DisableAllTiles();
        turnAlertBar.alpha = 0;
        turnText.text = message;

        while (turnAlertBar.alpha < 1)
        {
            turnAlertBar.alpha += turnFadeSpeed;
            yield return null;
        }
        yield return Wait(turnHangDuration);
        while (turnAlertBar.alpha > 0)
        {
            turnAlertBar.alpha -= turnFadeSpeed;
            yield return null;
        }

        turnAlertBar.alpha = 0;
        
        EnablePlayers();
        /*
        if (lastSelectedPlayer != null)
        {
            Debug.Log($"{lastSelectedPlayer.name} was last selected");
            selectedTile = lastSelectedPlayer.currentTile;
            StartCoroutine(ChooseMovePlayer(lastSelectedPlayer));
        }
        */
    }

    public void EnablePlayers()
    {
        foreach (PlayerEntity player in listOfPlayers)
        {
            player.currentTile.clickable = true;
            //player.currentTile.moveable = true;
        }
    }

    bool AnythingLeftThisTurn()
    {
        foreach (PlayerEntity player in listOfPlayers)
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

    public void BackToStart(bool startTurn)
    {
        if (listOfPlayers.Count > 0)
        {
            currentTurn = TurnSystem.You;
            StopCoroutine(ChooseMovePlayer(lastSelectedPlayer));
            StopCoroutine(ChooseCardPlay(lastSelectedPlayer));

            DisableAllTiles();
            DisableAllCards();

            UpdateStats(lastSelectedPlayer);
            EnablePlayers();
            objectiveButton.gameObject.SetActive(false);

            endTurnImage.color = AnythingLeftThisTurn() ? Color.gray : Color.white;

            if (startTurn)
            {
                StartCoroutine(FadeTurnBar("Player Turn"));
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
                StartCoroutine(ChooseMovePlayer(lastSelectedPlayer));
            }
        }
    }

    public IEnumerator ChooseMovePlayer(PlayerEntity currentPlayer)
    {
        if (lastSelectedPlayer != currentPlayer)
        {
            characterSelectSound.Post(currentPlayer.gameObject);
            lastSelectedPlayer = currentPlayer;
        }

        AkSoundEngine.SetState("Character", currentPlayer.name);

        selectedTile = currentPlayer.currentTile;
        UpdateInstructions("Choose a character to move / play a card.");
        //reset current traced path
        for (int i = 0; i < FullPath.Count; i++)
        {
            FullPath[i].directionIndicator.enabled = false;
        }
        yield return Wait(0.2f);

        List<TileData> possibleTiles = CalculateReachableGrids(currentPlayer.currentTile, currentPlayer.movementLeft, true);
        WaitForDecision(possibleTiles);

        UpdateStats(currentPlayer);
        StartCoroutine(ChooseCardPlay(currentPlayer));
        EnablePlayers();

        objectiveButton.gameObject.SetActive(currentPlayer.CheckForObjectives());
        if (currentPlayer.adjacentObjective != null)
            objectiveButton.GetComponentInChildren<TMP_Text>().text = currentPlayer.adjacentObjective.name;

        while (chosenTile == null)
        {
            if (selectedTile != currentPlayer.currentTile || currentTurn != TurnSystem.You)
            {
                Debug.LogError("switched off movement");
                yield break;
            }
            else
            {
                yield return null;
            }
        }

        currentTurn = TurnSystem.Resolving;
        Collector confirmDecision = ConfirmDecision("Confirm movement?", new Vector2(0, 200));
        if (confirmDecision != null)
        {
            selectedTile = chosenTile;
            yield return confirmDecision.WaitForChoice();
            int decision = confirmDecision.chosenButton;
            Destroy(confirmDecision.gameObject);

            if (decision == 1)
            {
                StopCoroutine(ChooseMovePlayer(currentPlayer));
                BackToStart(false);
                yield break;
            }
        }

        yield return MovePlayer(currentPlayer);

    }

    IEnumerator MovePlayer(PlayerEntity currentPlayer)
    { 
        currentTurn = TurnSystem.Resolving;
        int distanceTraveled = GetDistance(currentPlayer.currentTile.gridPosition, chosenTile.gridPosition);
        ChangeMovement(currentPlayer, -distanceTraveled);

        DisableAllTiles();
        DisableAllCards();

        if (distanceTraveled != 0)
            footsteps.Post(currentPlayer.gameObject);
        yield return (currentPlayer.MovePlayer(FullPath));
        //currentPlayer.MoveTile(chosenTile);

        BackToStart(false);
    }

    IEnumerator ChooseCardPlay(PlayerEntity currentPlayer) //choose a card to play
    {
        if (currentPlayer != null)
        {
            List<Card> canBePlayed = new List<Card>();
            foreach (Card card in currentPlayer.myHand)
            {
                if (card.CanPlay(currentPlayer))
                    canBePlayed.Add(card);
            }
            WaitForDecision(canBePlayed);

            while (chosenCard == null)
            {
                if (currentTurn != TurnSystem.You)
                {
                    yield break;
                }
                else
                {
                    yield return null;
                }
            }

            currentTurn = TurnSystem.Resolving;
            Collector confirmDecision = ConfirmDecision($"Play {chosenCard.name}?", new Vector2(0, -85));
            if (confirmDecision != null)
            {
                yield return confirmDecision.WaitForChoice();
                int decision = confirmDecision.chosenButton;
                Destroy(confirmDecision.gameObject);

                if (decision == 1)
                {
                    StopCoroutine(ChooseMovePlayer(currentPlayer));
                    BackToStart(false);
                    yield break;
                }
            }

            yield return currentPlayer.PlayCard(chosenCard, true);
            BackToStart(false);
        }
    }

    void DoObjective()
    {
        StartCoroutine(ResolveObjective());
    }

    IEnumerator ResolveObjective()
    {
        if (currentTurn == TurnSystem.You)
        {
            objectiveButton.gameObject.SetActive(false);
            currentTurn = TurnSystem.Resolving;
            if (lastSelectedPlayer != null && lastSelectedPlayer.adjacentObjective != null)
                yield return (lastSelectedPlayer.adjacentObjective.ObjectiveComplete(lastSelectedPlayer));

            BackToStart(false);
        }
    }

    void Regain()
    {
        print("Begin Regain");
        StopAllCoroutines();
        objectiveButton.gameObject.SetActive(false);
        UpdateInstructions("");

        foreach (PlayerEntity player in listOfPlayers)
        {
            SetEnergy(player, 3);
            SetMovement(player, player.movesPerTurn);
            player.PlusCards(5 - player.myHand.Count);
            player.cardsPlayed.Clear();
            UpdateStats(null);
        }

        StartCoroutine(EnvironmentalPhase());
    }

    IEnumerator EnvironmentalPhase()
    {
        selectedTile = null;
        currentTurn = TurnSystem.Environmentals;
        DisableAllTiles();
        DisableAllCards();

        foreach (PlayerEntity player in listOfPlayers)
        {
            if (player.stunned > 0)
                player.stunned--;
        }

        foreach (EnvironmentalEntity environment in listOfEnvironmentals)
        {
            if (environment != null)
            {
                FocusOnTile(environment.currentTile, false);
                yield return environment.EndOfTurn();
            }
        }
        StartCoroutine(EndTurn());
    }

    IEnumerator EndTurn() //Starts Guard Phase
    {
        //erases current visible path
        for (int i = 0; i < FullPath.Count; i++)
        {
            FullPath[i].directionIndicator.enabled = false;
        }
        endTurnSound.Post(gameObject);
        foreach (PlayerEntity player in listOfPlayers)
            yield return player.EndOfTurn();

        currentTurn = TurnSystem.Enemy;
        yield return FadeTurnBar("Company Turn");
        foreach (GuardEntity guard in listOfGuards)
        {
            FocusOnTile(guard.currentTile, false);
            yield return (guard.EndOfTurn());
            guard.movementLeft = guard.movesPerTurn;
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

#region Pathfinding

    public HashSet<Vector2Int> line(Vector2Int p1, Vector2Int p2)
    {
        HashSet<Vector2Int> points = new HashSet<Vector2Int>();
        float distance = GetDistance(p1, p2);
        for (float step = 0; step <= distance; step++)
        {
            float t = step/distance;
            Vector2 midPointRaw = Vector2.Lerp(p1, p2, t);
            Vector2Int midPoint = new Vector2Int(Mathf.RoundToInt(midPointRaw.x), Mathf.RoundToInt(midPointRaw.y));
            points.Add(midPoint);
        }
        return points;
    }

    //gets the distance (in gridspaces) between two gridspaces
    public int GetDistance(Vector2Int a, Vector2Int b)
    {
        int distX = Mathf.Abs(a.x - b.x);
        int distY = Mathf.Abs(a.y - b.y);
        return distY + distX;
    }

    //find all grids that can be moved to
    public List<TileData> CalculateReachableGrids(TileData startLocation, int movementSpeed, bool considerEntities)
    {
        List<TileData> reachableGrids = new List<TileData>();

        //First in first out
        Queue<(TileData, int)> queue = new Queue<(TileData, int)>();

        //HashSet is a simple collection without orders
        HashSet<TileData> visited = new HashSet<TileData>();

        queue.Enqueue((startLocation, 0));
        visited.Add(startLocation);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            TileData SelectTile = current.Item1;
            int cost = current.Item2;

            if (cost <= movementSpeed)
            {
                reachableGrids.Add(SelectTile);;
                //FindAdjacent(SelectTile);
                foreach (TileData neighbor in SelectTile.adjacentTiles)
                {
                    int newCost;
                    if (neighbor.myEntity != null && considerEntities)
                    {
                        newCost = cost + neighbor.myEntity.MoveCost;
                    }
                    else
                    {
                        newCost = cost + 1;
                    }

                    if (!visited.Contains(neighbor) && newCost <= movementSpeed )
                    {
                        if (neighbor.myEntity == null || considerEntities)
                        {
                            queue.Enqueue((neighbor, newCost));
                            visited.Add(neighbor);
                        }
                        else if (neighbor.myEntity.MoveCost! >= 999)
                        {
                            queue.Enqueue((neighbor, newCost));
                            visited.Add(neighbor);
                        }

                    }
                }
            }
        }
        return reachableGrids;
    }

    //used for determining how sound moves, uses different move cost limits than regular directions
    public List<TileData> CalculateIntensity(TileData startLocation, int movementSpeed, bool considerEntities)
    {
        //print("calculating intensity");
        List<TileData> reachableGrids = new List<TileData>();

        //First in first out
        Queue<(TileData, int)> queue = new Queue<(TileData, int)>();

        //HashSet is a simple collection without orders
        HashSet<TileData> visited = new HashSet<TileData>();

        queue.Enqueue((startLocation, 0));
        visited.Add(startLocation);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            TileData SelectTile = current.Item1;
            int cost = current.Item2;
            //print("cost: " + cost);
            //print("current: " + current.Item1.gridPosition);
            if (cost <= movementSpeed)
            {
                reachableGrids.Add(SelectTile); ;
                //FindAdjacent(SelectTile);
                foreach (TileData neighbor in SelectTile.adjacentTiles)
                {
                    int newCost;
                    if (neighbor.myEntity != null && considerEntities)
                    {
                        //print("moving over " + neighbor.myEntity.tag);
                        newCost = cost + neighbor.myEntity.SoundCost;
                    }
                    else
                    {
                        newCost = cost + 1;
                    }

                    if (!visited.Contains(neighbor) && newCost <= movementSpeed)
                    {
                        if (neighbor.myEntity == null || considerEntities)
                        {
                            queue.Enqueue((neighbor, newCost));
                            visited.Add(neighbor);
                        }
                        else if (neighbor.myEntity.SoundCost! >= 999)
                        {
                            queue.Enqueue((neighbor, newCost));
                            visited.Add(neighbor);
                        }

                    }
                }
            }
        }
        //print("finished calculating intensity");
        return reachableGrids;
    }

    //find fastest way to get from one point to another
    //startLocation - the tile the entity is starting from
    //targetLocation - the tile the entity wants to move to
    //movementPoints - the max amount of spaces the entity can move 
    //singleMovement - whether or not the entity will stop after 1 spaces moved
    //considerEntities - whether or not the entity will see players as pathable tiles (used for guards looking for players)

    public void CalculatePathfinding(TileData startLocation, TileData targetLocation, int movementPoints, bool singleMovement, bool considerPlayers)
    {
        //open list is all the current neighbors to the analyzed path, the tiles are avalible to be scanned and haven't been checked yet
        List<AStarNode> openList = new List<AStarNode>();
        //these tiles have all been scanned, tiles in the closed list can't be added to the open list
        HashSet<TileData> closedList = new HashSet<TileData>();
        //dictionary which uses neighbors to either call or create new AStarNodes to add to the open list
        Dictionary<TileData, AStarNode> nodeLookup = new Dictionary<TileData, AStarNode>();

        AStarNode startNode = new AStarNode { ATileData = startLocation };
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            AStarNode currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].FCost < currentNode.FCost || openList[i].FCost == currentNode.FCost && openList[i].HCost < currentNode.HCost)
                {
                    currentNode = openList[i];
                }
            }
            openList.Remove(currentNode);
            closedList.Add(currentNode.ATileData);
            if (currentNode.ATileData.gridPosition == targetLocation.gridPosition)
            {
                RetracePath(startNode, currentNode, movementPoints, singleMovement);
                return;
            }
            foreach (TileData neighbor in currentNode.ATileData.adjacentTiles)
            {
                int movementCostToNeighbor = 0;
                if (closedList.Contains(neighbor))
                {
                    continue;
                }
                if (neighbor.myEntity != null)
                {
                    if (considerPlayers && neighbor.myEntity.tag == "Player")
                    {
                        movementCostToNeighbor = currentNode.GCost + GetDistance(currentNode.ATileData.gridPosition, neighbor.gridPosition);
                    }
                    else
                    {
                        if (neighbor.gridPosition != targetLocation.gridPosition && neighbor.myEntity.MoveCost > 100)
                        {
                            continue;
                        }
                        movementCostToNeighbor = currentNode.GCost + GetDistance(currentNode.ATileData.gridPosition, neighbor.gridPosition) * neighbor.myEntity.MoveCost;
                    }

                }
                else
                {
                    movementCostToNeighbor = currentNode.GCost + GetDistance(currentNode.ATileData.gridPosition, neighbor.gridPosition);
                }
                AStarNode neighborNode;
                if (!nodeLookup.ContainsKey(neighbor))
                {
                    neighborNode = new AStarNode { ATileData = neighbor };
                    nodeLookup[neighbor] = neighborNode;
                }
                else
                {
                    neighborNode = nodeLookup[neighbor];
                }
                if (movementCostToNeighbor < neighborNode.GCost || !openList.Contains(neighborNode))
                {
                    neighborNode.GCost = movementCostToNeighbor;
                    neighborNode.HCost = GetDistance(neighbor.gridPosition, targetLocation.gridPosition);
                    neighborNode.Parent = currentNode;
                    //print(neighborNode.ATileData.gridPosition + "'s parent is " + currentNode.ATileData.gridPosition);
                    // Add neighbor to the open list if it's not already there
                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }
    }

    public void RetracePath(AStarNode startNode, AStarNode endNode, int actionPoint, bool singleMovement)
    {
        FullPath.Clear();
        List<AStarNode> path = new List<AStarNode>();
        AStarNode currentNode = endNode;
        while (currentNode != startNode)
        {
            //print("Current stage on path is" + currentNode.ATileData.gridPosition);
            path.Add(currentNode);
            currentNode = currentNode.Parent;

        }
        path.Reverse();

        int pathCost = 0;
        if (!singleMovement)
        {
            foreach (AStarNode CurrentTile in path)
            {
                if (CurrentTile.ATileData.myEntity != null)
                {
                    pathCost += CurrentTile.ATileData.myEntity.MoveCost;
                }
                else
                {
                    pathCost++;
                }

                // If the path cost exceeds the action points available, stop displaying the path
                if (pathCost > actionPoint)
                {
                    CurrentAvailableMoveTarget = CurrentTile.ATileData;
                    FullPath.Add(CurrentTile.ATileData);
                    continue;
                }
                // Update the current available move target and display the pathfinding visualization with the path cost
                CurrentAvailableMoveTarget = CurrentTile.ATileData;
                FullPath.Add(CurrentTile.ATileData);
            }
        }
        else
        {
            CurrentAvailableMoveTarget = path[0].ATileData;
        }
    }

    //find fastest way to get from one point to another

    #endregion

}