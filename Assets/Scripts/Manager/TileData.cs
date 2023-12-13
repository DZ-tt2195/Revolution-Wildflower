using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
//using UnityEditor.Experimental.GraphView;

public class TileData : MonoBehaviour
{
    [Foldout("Tile information", true)]
        [Tooltip("All adjacent tiles")] [ReadOnly] public List<TileData> adjacentTiles;
        [Tooltip("Position in the grid")] [ReadOnly] public Vector2Int gridPosition;
        [Tooltip("The entity on this tile")] [ReadOnly] public Entity myEntity;

    [Foldout("Mouse", true)]
        [Tooltip("timer that controls how long until a tool tip appears on hover")] float timeTillToolTip = 0.5f;
        [Tooltip("timer that controls how long until a tool tip appears on hover")] float toolTipHoverTimer = 0;
        [Tooltip("Defines whether you can choose this tile")][ReadOnly] public bool choosable = false;
        [Tooltip("Defines whether you can click this tile")][ReadOnly] public bool clickable = false;
        [Tooltip("Defines whether you can move onto this tile")][ReadOnly] public bool moveable = false;
        [Tooltip("If your mouse is over this")] private bool moused = false;

    [Foldout("Colors", true)]
        [Tooltip("Tile's sprite renderer")] SpriteRenderer myRenderer;
        [Tooltip("Tile's materal")] [SerializeField] Renderer renderer3d;
        [Tooltip("Glowing border's sprite renderer")] SpriteRenderer border;
        [Tooltip("color used for unselected moused over tiles")][SerializeField] Color mouseOverColor = new Color(0.9f,0.9f,0.9f,1);
        [Tooltip("color used for selected tiles")][SerializeField] Color SelectedColor;
        [Tooltip("color used for unselected moused over tiles")][SerializeField] Color MoveableColor = new Color(0.9f, 0.9f, 0.9f, 1);
        [Tooltip("color used for unselected moused over tiles")] [SerializeField] Color AlertColor = new Color(0.9f, 0.7f, 0.1f, 1);
        [Tooltip("Time for noise indecator to show")] [SerializeField] float AlertDelay = 0.2f;
        [Tooltip("Base delay noise indecator")] [SerializeField] float BaseAlertDelay = 0.2f;
        [Tooltip("Variable indicating when tile should highlight for noise")][ReadOnly] bool noiseThrough = false;

    private void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        myRenderer.sortingOrder = 0;
        border = this.transform.GetChild(0).GetComponent<SpriteRenderer>();
        border.color = new Color(1f, 1f, 1f, 0);
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
            border.color = new Color(SelectedColor.r, SelectedColor.g, SelectedColor.b, NewManager.instance.opacity);
        }
        else if (moveable)
        {
            border.color = new Color(MoveableColor.r, MoveableColor.g, MoveableColor.b, NewManager.instance.opacity);
        }
        else if (moused)
        {
            border.color = mouseOverColor;
        }
        else
        {
            border.color = new Color(1, 1, 1, 0);
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

    private void OnMouseEnter()
    {
        moused = true;
    }

    private void OnMouseExit() 
    {
        moused = false;
    }

    private void OnMouseOver()
    {
        if (clickable && Input.GetKeyDown(KeyCode.Mouse0) && Input.mousePosition.y > 335)
        {
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
                        StartCoroutine(NewManager.instance.ChooseMovePlayer(player));
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
            }
        }
    }

    public void SurveillanceState(bool underSurveillance)
    {
        renderer3d.material.color = (underSurveillance) ? Color.red : Color.gray;
    }

    private void Update()
    {
        if (!moused)
        {
            toolTipHoverTimer = 0;
        }
    }
}