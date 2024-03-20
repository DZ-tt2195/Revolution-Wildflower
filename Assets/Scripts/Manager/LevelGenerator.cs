using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;

public class LevelGenerator : MonoBehaviour
{

#region Variables

    public static LevelGenerator instance;

    [Foldout("Lists of things", true)]
    [Tooltip("Reference to players")][ReadOnly] public List<PlayerEntity> listOfPlayers = new();
    [Tooltip("Reference to walls")][ReadOnly] public List<WallEntity> listOfWalls = new();
    [Tooltip("Reference to guards")][ReadOnly] public List<GuardEntity> listOfGuards = new();
    [Tooltip("Reference to active environmental objects")][ReadOnly] public List<EnvironmentalEntity> listOfEnvironmentals = new();
    [Tooltip("Reference to objectives")][ReadOnly] public List<ObjectiveEntity> listOfObjectives = new();

    [Foldout("Grid of tiles", true)]
    [Tooltip("Tiles in the inspector")] Transform gridContainer;
    [Tooltip("Storage of tiles")][ReadOnly] public TileData[,] listOfTiles;
    [Tooltip("Spacing between tiles")][SerializeField] float tileSpacing;

    [Foldout("Prefabs", true)]
    [Tooltip("Floor tile prefab")][SerializeField] TileData floorTilePrefab;
    [Tooltip("Player prefab")][SerializeField] PlayerEntity playerPrefab;
    [Tooltip("Wall prefab")][SerializeField] WallEntity wallPrefab;
    [Tooltip("Wall T prefab")][SerializeField] WallEntity wallTSplitPrefab;
    [Tooltip("Wall Corner prefab")][SerializeField] WallEntity wallCornerPrefab;
    [Tooltip("Wall Plus prefab")][SerializeField] WallEntity wallPlusPrefab;
    [Tooltip("Guard prefab")][SerializeField] GuardEntity guardPrefab;
    [Tooltip("static guard prefab")][SerializeField] StaticGuard staticGuardPrefab;
    [Tooltip("Objective prefab")][SerializeField] ObjectiveEntity objectivePrefab;
    [Tooltip("Toggle prefab")][SerializeField] ToggleEntity togglePrefab;
    [Tooltip("Environmental prefab")][SerializeField] EnvironmentalEntity environmentPrefab;

    #endregion

#region Setup

    private void Awake()
    {
        instance = this;
        gridContainer = GameObject.Find("Grid Container").transform;
    }

    private void Start()
    {
        int levelToLoad = SaveManager.instance.currentSaveData.currentLevel;

        PlayerEntity defaultPlayer = playerPrefab;
        LevelUIManager.instance.healthBar.SetMaximumValue(defaultPlayer.health);
        LevelUIManager.instance.movementBar.SetMaximumValue(defaultPlayer.maxMovement);
        LevelUIManager.instance.energyBar.SetMaximumValue(defaultPlayer.maxEnergy);

        GenerateTiles(levelToLoad);

        TutorialManager.SetLevelStartParameters(SaveManager.instance.levelSheets[levelToLoad]);
    }

    /// <summary>
    /// generate all the tiles in the level
    /// </summary>
    void GenerateTiles(int levelToLoad)
    {
        string[,] newGrid = LevelLoader.LoadLevelGrid(SaveManager.instance.levelSheets[levelToLoad]);
        listOfTiles = new TileData[newGrid.GetLength(0), newGrid.GetLength(1)];
        Transform playerButtons = GameObject.Find("Player Buttons").transform;

        for (int i = 0; i < listOfTiles.GetLength(0); i++) //generate all tiles
        {
            for (int j = 0; j < listOfTiles.GetLength(1); j++)
            {
                try { newGrid[i, j] = newGrid[i, j].Trim().Replace("\"", "").Replace("]", ""); }
                catch (NullReferenceException) { continue; }

                if (newGrid[i, j] != "")
                {
                    TileData nextTile = Instantiate(floorTilePrefab);
                    nextTile.transform.SetParent(gridContainer);
                    nextTile.transform.position = new Vector3(i * -tileSpacing, 0, j * -tileSpacing);
                    nextTile.name = $"Tile {i},{j}";
                    listOfTiles[i, j] = nextTile;
                    nextTile.gridPosition = new Vector2Int(i, j);

                    Entity thisTileEntity = null;
                    string[] numberPlusAddition = newGrid[i, j].Split("|");

                    switch (int.Parse(numberPlusAddition[0]))
                    {
                        case 1: //create player
                            thisTileEntity = Instantiate(playerPrefab, nextTile.transform);
                            PlayerEntity player = thisTileEntity.GetComponent<PlayerEntity>();
                            player.myPosition = listOfPlayers.Count;
                            player.myBar = playerButtons.GetChild(listOfPlayers.Count).GetComponent<PlayerBar>();
                            player.SetEnergy(player.maxEnergy);
                            player.SetMovement(player.maxMovement);
                            listOfPlayers.Add(player);
                            player.PlayerSetup(numberPlusAddition[1], LevelUIManager.instance.handContainer.GetChild(player.myPosition).GetChild(0));
                            break;

                        case 2: //create exit
                            nextTile.myType = TileData.TileType.Exit;
                            break;

                        case 5: //create all exit
                            nextTile.myType = TileData.TileType.AllExit;
                            break;

                        case 3: //create objective
                            thisTileEntity = Instantiate(objectivePrefab, nextTile.transform);
                            thisTileEntity.name = numberPlusAddition[1];
                            ObjectiveEntity defaultObjective = thisTileEntity.GetComponent<ObjectiveEntity>();
                            defaultObjective.objective = numberPlusAddition[2];

                            if (numberPlusAddition.Length > 3)
                            {
                                Debug.Log(numberPlusAddition.Length);
                                if (numberPlusAddition[3] != "null")
                                {
                                    defaultObjective.instructionsWhenCompleted = numberPlusAddition[3].ToUpper().Trim();
                                }
                            }

                            if (numberPlusAddition.Length > 4)
                            {
                                if (numberPlusAddition[4] != "null")
                                {
                                    defaultObjective.textAssetFile = numberPlusAddition[4];
                                }
                            }


                            listOfObjectives.Add(defaultObjective);
                            break;

                        case 4: //create toggle
                            thisTileEntity = Instantiate(togglePrefab, nextTile.transform);
                            thisTileEntity.name = numberPlusAddition[2];
                            ToggleEntity defaultToggle = thisTileEntity.GetComponent<ToggleEntity>();
                            defaultToggle.interactCondition = numberPlusAddition[1].ToUpper().Trim();
                            defaultToggle.interactInstructions = numberPlusAddition[2].ToUpper().Trim();
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
                            thisTileEntity = Instantiate(wallPrefab, nextTile.transform);
                            thisTileEntity.name = "Wall";
                            WallEntity wall = thisTileEntity.GetComponent<WallEntity>();
                            listOfWalls.Add(wall);
                            wall.WallDirection(int.Parse(numberPlusAddition[1]), numberPlusAddition[2]);
                            break;

                        case 30: //create +
                            thisTileEntity = Instantiate(wallPlusPrefab, nextTile.transform);
                            thisTileEntity.name = "PlusWall";
                            WallEntity PlusWall = thisTileEntity.GetComponent<WallEntity>();
                            listOfWalls.Add(PlusWall);
                            PlusWall.WallDirection(int.Parse(numberPlusAddition[1]), numberPlusAddition[2]);
                            break;

                        case 40: //create T
                            thisTileEntity = Instantiate(wallTSplitPrefab, nextTile.transform);
                            thisTileEntity.name = "T-Wall";
                            WallEntity Twall = thisTileEntity.GetComponent<WallEntity>();
                            listOfWalls.Add(Twall);
                            Twall.WallDirection(int.Parse(numberPlusAddition[1]), numberPlusAddition[2]);
                            break;

                        case 50: //create corner
                            thisTileEntity = Instantiate(wallCornerPrefab, nextTile.transform);
                            thisTileEntity.name = "CornerWall";
                            WallEntity CornerWall = thisTileEntity.GetComponent<WallEntity>();
                            listOfWalls.Add(CornerWall);
                            CornerWall.WallDirection(int.Parse(numberPlusAddition[1]), numberPlusAddition[2]);
                            break;

                        case 20: //create guard
                            thisTileEntity = Instantiate(guardPrefab, nextTile.transform);
                            thisTileEntity.name = "Guard";
                            GuardEntity theGuard = thisTileEntity.GetComponent<GuardEntity>();
                            theGuard.movementLeft = theGuard.maxMovement;
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
                            catch (IndexOutOfRangeException) { continue; }
                            break;

                        case 21: //create static guard
                            thisTileEntity = Instantiate(staticGuardPrefab, nextTile.transform);
                            thisTileEntity.name = "Guard";
                            GuardEntity theStaticGuard = thisTileEntity.GetComponent<GuardEntity>();
                            theStaticGuard.movementLeft = theStaticGuard.maxMovement;
                            theStaticGuard.direction = StringToDirection(numberPlusAddition[1]);
                            listOfGuards.Add(theStaticGuard);

                            try
                            {
                                string[] patrolList = numberPlusAddition[2].Split('|');
                                foreach (string patrol in patrolList)
                                {
                                    string[] points = patrol.Split(",");
                                    int.TryParse(points[0], out int curX);
                                    int.TryParse(points[1], out int curY);
                                    theStaticGuard.PatrolPoints.Add(new Vector2Int(curX, curY));
                                }
                            }
                            catch (IndexOutOfRangeException) { continue; }
                            break;
                    }

                    try
                    {
                        StartCoroutine(thisTileEntity.MoveTile(nextTile));
                    }
                    catch (NullReferenceException)
                    {
                        continue;
                    }
                }
            }
        }

        for (int i = 0; i < listOfTiles.GetLength(0); i++) //then each tile finds adjacent tiles
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

        foreach (GuardEntity curGuard in listOfGuards)
        {
            curGuard.CalculateTiles();
            curGuard.CheckForPlayer();
        }
    }

#endregion

#region Helper Methods

    /// <summary>
    /// convert a string to a direction
    /// </summary>
    /// <param name="direction">u, d, l, r</param>
    /// <returns>vector2int of the direction</returns>
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

    /// <summary>
    /// a tile finds all tiles adjacent to it
    /// </summary>
    /// <param name="tile">the tile to find adjacent tiles to</param>
    void FindAdjacent(TileData tile)
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

    /// <summary>
    /// find a tile based off a vector2
    /// </summary>
    /// <param name="vector">the coordinates of the tile you want to find</param>
    /// <returns>the tile if it exists</returns>
    public TileData FindTile(Vector2 vector)
    {
        try { return listOfTiles[(int)vector.x, (int)vector.y]; }
        catch (IndexOutOfRangeException) { return null; }
    }

    /// <summary>
    /// find a tile based off a vector2int
    /// </summary>
    /// <param name="vector">the coordinates of the tile you want to find</param>
    /// <returns>the tile if it exists</returns>
    public TileData FindTile(Vector2Int vector)
    {
        return FindTile(new Vector2(vector.x, vector.y));
    }

    /// <summary>
    /// make all tiles unusable
    /// </summary>
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
                    listOfTiles[i, j].CardSelectable = false;
                    listOfTiles[i, j].directionIndicator.enabled = false;
                    listOfTiles[i, j].indicatorArrow = false;
                }
                catch (NullReferenceException)
                {
                    continue;
                }
            }
        }
    }

    /// <summary>
    /// make all cards unusable
    /// </summary>
    public void DisableAllCards()
    {
        foreach (Card card in SaveManager.instance.allCards)
        {
            card.DisableCard();
        }
    }

    /// <summary>
    /// make all players clickable
    /// </summary>
    public void EnablePlayers()
    {
        foreach (PlayerEntity player in listOfPlayers)
        {
            player.currentTile.clickable = true;
        }
    }

    /// <summary>
    /// only allow 1 specific player to be clickable
    /// </summary>
    /// <param name="playerToForce"></param>
    public void ForcePlayer(PlayerEntity playerToForce)
    {
        DisableAllTiles();
        listOfPlayers.Find(x => x == playerToForce).currentTile.clickable = true;
    }

    public EnvironmentalEntity CreateEnvironmental()
    {
        EnvironmentalEntity newEnviro = Instantiate(environmentPrefab);
        return newEnviro;
    }

    #endregion

}
