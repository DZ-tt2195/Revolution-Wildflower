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
                    string[] textPlusAddition = newGrid[i, j].Split("|");

                    switch (textPlusAddition[0])
                    {
                        case "Floor":
                            if (textPlusAddition.Length>=2)
                            {
                                nextTile.transform.Find(textPlusAddition[1]).gameObject.SetActive(true);
                                if (textPlusAddition[2] != "true")
                                {
                                    nextTile.transform.GetChild(0).transform.Find("Pointer").gameObject.SetActive(false);
                                    nextTile.enabled = false;
                                }
                            }
                            break;

                        case "Player":
                            thisTileEntity = Instantiate(playerPrefab, nextTile.transform);
                            PlayerEntity player = thisTileEntity.GetComponent<PlayerEntity>();
                            player.SetEnergy(player.maxEnergy);
                            player.SetMovement(player.maxMovement);
                            listOfPlayers.Add(player);
                            player.PlayerSetup(textPlusAddition[1]);
                            break;

                        case "Exit":
                            nextTile.myType = TileData.TileType.Exit;
                            break;

                        case "All Exit":
                            nextTile.myType = TileData.TileType.AllExit;
                            break;

                        case "Objective":
                            thisTileEntity = Instantiate(objectivePrefab, nextTile.transform);
                            thisTileEntity.name = textPlusAddition[1];
                            ObjectiveEntity defaultObjective = thisTileEntity.GetComponent<ObjectiveEntity>();
                            defaultObjective.objective = textPlusAddition[2];

                            if (textPlusAddition.Length > 3)
                            {
                                Debug.Log(textPlusAddition.Length);
                                if (textPlusAddition[3] != "null")
                                {
                                    defaultObjective.instructionsWhenCompleted = textPlusAddition[3].ToUpper().Trim();
                                }
                            }

                            if (textPlusAddition.Length > 4)
                            {
                                if (textPlusAddition[4] != "null")
                                {
                                    defaultObjective.textAssetFile = textPlusAddition[4];
                                }
                            }

                            listOfObjectives.Add(defaultObjective);
                            break;

                        case "Toggle":
                            thisTileEntity = Instantiate(togglePrefab, nextTile.transform);
                            thisTileEntity.name = textPlusAddition[2];
                            ToggleEntity defaultToggle = thisTileEntity.GetComponent<ToggleEntity>();
                            defaultToggle.interactCondition = textPlusAddition[1].ToUpper().Trim();
                            defaultToggle.interactInstructions = textPlusAddition[2].ToUpper().Trim();
                            defaultToggle.toggledOn = (textPlusAddition[3] != "true");

                            try
                            {
                                string[] pointList = textPlusAddition[4].Split('-');
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

                        case "Moving Guard":
                            thisTileEntity = Instantiate(guardPrefab, nextTile.transform);
                            thisTileEntity.name = "Guard";
                            GuardEntity theGuard = thisTileEntity.GetComponent<GuardEntity>();
                            theGuard.movementLeft = theGuard.maxMovement;
                            theGuard.direction = StringToDirection(textPlusAddition[1]);
                            listOfGuards.Add(theGuard);

                            try
                            {
                                string[] patrolList = textPlusAddition[2].Split('-');
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

                        case "Static Guard":
                            thisTileEntity = Instantiate(staticGuardPrefab, nextTile.transform);
                            thisTileEntity.name = "Guard";
                            GuardEntity theStaticGuard = thisTileEntity.GetComponent<GuardEntity>();
                            theStaticGuard.movementLeft = theStaticGuard.maxMovement;
                            theStaticGuard.direction = StringToDirection(textPlusAddition[1]);
                            listOfGuards.Add(theStaticGuard);

                            try
                            {
                                string[] patrolList = textPlusAddition[2].Split('-');
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

                        case "Wall":
                            thisTileEntity = Instantiate(wallPrefab, nextTile.transform);
                            thisTileEntity.name = "Wall";
                            WallEntity wall = thisTileEntity.GetComponent<WallEntity>();
                            listOfWalls.Add(wall);
                            wall.WallSetup(int.Parse(textPlusAddition[1]), textPlusAddition[2]);
                            thisTileEntity.transform.Find(textPlusAddition[3]).gameObject.SetActive(true);
                            break;
                    }

                    try
                    {
                        StartCoroutine(thisTileEntity.MoveTile(nextTile));
                        if (textPlusAddition[0] == "Player")
                            PhaseManager.instance.FocusOnTile(nextTile, false);
                    }
                    catch (NullReferenceException)
                    {
                        continue;
                    }
                }
            }
        }

        LevelUIManager.instance.UpdateStats(null);

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
        Debug.LogError("disabling player cards");
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
