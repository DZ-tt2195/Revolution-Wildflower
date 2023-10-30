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
        [Tooltip("Defines whether you can move onto this tile")][ReadOnly] public bool moveable = false;
        [Tooltip("If your mouse is over this")] private bool moused = false;

    [Foldout("Colors", true)]
        [Tooltip("Tile's sprite renderer")] SpriteRenderer myRenderer;
        [Tooltip("Glowing border's sprite renderer")] SpriteRenderer border;
        [Tooltip("color used for unselected moused over tiles")][SerializeField] Color mouseOverColor = new Color(0.9f,0.9f,0.9f,1);
        [Tooltip("color used for selected tiles")][SerializeField] Color SelectedColor;
        [Tooltip("color used for unselected moused over tiles")][SerializeField] Color MoveableColor = new Color(0.9f, 0.9f, 0.9f, 1);

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
        if (NewManager.instance.selectedTile == this)
        {
            border.color = new Color(SelectedColor.r, SelectedColor.g, SelectedColor.b, ChoiceManager.instance.opacity);
        }
        else if (moveable)
        {
            border.color = new Color(MoveableColor.r, MoveableColor.g, MoveableColor.b, ChoiceManager.instance.opacity);
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
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (moveable)
                ChoiceManager.instance.ReceiveChoice(this);

            NewManager.instance.selectedTile = this;
            if (myEntity != null)
            {
                if (myEntity.CompareTag("Player"))
                {
                    StartCoroutine(NewManager.instance.ChooseMovePlayer(this));
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
        myRenderer.color = (underSurveillance) ? Color.red : Color.gray;
    }

    private void Update()
    {
        if (!moused)
        {
            toolTipHoverTimer = 0;
        }
    }
}