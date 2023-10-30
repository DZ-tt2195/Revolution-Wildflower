using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    /*
    [Header("General Variables")]
    //movement speed is the max distance an object can move on its turn
    [SerializeField] public int movementSpeed = 3;
    //movement points are the remaining points an object has to spend on moving
    public float movementPoints = 0;

    public Vector2Int CurrentGrid;
    public GridManager manager;

    //direction determins the way they'll patrol and the tiles that are highlighted
    //south east = (-1,0),south west = (0,-1),north east = (1,0), North west = (0,1) 
    [SerializeField] public Vector2Int direction = new Vector2Int(0, -1);

    private MeshRenderer Renderer;
    [Space(5)]


    [Header("Guard Variables")]
    [SerializeField] float movePauseTime = 0.5f;

    public bool patrol = true;
    public ObjectManager target;
    [SerializeField] int DetectionRangePatrol = 3;

    public int stunned = 0;

    [SerializeField] private int guardAmmoMax = 1;
    private int Ammo = 0;

    private LineRenderer lineRenderer;
    [Space(5)]


    [Header("Player Variables")]
    public int hidden = 0;
    [SerializeField] Material DefaultPlayerMaterial;
    [SerializeField] Material HiddenPlayerMaterial;



    private void Awake()
    {
        if (tag == "Enemy")
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        Renderer = GetComponent<MeshRenderer>();
    }

    public void endPlayerTurn()
    {

        if (stunned != 0)
        {
            stunned--;
        }
        if (hidden != 0)
        {
            hidden--;
        }
    }

    // ENEMY VOIDS

    //called at end of a round or turn, only in effect for enemy tagged objects
    public void enemyEndTurn()
    {
        if (stunned == 0)
        {
            movementPoints = movementSpeed;
            Ammo = guardAmmoMax;
            if (patrol)
            {
                StartCoroutine(guardPatrol(movePauseTime));
            }
            else
            {
                StartCoroutine(guardAttack(movePauseTime));
            }
        }
        else
        {
            stunned--;
            manager.enemiesActive--;
        }

    }

    
    IEnumerator guardAttack(float pauseTimer)
    {
        float Timer = 0;
        while (Timer < pauseTimer / 2)
        {
            Timer += Time.deltaTime;
            yield return null;
        }
        RaycastHit hit;
        Vector3 shotDirection = target.transform.position - transform.position;
        if (Physics.Raycast(transform.position, shotDirection, out hit, Mathf.Infinity, 1 << 2))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                TurnManager.instance.ChangeHealth(-1);
            }
        }
        Ammo--;
        if (Ammo > 0)
        {
            StartCoroutine(guardAttack(movePauseTime));
        }
        else
        {
            manager.enemiesActive--;
        }
        
    }
    IEnumerator guardPatrol(float pauseTimer)
    {
        float Timer = 0;
        while (Timer < pauseTimer)
        {
            Timer += Time.deltaTime;
            yield return null;
        }


        bool validSpace = true;
        bool trapped = false;
        //checking to see that the tile it wants to move onto is A) within the map and B) not a wall
        if (CurrentGrid.x + direction.x > 0 && CurrentGrid.x + direction.x <= manager.GridSize.x &&
            CurrentGrid.y + direction.y > 0 && CurrentGrid.y + direction.y <= manager.GridSize.y)
        {
            if (manager._Grid[CurrentGrid.x + direction.x, CurrentGrid.y + direction.y].AttachedObject != null)
            {
                validSpace = false;
            }
        }
        else
        {
            validSpace = false;
        }


        if (validSpace)
        {
            manager._Grid[CurrentGrid.x, CurrentGrid.y].AttachedObject = null;
            CurrentGrid = CurrentGrid + direction;


            manager._Grid[CurrentGrid.x, CurrentGrid.y].AttachedObject = this;
            transform.parent = manager._Grid[CurrentGrid.x, CurrentGrid.y].transform;
            transform.position = new Vector3(CurrentGrid.x * manager.tileSize, manager._Grid[CurrentGrid.x, CurrentGrid.y].transform.position.y + manager.tileSize, CurrentGrid.y * -manager.tileSize);

            movementPoints--;
            if (movementPoints > 0)
            {
                StartCoroutine(guardPatrol(movePauseTime));
            }
            else
            {
                manager.enemiesActive--;
            }
        }
        else //if not a valid space ahead
        {
            //flips the guard 180 degrees
            direction = -direction;

            //checks to make sure the guard isn't trapped
            if (CurrentGrid.x + direction.x > 0 && CurrentGrid.x + direction.x <= manager.GridSize.x &&
            CurrentGrid.y + direction.y > 0 && CurrentGrid.y + direction.y <= manager.GridSize.y)
            {
                if (manager._Grid[CurrentGrid.x + direction.x, CurrentGrid.y + direction.y].AttachedObject != null)
                {
                    trapped = true;
                }
            }
            else
            {
                trapped = true;
            }

            if (trapped)
            {
                movementPoints = 0;
                manager.enemiesActive--;
            }
            else
            {
                manager._Grid[CurrentGrid.x, CurrentGrid.y].AttachedObject = null;
                CurrentGrid = CurrentGrid + direction;


                manager._Grid[CurrentGrid.x, CurrentGrid.y].AttachedObject = this;
                transform.parent = manager._Grid[CurrentGrid.x, CurrentGrid.y].transform;
                transform.position = new Vector3(CurrentGrid.x * manager.tileSize, manager._Grid[CurrentGrid.x, CurrentGrid.y].transform.position.y + manager.tileSize, CurrentGrid.y * -manager.tileSize);

                movementPoints--;
                if (movementPoints > 0)
                {
                    StartCoroutine(guardPatrol(movePauseTime));
                }
                else
                {
                    manager.enemiesActive--;
                }
            }
        }

    }

    private void Update()
    {
        if (gameObject.tag == "Enemy")
        {
            //if guard has detected someone and they're not stunned it will draw a line and prepare to shoot
            if (!patrol)
            {
                if (stunned == 0)
                {
                    lineRenderer.enabled = true;
                    lineRenderer.SetPositions(new Vector3[] { transform.position, target.transform.position });
                    RaycastHit hit;
                    Vector3 shotDirection = target.transform.position - transform.position;
                    if (Physics.Raycast(transform.position, shotDirection, out hit, Mathf.Infinity, 1 << 2))
                    {
                        lineRenderer.SetPositions(new Vector3[] { transform.position, hit.collider.transform.position });
                        UnityEngine.Debug.DrawRay(transform.position, shotDirection * hit.distance, Color.yellow);
                        if (hit.collider.gameObject.tag != "Player")
                        {
                            patrol = true;
                        }
                    }
                }
                else
                {
                    lineRenderer.enabled = false;
                }
                if (target.hidden > 0)
                {
                    patrol = true;
                }

            }
            else
            {
                lineRenderer.enabled = false;
            }




            //detects the tiles around you
            if (stunned == 0)
            {
                //creates a vector 2 to check to the left
                Vector2Int Side = Vector2Int.RoundToInt(Vector3.Cross((Vector2)direction, Vector3.forward));

                //checks all the sides of the guard
                List<FloorTile> tilesToCheck = new List<FloorTile>();

                if (CurrentGrid.x + Side.x <= manager.GridSize.x && CurrentGrid.x + Side.x > 0 &&
                    CurrentGrid.y + Side.y <= manager.GridSize.y && CurrentGrid.y + Side.y > 0)
                {
                    tilesToCheck.Add(manager._Grid[CurrentGrid.x + Side.x, CurrentGrid.y + Side.y]);
                }

                if (CurrentGrid.x + Side.x + direction.x <= manager.GridSize.x && CurrentGrid.x + Side.x + direction.x > 0 &&
                    CurrentGrid.y + Side.y + direction.y <= manager.GridSize.y && CurrentGrid.y + Side.y + direction.y > 0)
                {
                    tilesToCheck.Add(manager._Grid[CurrentGrid.x + Side.x + direction.x, CurrentGrid.y + Side.y + direction.y]);
                }

                if (CurrentGrid.x - Side.x <= manager.GridSize.x && CurrentGrid.x - Side.x > 0 &&
                    CurrentGrid.y - Side.y <= manager.GridSize.y && CurrentGrid.y - Side.y > 0)
                {
                    tilesToCheck.Add(manager._Grid[CurrentGrid.x - Side.x, CurrentGrid.y - Side.y]);
                }

                if (CurrentGrid.x - Side.x + direction.x <= manager.GridSize.x && CurrentGrid.x - Side.x + direction.x > 0 &&
                    CurrentGrid.y - Side.y + direction.y <= manager.GridSize.y && CurrentGrid.y - Side.y + direction.y > 0)
                {
                    tilesToCheck.Add(manager._Grid[CurrentGrid.x - Side.x + direction.x, CurrentGrid.y - Side.y + direction.y]);
                }


                foreach (FloorTile Tile in tilesToCheck)
                {
                    if (Tile.AttachedObject != null)
                    {
                        if (Tile.AttachedObject.tag != "Environmental")
                        {
                            Tile.underSurveillance = true;
                            Tile.patrollingGuard = this;
                        }
                    }
                    else
                    {
                        Tile.underSurveillance = true;
                        Tile.patrollingGuard = this;
                    }
                }

                //checks all spaces in front of the guard in range
                for (int i = 1; i <= DetectionRangePatrol; i++)
                {
                    if (CurrentGrid.x + (direction.x * i) <= manager.GridSize.x && CurrentGrid.x + (direction.x * i) > 0 &&
                        CurrentGrid.y + (direction.y * i) <= manager.GridSize.y && CurrentGrid.y + (direction.y * i) > 0)
                    {
                        FloorTile targetGrid = manager._Grid[CurrentGrid.x + (direction.x * i), CurrentGrid.y + (direction.y * i)];
                        if (targetGrid.AttachedObject != null)
                        {
                            if (targetGrid.AttachedObject.tag != "Environmental")
                            {
                                targetGrid.underSurveillance = true;
                                targetGrid.patrollingGuard = this;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            targetGrid.underSurveillance = true;
                            targetGrid.patrollingGuard = this;
                        }
                    }

                }
            }
        }

        //                  PLAYER UPDATE
        if (tag == "Player")
        {
            if (hidden > 0)
            {
                Renderer.material = HiddenPlayerMaterial;
            }
            else
            {
                Renderer.material = DefaultPlayerMaterial;
            }
        }


    }
    */
}
