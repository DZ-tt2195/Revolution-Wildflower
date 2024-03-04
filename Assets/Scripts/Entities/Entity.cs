using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using MyBox;
using System;

public class Entity : MonoBehaviour
{
    [Foldout("Base Entity", true)]
        [Tooltip("Store this entity's position")] [ReadOnly] public TileData currentTile;
        [Tooltip("The visual offset for this entity relative to its tile")] public Vector3 tileOffset;
        [ReadOnly] public SpriteRenderer spriteRenderer;
        [ReadOnly] public LineRenderer lineRenderer;
        [Tooltip("Cost of moving through item, default 999 (intraversable)")] [SerializeField] public int MoveCost = 999;
        [Tooltip("Cost of sound moving through item, default to 1")] [SerializeField] public int SoundCost = 1;
        [Tooltip("whether a guard can see through this object")] [SerializeField] public bool Occlusion = true;
        [Tooltip("Where this moves and looks")] [ReadOnly] public Vector2Int direction;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = 200;
        lineRenderer = GetComponent<LineRenderer>();
    }

    public virtual string HoverBoxText()
    {
        return "";
    }

    public virtual IEnumerator MoveTile(TileData newTile)
    {
        newTile = NewManager.instance.listOfTiles[newTile.gridPosition.x, newTile.gridPosition.y];
        if (currentTile != null)
            currentTile.myEntity = null;

        newTile.myEntity = this;
        this.currentTile = newTile;
        this.transform.SetParent(newTile.transform);
        this.transform.localScale = new Vector3(1, 1, 1);
        this.transform.localPosition = tileOffset;

        foreach (TileModifier modifier in newTile.listOfModifiers)
        {
            yield return modifier.ResolveList(this);
        }
        newTile.listOfModifiers.RemoveAll(item => item == null);
        foreach (GuardEntity guard in NewManager.instance.listOfGuards)
            guard.CheckForPlayer();
        CalculateTiles();
    }

    public virtual void CalculateTiles()
    {
        
    }
}
