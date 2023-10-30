using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FloorTile : MonoBehaviour
{
    /*
    public Vector2Int gridPosition;
    [SerializeField] float hoverDistance;
    [SerializeField] float climbSpeed = 0.9f;
    [SerializeField] float dropSpeed;
    private bool hover = false;
    private float baseHeight = 0;
    public GridManager manager;
    public ObjectManager AttachedObject;

    [SerializeField] Material defaultTexture;
    [SerializeField] Material HazardTexture;
    public bool underSurveillance = false;
    public ObjectManager patrollingGuard;

    private MeshRenderer currentMaterial;
    // Start is called before the first frame update

    private void Awake()
    {
        currentMaterial = GetComponent<MeshRenderer>();
    }

    private void OnMouseOver()
    {
        bool action = false;
        hover = true;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //selects this tile if it's not selected (unselects all tiles otherwise)
            if (manager.selectTile != gridPosition)
            {
                //checks if an object was selected and that it's the player's turn
                if (manager.selectObject != null && manager.Turn == 1)
                {
                    //checks if selected object was a player
                    if (manager.selectObject.gameObject.tag == "Player")
                    {
                        //print(AttachedObject);
                        //moves the player depending on their assigned movement speed, asigning them to this tile
                        if (MathF.Abs(gridPosition.x - manager.selectTile.x) + MathF.Abs(gridPosition.y - manager.selectTile.y) <= manager.selectObject.movementPoints && AttachedObject == null)
                        {
                            Vector2Int direction = new Vector2Int(gridPosition.x - manager.selectTile.x, gridPosition.y - manager.selectTile.y);
                            Vector2Int checkSpace = manager.selectTile;
                            bool blocked = false;
                            //print("New direction" + direction);

                            //loops through all the spaces you would move through to get to the new location
                            while (direction != new Vector2(0, 0))
                            {
                                //print("current checkspace " + checkSpace);
                                if (direction.x != 0)
                                {
                                    if (direction.x > 0)
                                    {
                                        checkSpace.x += 1;
                                        if (manager._Grid[checkSpace.x, checkSpace.y].AttachedObject != null)
                                        {
                                            blocked = true;
                                        }
                                    }
                                    if (direction.x < 0)
                                    {
                                        checkSpace.x -= 1;
                                        if (manager._Grid[checkSpace.x, checkSpace.y].AttachedObject != null)
                                        {
                                            blocked = true;
                                        }
                                    }
                                    direction.x -= (int)Mathf.Sign(direction.x);
                                    //print("new direction " + direction);
                                }

                                if (direction.y != 0)
                                {
                                    if (direction.y > 0)
                                    {
                                        checkSpace.y += 1;
                                        if (manager._Grid[checkSpace.x, checkSpace.y].AttachedObject != null)
                                        {
                                            blocked = true;
                                        }
                                    }
                                    if (direction.y < 0)
                                    {
                                        checkSpace.y -= 1;
                                        if (manager._Grid[checkSpace.x, checkSpace.y].AttachedObject != null)
                                        {
                                            blocked = true;
                                        }
                                    }
                                    direction.y -= (int)Mathf.Sign(direction.y);
                                    //print("new direction " + direction);
                                }


                            }
                            if (!blocked)
                            {
                                AttachedObject = manager.selectObject;
                                manager._Grid[manager.selectTile.x, manager.selectTile.y].AttachedObject = null;
                                AttachedObject.transform.parent = transform;
                                AttachedObject.transform.position = new Vector3(gridPosition.x * manager.tileSize, transform.position.y + manager.tileSize, gridPosition.y * -manager.tileSize);
                                action = true;
                                AttachedObject.movementPoints -= MathF.Abs(gridPosition.x - manager.selectTile.x) + MathF.Abs(gridPosition.y - manager.selectTile.y);
                                manager.endTurn();

                            }

                        }
                    }
                }
                if (!action)
                {
                    manager.selectTile = gridPosition;
                }
                else
                {
                    manager.selectTile = new Vector2Int(0, 0);
                }

            }
            else
            {
                manager.selectTile = new Vector2Int(0, 0);
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        //if this tile is selected, it holds hover and changes the selected object in the manager
        if (manager.selectTile == gridPosition)
        {
            hover = true;
            manager.selectObject = AttachedObject;
        }

        if (!hover)
        { 
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, baseHeight, transform.position.z), dropSpeed);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, hoverDistance, transform.position.z), climbSpeed);
        }

        hover = false;

        if (underSurveillance)
        {
            currentMaterial.material = HazardTexture;
            if (AttachedObject != null)
            {
                print("TARGET SIGHTED!!");
                if (AttachedObject.tag == "Player" && AttachedObject.hidden == 0)
                {
                    patrollingGuard.patrol = false;
                    patrollingGuard.target = AttachedObject;
                }
            }
        }
        else
        {
            currentMaterial.material = defaultTexture;
        }
        underSurveillance = false;
    }
    */
}
