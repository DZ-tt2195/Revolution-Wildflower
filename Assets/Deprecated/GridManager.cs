using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridManager : MonoBehaviour
{
    /*
    public static GridManager instance;
    [Header("Grid Settings")]
    //controls the grid size with x,y length (1-10 instead of 0-10)
    [SerializeField] public Vector2Int GridSize = new Vector2Int(10, 10);
    //grid of all floor tiles for quick reference
    public FloorTile[,] _Grid;
    //controls the spacing between each tile
    [SerializeField] public float tileSize = 2;
    //controls how high up the floor tiles are set
    [SerializeField] float baseTileLayer = 0;

    [Space(5)]

    [Header("Summonable Objects")]
    //these are generic for hard coading levels, tile is used for the flooring
    [SerializeField] GameObject genericTile;
    [SerializeField] GameObject genericWall;
    [SerializeField] GameObject genericGuard;
    [SerializeField] GameObject Player;
    public ObjectManager Player1;
    Slider movementBar;
    TMP_Text movementText;

    [Space(5)]

    [Header("Selection Settings")]
    //checks currently selected tile
    public Vector2Int selectTile = new Vector2Int(0, 0);
    //quick refrence for the object within select tile
    public ObjectManager selectObject;


    [Header("Turn Management")]
    //in turn, 1 = player, 2 = Enemy, not a bool incase we need more states
    public int Turn = 1;
    //checks to make sure enemies arn't still taking their turn before swapping back to player control
    public float enemiesActive = 0;
    Button endRoundButton;

    private void Awake()
    {
        instance = this;
        endRoundButton = GameObject.Find("End Round Button").GetComponent<Button>();
        endRoundButton.onClick.AddListener(endRound);
        movementBar = GameObject.Find("Movement Slider").GetComponent<Slider>();
        movementText = movementBar.transform.GetChild(2).GetComponent<TMP_Text>();
    }

    void Start()
    {
        endRoundButton.gameObject.SetActive(true);
        _Grid = new FloorTile[GridSize.x + 1, GridSize.y + 1];

        //generates the base grid tiles, adds them to an array that refrences the grid with an x/y position, technically 0,y and x,0 are valid, but nothing is held in them, 0,0 is used as "unselected space"
        for (int i = 1; i <= GridSize.x; i++)
        {
            for (int j = 1; j <= GridSize.y; j++)
            {
                GameObject curTile = Instantiate(genericTile, new Vector3(i * tileSize, baseTileLayer, j * -tileSize), Quaternion.identity);
                FloorTile curFloorScript = curTile.GetComponent<FloorTile>();
                curFloorScript.gridPosition = new Vector2Int(i, j);
                curFloorScript.manager = this;
                _Grid[i, j] = curFloorScript;
            }
        }


        //Generates objects onto the grid using i,j positions, assigns each object a manager and parents them to a tile
        for (int i = 1; i <= GridSize.x; i++)
        {
            for (int j = 1; j <= GridSize.y; j++)
            {
                //honestly these generation scripts are all the same and could be made into a single void

                //generates walls
                if (i == 6 && j <= 5)
                {
                    GameObject curObj = Instantiate(genericWall, new Vector3(_Grid[i, j].gridPosition.x * tileSize, baseTileLayer + tileSize, _Grid[i, j].gridPosition.y * -tileSize), Quaternion.identity);
                    ObjectManager curObjManager = curObj.GetComponent<ObjectManager>();
                    curObj.transform.parent = _Grid[i, j].transform;
                    _Grid[i, j].AttachedObject = curObjManager;
                    curObjManager.CurrentGrid = _Grid[i, j].gridPosition;
                    curObjManager.manager = this;
                }

                //generates guards
                if (i == 9 && j == 3)
                {
                    GameObject curObj = Instantiate(genericGuard, new Vector3(_Grid[i, j].gridPosition.x * tileSize, baseTileLayer + tileSize, _Grid[i, j].gridPosition.y * -tileSize), Quaternion.identity);
                    ObjectManager curObjManager = curObj.GetComponent<ObjectManager>();
                    curObj.transform.parent = _Grid[i, j].transform;
                    _Grid[i, j].AttachedObject = curObjManager;
                    curObjManager.CurrentGrid = _Grid[i, j].gridPosition;
                    curObjManager.manager = this;
                }

                if ((i == 8 && j == 8) || (i == 3 && j == 7))
                {
                    GameObject curObj = Instantiate(genericGuard, new Vector3(_Grid[i, j].gridPosition.x * tileSize, baseTileLayer + tileSize, _Grid[i, j].gridPosition.y * -tileSize), Quaternion.identity);
                    ObjectManager curObjManager = curObj.GetComponent<ObjectManager>();
                    curObj.transform.parent = _Grid[i, j].transform;
                    _Grid[i, j].AttachedObject = curObjManager;
                    curObjManager.CurrentGrid = _Grid[i, j].gridPosition;
                    curObjManager.manager = this;
                    curObjManager.direction = new Vector2Int(-1, 0);
                }

                //Generates the Player
                if (i == 1 && j == 1)
                {
                    GameObject curObj = Instantiate(Player, new Vector3(_Grid[i, j].gridPosition.x * tileSize, baseTileLayer + tileSize, _Grid[i, j].gridPosition.y * -tileSize), Quaternion.identity);
                    ObjectManager curObjManager = curObj.GetComponent<ObjectManager>();
                    curObj.transform.parent = _Grid[i, j].transform;
                    _Grid[i, j].AttachedObject = curObjManager;
                    curObjManager.CurrentGrid = _Grid[i, j].gridPosition;
                    curObjManager.manager = this;
                    curObjManager.movementPoints = curObjManager.movementSpeed;
                    Player1 = curObjManager;
                }
            }
        }

    }

    private void Update()
    {
        movementText.text = $"Movement: {Player1.movementPoints}";
        movementBar.value = Player1.movementPoints;

        //checks to make sure all the enemies are done before switching back to player control
        if (Turn == 2)
        {
            if (enemiesActive == 0)
            {
                Turn = 1;
                endRoundButton.gameObject.SetActive(true);
                StartCoroutine(TurnManager.instance.CanPlayCard());
            }
        }
    }

    public void endTurn()
    {
        endRoundButton.gameObject.SetActive(false);
        ChoiceManager.instance.DisableCards();

        //sets turn to the enemies, and counts through the grid activating all enemies simultaniously
        Turn = 2;
        for (int i = 1; i <= GridSize.x; i++)
        {
            for (int j = 1; j <= GridSize.y; j++)
            {
                if (_Grid[i, j].AttachedObject != null)
                {
                    if (_Grid[i, j].AttachedObject.gameObject.tag == "Enemy")
                    {
                        enemiesActive++;
                        _Grid[i, j].AttachedObject.enemyEndTurn();
                    }
                    if (_Grid[i, j].AttachedObject.gameObject.tag == "Player")
                    {
                        _Grid[i, j].AttachedObject.endPlayerTurn();
                    }
                }
            }
        }
    }

    public void endRound()
    {
        TurnManager.instance.ChangeEnergy(3);
        TurnManager.instance.DrawCards(5 - TurnManager.instance.listOfHand.Count);
        Player1.movementPoints = Player1.movementSpeed;
        endTurn();
    }
    */
}
