using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using UnityEngine.EventSystems;

public class TileData : MonoBehaviour
{
    public enum TileType { Regular, Exit, AllExit };
    [Foldout("Tile information", true)]
        [Tooltip("Attached arrow")] public SpriteRenderer directionIndicator;
        [Tooltip("arrow sprites")] public List<Sprite> arrowSprites = new();
        [Tooltip("All adjacent tiles")] [ReadOnly] public List<TileData> adjacentTiles;
        [Tooltip("Position in the grid")] [ReadOnly] public Vector2Int gridPosition;
        [Tooltip("The entity on this tile")] [ReadOnly] public Entity myEntity;
        [Tooltip("What kind of tile this is")] [ReadOnly] public TileType myType;
        [Tooltip("Modifiers on this tile")][ReadOnly] public List<TileModifier> listOfModifiers = new();

    [Foldout("Tile conditions", true)]
        [Tooltip("Defines whether you can choose this tile")][ReadOnly] public bool choosable = false;
        [Tooltip("Defines whether you can click this tile")][ReadOnly] public bool clickable = false;
        [Tooltip("Defines whether you can move onto this tile")][ReadOnly] public bool moveable = false;
        [Tooltip("Defines whether you can move onto this tile")][ReadOnly] public bool currentGuardTarget = false;
        [Tooltip("Defines whether you can select this tile for a card action")][ReadOnly] public bool CardSelectable = false;
        [Tooltip("If your mouse is over this")] private bool moused = false;

    [Foldout("Mouse", true)]
        [Tooltip("Layer mask that mouse raycasts ignore")] [SerializeField] LayerMask mask;
        [Tooltip("timer that controls how long until a tool tip appears on hover")] float timeTillToolTip = 0.5f;
        [Tooltip("timer that controls how long until a tool tip appears on hover")] float toolTipHoverTimer = 0;

    [Foldout("Colors", true)]
        [Tooltip("Tile's sprite renderer")] SpriteRenderer myRenderer;
        [Tooltip("Tile's materal")] [SerializeField] Renderer renderer3d;
        [Tooltip("Glowing border's sprite renderer")] SpriteRenderer border;
        [Tooltip("color used for unselected moused over tiles")][SerializeField] Color mouseOverColor = new Color(0.9f,0.9f,0.9f,1);
        [Tooltip("color used for selected tiles")][SerializeField] Color SelectedColor = new Color(0.6f, 0.6f, 0.6f, 1);
        [Tooltip("color used for unselected moused over tiles (general)")][SerializeField] Color ClickableColor = new Color(0.9f, 0.9f, 0.9f, 1);
        [Tooltip("color used for unselected moused over tiles you can move onto")][SerializeField] Color MoveableColor = new Color(0.9f, 0.9f, 0.9f, 1);
        [Tooltip("color used for guard's distraction target tiles")] [SerializeField] Color AlertColor = new Color(0.9f, 0.7f, 0.1f, 1);
        [Tooltip("color used for unselected moused over tiles you can select for a card action")][SerializeField] Color CardSelectableColor = new Color(0.9f, 0.7f, 0.1f, 1);     
        [Tooltip("Time for noise indecator to show")] [SerializeField] float AlertDelay = 0.2f;
        [Tooltip("Base delay noise indecator")] [SerializeField] float BaseAlertDelay = 0.2f;
        [Tooltip("Variable indicating when tile should highlight for noise")][ReadOnly] bool noiseThrough = false;

    private void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        myRenderer.sortingOrder = 0;
        border = this.transform.GetChild(0).GetComponent<SpriteRenderer>();
        border.color = new Color(1f, 1f, 1f, 0);
        directionIndicator.enabled = false;
    }

    void FixedUpdate()
    {
        if (border == null)
        {
            Debug.Log("border is null");
        }
        else if (noiseThrough)
        {
            border.color = AlertColor;
        }
        else if (NewManager.instance.selectedTile == this)
        {
            border.color = SelectedColor;
            border.SetAlpha(NewManager.instance.opacity);
        }
        else if (currentGuardTarget)
        {
            border.color = AlertColor;
        }
        else if (moused)
        {
            border.color = mouseOverColor;
        }
        else if (moveable)
        {
            border.color = MoveableColor;
            border.SetAlpha(NewManager.instance.opacity);
        }
        else if (CardSelectable)
        {
            border.color = CardSelectableColor;
            border.SetAlpha(NewManager.instance.opacity);
        }
        else if (clickable)
        {
            border.color = ClickableColor;
            border.SetAlpha(NewManager.instance.opacity);
        }
        else if (myType != TileType.Regular)
        {
            border.color = new Color(0, 1, 0, 1);
        }
        else
        {
            border.SetAlpha(0);
        }
    }

    public IEnumerator NoiseFlash(int distance)
    {
        //print("noiseflash at distance " + distance);
        //print("Time to flash" + AlertDelay * distance);
        yield return NewManager.Wait(AlertDelay * distance);
        noiseThrough = true;
        yield return NewManager.Wait(BaseAlertDelay + AlertDelay * distance);
        noiseThrough = false;
    }

    private void MouseEnter()
    {
        moused = true;
        //generates a visible path the player is going to take to get to the space (clearing the last list and ignoring the first and last tile)
        if (moveable)
        {
            for (int i = 0; i < NewManager.instance.FullPath.Count; i++)
            {
                NewManager.instance.FullPath[i].directionIndicator.enabled = false;
            }

            NewManager.instance.CalculatePathfinding(NewManager.instance.lastSelectedPlayer.currentTile,this, NewManager.instance.lastSelectedPlayer.movementLeft,false,false);
            for (int i = 0; i < NewManager.instance.FullPath.Count; i++) 
            {
                NewManager.instance.FullPath[i].directionIndicator.enabled = true;
                /*
                if (i != NewManager.instance.FullPath.Count - 1)
                {
                    NewManager.instance.FullPath[i].directionIndicator.enabled = true;
                }
                */
            }
        }
    }

    private void MouseExit() 
    {
        moused = false;
        if (moveable)
        {
            for (int i = 0; i < NewManager.instance.FullPath.Count; i++)
            {
                //NewManager.instance.FullPath[i].directionIndicator.enabled = false;
            }
        }
    }

    private void MouseOver()
    {
        if (clickable && Input.GetKeyDown(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (moveable)
            {
                for (int i = 0; i < NewManager.instance.FullPath.Count; i++)
                {
                    NewManager.instance.FullPath[i].directionIndicator.enabled = false;
                }
            }
            NewManager.instance.selectedTile = this;
            if (choosable)
            {
                NewManager.instance.ReceiveChoice(this);
            }
            else if (myEntity != null)
            {
                if (myEntity.CompareTag("Player"))
                {
                    PlayerEntity player = myEntity.GetComponent<PlayerEntity>();
                    if (player.stunned == 0)
                    {
                        NewManager.instance.StopAllCoroutines();
                        NewManager.instance.ControlCharacter(player);
                    }
                }
            }
        }

        if (myEntity != null)
        {
            toolTipHoverTimer += Time.deltaTime;
            if (toolTipHoverTimer >= timeTillToolTip)
            {
                NewManager.instance.toolTip.EntityName.text = myEntity.name;
                NewManager.instance.toolTip.EntityInfo.text = myEntity.HoverBoxText();
                NewManager.instance.toolTip.gameObject.SetActive(true);
                NewManager.instance.toolTip.isActive = true;
                
                //if the tile entity is a guard, show their path to their current target
                if (myEntity.CompareTag("Enemy"))
                {
                    for (int i = 0; i < NewManager.instance.FullPath.Count; i++)
                    {
                        NewManager.instance.FullPath[i].directionIndicator.enabled = false;
                    }
                    GuardEntity currentGuard = myEntity.gameObject.GetComponent<GuardEntity>();
                    if (currentGuard.alertStatus == GuardEntity.Alert.Patrol)
                    {
                        NewManager.instance.CalculatePathfinding(this, NewManager.instance.listOfTiles[currentGuard.PatrolPoints[currentGuard.PatrolTarget].x, currentGuard.PatrolPoints[currentGuard.PatrolTarget].y],99,false,false);
                        for (int i = 0; i < NewManager.instance.FullPath.Count; i++)
                        {
                            NewManager.instance.FullPath[i].directionIndicator.enabled = true;
                        }
                    }
                    else if (currentGuard.alertStatus == GuardEntity.Alert.Attack)
                    {
                        NewManager.instance.CalculatePathfinding(this, currentGuard.CurrentTarget.currentTile, 99, false, false);
                        for (int i = 0; i < NewManager.instance.FullPath.Count; i++)
                        {
                            NewManager.instance.FullPath[i].directionIndicator.enabled = true;
                        }
                    }
                    else if (currentGuard.alertStatus == GuardEntity.Alert.Persue)
                    {
                        NewManager.instance.CalculatePathfinding(this, NewManager.instance.listOfTiles[currentGuard.DistractionPoints[0].x,currentGuard.DistractionPoints[0].y], 99, false, false);
                        for (int i = 0; i < NewManager.instance.FullPath.Count; i++)
                        {
                            NewManager.instance.FullPath[i].directionIndicator.enabled = true;
                        }
                    }
                }
            }
        }
    }

    public void SurveillanceState(bool underSurveillance)
    {
        //renderer3d.material.color = (underSurveillance) ? Color.red : Color.gray;
        renderer3d.material.SetColor("_palette_color", underSurveillance ? Color.red : new Color(0, 0.3686275f, 0.2352941f));
    }

    private void Update()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(mouseRay, out hit, Mathf.Infinity, mask) && hit.collider.gameObject == gameObject)
        {
            if (!moused) MouseEnter();
            MouseOver();
        }
        else if (moused) MouseExit();
        
        if (!moused)
        {
            if (toolTipHoverTimer > 0)
            {
                toolTipHoverTimer = 0;
                for (int i = 0; i < NewManager.instance.FullPath.Count; i++)
                {
                    NewManager.instance.FullPath[i].directionIndicator.enabled = false;
                }
            }
        }
    }
}