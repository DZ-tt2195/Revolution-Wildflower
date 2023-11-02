using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using UnityEngine.SceneManagement;

public class AStarNode
{
    public TileData ATileData;
    public AStarNode Parent;
    //travel distance from node to start node
    public int GCost;
    //travel distance from node to target node
    public int HCost;
    //Astar value of this tile, the lower it is, the better for the pathfinder.
    public int FCost => GCost + HCost;
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

    [Foldout("Movement", true)]
        [Tooltip("Current Selected Tile")][ReadOnly] public TileData selectedTile;
        [Tooltip("Quick reference to current movable tile")][ReadOnly] public TileData CurrentAvailableMoveTarget;

    [Foldout("UI Elements", true)]
        [Tooltip("Your hand in the canvas")] [ReadOnly] public Transform handContainer;
        [Tooltip("The bar in the bottom center of the screen")] Transform informationImage;
        [Tooltip("All the player's stats in text form")] TMP_Text stats;
        [Tooltip("Instructions for what the player is allowed to do right now")] TMP_Text instructions;
        [Tooltip("End the turn")] Button endTurnButton;
        [Tooltip("Complete an objective you're next to")] [ReadOnly] public Button objectiveButton;
        [Tooltip("info on entities")] [ReadOnly] public EntityToolTip toolTip;
        [Tooltip("the text that gets displayed when you game over")] TMP_Text gameOverText;
        [Tooltip("tracks number of cards in deck and discard pile")] TMP_Text deckTracker;

    [Foldout("Grid", true)]
        [Tooltip("Tiles in the inspector")] Transform gridContainer;
        [Tooltip("Storage of tiles")][ReadOnly] public TileData[,] listOfTiles;
        [Tooltip("Spacing between tiles")][SerializeField] float tileSpacing;

    [Foldout("Prefabs", true)]
        [Tooltip("Floor tile prefab")] [SerializeField] TileData floorTilePrefab;
        [Tooltip("Player prefab")][SerializeField] PlayerEntity playerPrefab;
        [Tooltip("Wall prefab")][SerializeField] WallEntity wallPrefab;
        [Tooltip("Guard prefab")][SerializeField] GuardEntity guardPrefab;
        [Tooltip("Objective prefab")][SerializeField] ObjectiveEntity objectivePrefab;
        [Tooltip("Guard prefab")][SerializeField] ExitEntity exitPrefab;

    [Foldout("Setup", true)]
        [Tooltip("Amount of turns before a game over")] public int turnCount;
        [Tooltip("Name of the level tsv")] [SerializeField] string levelToLoad;

    public enum TurnSystem { You, Resolving, Environmentals, Enemy };
    [Foldout("Turn System", true)]
        [Tooltip("What's happening in the game")][ReadOnly] public TurnSystem currentTurn;
        [Tooltip("Effects to do on future turns")][ReadOnly] public List<Card> futureEffects = new List<Card>();

    [Foldout("Sound Effects", true)]
        [SerializeField] AudioClip button;
        [SerializeField] AudioClip endTurnSound;
        [SerializeField] AudioClip footsteps;

    #endregion

#region Setup

    void Awake()
    {
        instance = this;

        informationImage = GameObject.Find("Information Image").transform;
        stats = informationImage.GetChild(0).GetComponent<TMP_Text>();
        instructions = informationImage.GetChild(1).GetComponent<TMP_Text>();
        deckTracker = GameObject.Find("Deck Tracker").GetComponent<TMP_Text>();

        endTurnButton = GameObject.Find("End Turn Button").GetComponent<Button>();
        endTurnButton.onClick.AddListener(Regain);
        objectiveButton = GameObject.Find("Objective Button").GetComponent<Button>();
        objectiveButton.onClick.AddListener(ResolveObjective);

        handContainer = GameObject.Find("Hand Container").transform;
        gridContainer = GameObject.Find("Grid Container").transform;
    }

    void GetCards()
    {
        Transform emptyObject = new GameObject("Card Container").transform;
        handContainer.transform.localPosition = new Vector3(10000, 10000, 0);
        foreach (List<Card> cardList in SaveManager.instance.characterCards)
        {
            foreach(Card card in cardList)
                card.transform.SetParent(emptyObject);
        }

        for (int i = 0; i < listOfPlayers.Count; i++)
        {
            PlayerEntity player = listOfPlayers[i];
            player.handTransform = handContainer.GetChild(player.myPosition).GetChild(0);
            SetEnergy(player, 3);

            foreach (string cardName in SaveManager.instance.currentSaveData.savedDecks[i])
            {
                Card nextCard = emptyObject.transform.Find(cardName).GetComponent<Card>();
                nextCard.transform.SetParent(player.transform);
                player.myDrawPile.Add(nextCard);
                nextCard.transform.localPosition = new Vector3(10000, 10000, 0); //send the card far away where you can't see it anymore
                nextCard.choiceScript.DisableButton();
            }

            player.myDrawPile.Shuffle(); //shuffle your deck
            player.DrawCards(5);
        }
    }

    void GetTiles()
    {
        //generate grids and entities from csv
        string[,] newGrid = LevelLoader.LoadLevelGrid(levelToLoad);
        listOfTiles = new TileData[newGrid.GetLength(0), newGrid.GetLength(1)];

        for (int i = 0; i < listOfTiles.GetLength(0); i++)
        {
            for (int j = 0; j < listOfTiles.GetLength(1); j++)
            {
                string[] numberPlusAddition = newGrid[i, j].Split("/");
                if (numberPlusAddition[0] != "")
                {
                    TileData nextTile = Instantiate(floorTilePrefab);
                    nextTile.transform.SetParent(gridContainer);
                    nextTile.transform.position = new Vector3(i * -tileSpacing, 0, j * -tileSpacing);
                    nextTile.name = $"Tile {i},{j}";
                    listOfTiles[i, j] = nextTile;
                    nextTile.gridPosition = new Vector2Int(i, j);

                    Entity thisTileEntity = null;
                    switch (numberPlusAddition[0])
                    {
                        case "1": //create player
                            thisTileEntity = Instantiate(playerPrefab, nextTile.transform);
                            PlayerEntity player = thisTileEntity.GetComponent<PlayerEntity>();
                            player.movementLeft = player.movesPerTurn;
                            player.myPosition = listOfPlayers.Count;
                            listOfPlayers.Add(player);
                            thisTileEntity.name = $"Player {listOfPlayers.Count}";
                            FocusOnPlayer();
                            break;
                        case "2": //create exit
                            thisTileEntity = Instantiate(exitPrefab, nextTile.transform);
                            thisTileEntity.name = "Exit";
                            ObjectiveEntity exitObjective = thisTileEntity.GetComponent<ExitEntity>();
                            listOfObjectives.Add(exitObjective);
                            break;
                        case "3": //create objective
                            thisTileEntity = Instantiate(objectivePrefab, nextTile.transform);
                            thisTileEntity.name = numberPlusAddition[1];
                            ObjectiveEntity defaultObjective = thisTileEntity.GetComponent<ObjectiveEntity>();
                            listOfObjectives.Add(defaultObjective);
                            break;
                        case "10": //create weak wall
                            thisTileEntity = Instantiate(wallPrefab, nextTile.transform);
                            thisTileEntity.name = "Wall";
                            WallEntity weakWall = thisTileEntity.GetComponent<WallEntity>();
                            //weakWall.WallDirection(numberPlusAddition[1]);
                            listOfWalls.Add(weakWall);
                            weakWall.health = 2;
                            break;
                        case "11": //create med wall
                            thisTileEntity = Instantiate(wallPrefab, nextTile.transform);
                            thisTileEntity.name = "Wall";
                            WallEntity medWall = thisTileEntity.GetComponent<WallEntity>();
                            //medWall.WallDirection(numberPlusAddition[1]);
                            listOfWalls.Add(medWall);
                            medWall.health = 4;
                            break;
                        case "12": //create strong wall
                            thisTileEntity = Instantiate(wallPrefab, nextTile.transform);
                            thisTileEntity.name = "Wall";
                            WallEntity strongWall = thisTileEntity.GetComponent<WallEntity>();
                            //strongWall.WallDirection(numberPlusAddition[1]);
                            listOfWalls.Add(strongWall);
                            strongWall.health = 6;
                            break;
                        case "20": //create guard
                            thisTileEntity = Instantiate(guardPrefab, nextTile.transform);
                            thisTileEntity.name = "Guard";
                            GuardEntity theGuard = thisTileEntity.GetComponent<GuardEntity>();
                            theGuard.movementLeft = theGuard.movesPerTurn;
                            theGuard.direction = StringToDirection(numberPlusAddition[1]);
                            listOfGuards.Add(theGuard);
                            string[] patrolList = numberPlusAddition[2].Split('|');
                            foreach (string patrol in patrolList)
                            {
                                string[] points = patrol.Split(",");
                                int curX = 0;
                                int curY = 0;
                                int.TryParse(points[0], out curX);
                                int.TryParse(points[1], out curY);
                                theGuard.PatrolPoints.Add(new Vector2Int(curX, curY));
                            }
                            print("This guard will patrol!");
                            foreach (Vector2Int cord in theGuard.PatrolPoints)
                            {
                                print("Next point: " + cord);
                            }
                            break;
                    }
                    try
                    {
                        thisTileEntity.MoveTile(nextTile);
                    }
                    catch (NullReferenceException)
                    {
                        continue;
                    }
                }
            }
        }

        for (int i = 0; i < listOfTiles.GetLength(0); i++) //then find adjacent tiles
        {
            for (int j = 0; j < listOfTiles.GetLength(1); j++)
            {
                try
                {
                    FindAdjacent(listOfTiles[i, j]);
                }
                catch (NullReferenceException)
                {
                    continue;
                }
            }
        }
    }

    void Start()
    {
        if (turnCount <= 0)
            throw new Exception("Didn't set turn count in NewManager (has to be > 0");

        gameOverText = GameObject.Find("Game Over").transform.GetChild(0).GetComponent<TMP_Text>();
        gameOverText.transform.parent.gameObject.SetActive(false);

        GetTiles();
        GetCards();

        StartCoroutine(StartPlayerTurn());
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

    public TileData FindTile(Vector2 vector) //find a tile based off Vector2
    {
        try
        {
            return listOfTiles[(int)vector.x, (int)vector.y];
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
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

    private void Update()
    {
        endTurnButton.gameObject.SetActive(currentTurn == TurnSystem.You);
        if (Input.GetKeyDown(KeyCode.Space))
            GameOver("You quit.");
    }

    public void FocusOnPlayer()
    {
        Camera.main.transform.position = new Vector3(listOfPlayers[0].transform.position.x-13, Camera.main.transform.position.y, listOfPlayers[0].transform.position.z+12);
        Debug.Log("FocusOnPlayer() hasn't been rewritten yet");
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
        UpdateStats(player);
        if (player.health <= 0)
            GameOver($"{player.name} lost all their HP.");
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
    public void ResolveObjective()
    {
        foreach (PlayerEntity player in listOfPlayers)
        {
            try
            {
                StartCoroutine(player.adjacentObjective.ObjectiveComplete());
            }
            catch (NullReferenceException)
            {
                continue;
            }
        }
    }

    void UpdateStats(PlayerEntity player)
    {
        if (player != null)
        {
            stats.text = $"{player.name} | <color=#ffc73b>{player.health} Health <color=#ffffff>" +
                $"| <color=#ecff59>{player.movementLeft} Movement <color=#ffffff>" +
                $"| <color=#59fff4>{player.myEnergy} Energy <color=#ffffff>";
            deckTracker.text = $"<color=#70f5ff>Draw Pile <color=#ffffff>/ <color=#ff9670>Discard Pile " +
                $"\n\n<color=#70f5ff>{player.myDrawPile.Count} <color=#ffffff>/ <color=#ff9670>{player.myDiscardPile.Count}";
            handContainer.transform.localPosition = new Vector3(player.myPosition * -2000, -75, 0);
        }
        else
        {
            stats.text = "";
            deckTracker.text = "";
            handContainer.transform.localPosition = new Vector3(10000, 10000, 0);
        }

        stats.text += $"\n<color=#75ff59>{listOfObjectives.Count} Objectives Left" +
            $"| {turnCount} Turns Left";
    }

    public void UpdateInstructions(string instructions)
    {
        this.instructions.text = instructions;
    }

#endregion

#region Turn System 

    public void GameOver(string cause)
    {
        gameOverText.text = cause;
        gameOverText.transform.parent.gameObject.SetActive(true);
        StopAllCoroutines();
        CollectTime.EndTime();
    }

    IEnumerator StartPlayerTurn()
    {
        foreach(Card card in futureEffects)
            yield return card.NextRoundEffect();
        futureEffects.Clear();
        UpdateStats(null);
        BackToStart();
    }

    void BackToStart()
    {
        currentTurn = TurnSystem.You;
        UpdateInstructions("Choose a character to move / play a card.");
        selectedTile = null;

        ChoiceManager.instance.DisableAllTiles();
        ChoiceManager.instance.DisableAllCards();

        foreach (PlayerEntity player in listOfPlayers)
            player.currentTile.moveable = true;
    }

    public IEnumerator ChooseMovePlayer(TileData currentTile)
    {
        PlayerEntity currentPlayer = currentTile.myEntity.GetComponent<PlayerEntity>();
        List<TileData> possibleTiles = CalculateReachableGrids(currentTile, currentPlayer.movementLeft, true);
        ChoiceManager.instance.ChooseTile(possibleTiles);
        UpdateStats(currentPlayer);
        StartCoroutine(ChooseCardPlay(currentPlayer));

        while (ChoiceManager.instance.chosenTile == null)
        {
            if (selectedTile != currentTile)
            {
                ChoiceManager.instance.DisableAllTiles();
                yield break;
            }
            else
            {
                yield return null;
            }
        }

        MovePlayer(currentPlayer);
    }

    void MovePlayer(PlayerEntity currentPlayer)
    { 
        currentTurn = TurnSystem.Resolving;
        int distanceTraveled = GetDistance(currentPlayer.currentTile, ChoiceManager.instance.chosenTile);
        ChangeMovement(currentPlayer, -distanceTraveled);
        SoundManager.instance.PlaySound(footsteps);
        currentPlayer.MoveTile(ChoiceManager.instance.chosenTile);
        BackToStart();
    }

    IEnumerator ChooseCardPlay(PlayerEntity player) //choose a card to play
    {
        List<Card> canBePlayed = new List<Card>();
        foreach (Card card in player.myHand)
        {
            if (card.CanPlay(player))
                canBePlayed.Add(card);
        }
        ChoiceManager.instance.ChooseCard(canBePlayed);

        while (ChoiceManager.instance.chosenCard == null)
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
        yield return PlayCard(player, ChoiceManager.instance.chosenCard);
    }

    IEnumerator PlayCard(PlayerEntity player, Card playMe) //resolve that card
    {
        currentTurn = TurnSystem.Resolving;
        SoundManager.instance.PlaySound(playMe.cardPlay);

        player.DiscardFromHand(playMe);
        ChangeEnergy(player, -playMe.energyCost);
        yield return playMe.OnPlayEffect();

        futureEffects.Add(playMe);
        player.cardsPlayed.Add(playMe);
        BackToStart();
    }

    public void Regain()
    {
        StopAllCoroutines();
        foreach (PlayerEntity player in listOfPlayers)
        {
            SetEnergy(player, 3);
            SetMovement(player, player.movesPerTurn);
            player.DrawCards(5 - player.myHand.Count);
            player.cardsPlayed.Clear();
            UpdateStats(null);
        }
        StartCoroutine(EnvironmentalPhase());
    }

    IEnumerator EnvironmentalPhase()
    {
        selectedTile = null;
        currentTurn = TurnSystem.Environmentals;
        ChoiceManager.instance.DisableAllTiles();
        ChoiceManager.instance.DisableAllCards();

        currentTurn = TurnSystem.Environmentals;
        foreach (EnvironmentalEntity environment in  listOfEnvironmentals)
        {
            yield return environment.EndOfTurn();
        }
        StartCoroutine(EndTurn());
        yield return null;
    }

    IEnumerator EndTurn() //Starts Guard Phase
    {
        SoundManager.instance.PlaySound(endTurnSound);
        foreach (PlayerEntity player in listOfPlayers)
            yield return player.EndOfTurn();

        //sets turn to the enemies, and counts through the grid activating all enemies simultaniously
        currentTurn = TurnSystem.Enemy;

        CoroutineGroup group = new CoroutineGroup(this);
        foreach (GuardEntity guard in listOfGuards)
            group.StartCoroutine(guard.EndOfTurn());
        while (group.AnyProcessing)
            yield return null;

        turnCount--;
        if (turnCount == 0)
        {
            GameOver("You ran out of time.");
        }
        else
        {
            StartCoroutine(StartPlayerTurn());
        }
    }

#endregion

#region Pathfinding

    //gets the distance (in gridspaces) between two gridspaces
    public int GetDistance(TileData a, TileData b)
    {
        int distX = Mathf.Abs(a.gridPosition.x - b.gridPosition.x);
        int distY = Mathf.Abs(a.gridPosition.y - b.gridPosition.y);
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

    //find fastest way to get from one point to another

    public void CalculatePathfinding(TileData startLocation, TileData targetLocation, int movementPoints, bool singleMovement)
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
                    if (neighbor.gridPosition != targetLocation.gridPosition && neighbor.myEntity.MoveCost > 100)
                    {
                        continue;
                    }
                    movementCostToNeighbor = currentNode.GCost + GetDistance(currentNode.ATileData, neighbor) * neighbor.myEntity.MoveCost;
                }
                else
                {
                    movementCostToNeighbor = currentNode.GCost + GetDistance(currentNode.ATileData, neighbor);
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
                    neighborNode.HCost = GetDistance(neighbor, targetLocation);
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
                    continue;
                }
                // Update the current available move target and display the pathfinding visualization with the path cost
                CurrentAvailableMoveTarget = CurrentTile.ATileData;
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