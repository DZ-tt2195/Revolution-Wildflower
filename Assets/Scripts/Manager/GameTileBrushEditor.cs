using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEngine;

[CustomEditor(typeof(GameTileBrush))]
public class GameTileBrushEditor : GameObjectBrushEditor
{
    private SerializedProperty TileGrid;

    private void OnEnable()
    {
        GameTileBrush brush = (GameTileBrush)target;   
        brush.TileGrid = FindObjectOfType<GameTileGrid>();
        brush.LayoutGrid = brush.TileGrid.gameObject.GetComponent<Grid>();
    }

    /// <summary>
    /// The targets that the GameObjectBrush can paint on
    /// </summary>
    public override GameObject[] validTargets
    {
        get
        {
            var currentStageHandle = StageUtility.GetCurrentStageHandle();
            var results = currentStageHandle.FindComponentsOfType<GridLayout>();
            var validGridLayouts = new List<GameObject>(results.Length + 1) { brush.hiddenGrid };
            foreach (var result in results)
            {
                if (result.gameObject.scene.isLoaded && result.gameObject.activeInHierarchy)
                    validGridLayouts.Add(result.gameObject);
            }
            return validGridLayouts.ToArray();
        }
    }
}
