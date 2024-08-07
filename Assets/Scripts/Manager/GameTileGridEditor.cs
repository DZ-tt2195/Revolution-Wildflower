using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(GameTileGrid))]
public class GameTileGridEditor : Editor
{
    private GameTileGrid _gameTileGrid;
    private Grid _grid;

    private SerializedProperty CellSize;
    private SerializedProperty GridSize;
    private SerializedProperty Grid;
    private void OnEnable()
    {
        CellSize = serializedObject.FindProperty("_cellSize");
        GridSize = serializedObject.FindProperty("_gridSize");
        Grid = serializedObject.FindProperty("_grid");
      
        _gameTileGrid = (GameTileGrid)target;
        if (!_gameTileGrid.gameObject.GetComponent<Grid>())
        {
            _grid = _gameTileGrid.gameObject.AddComponent<Grid>();
            _grid.cellSize = new Vector3(CellSize.intValue, CellSize.intValue, 0);
            _grid.cellSwizzle = GridLayout.CellSwizzle.XZY;
        }

        else
        {
            _grid = _gameTileGrid.gameObject.GetComponent<Grid>();
        }

        Grid.objectReferenceValue = _grid;
        _grid.hideFlags = HideFlags.None;
        //_grid.hideFlags = HideFlags.HideInInspector;
        serializedObject.ApplyModifiedProperties();
    }
    public override void OnInspectorGUI()
    {
        _gameTileGrid.transform.position = new Vector3(0, 0, 0);

        EditorGUI.BeginChangeCheck();
        GUILayout.BeginVertical($"Cell Size: {CellSize.intValue}", "window");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+"))
        {
            CellSize.intValue += 1;
            _grid.cellSize = new Vector3(_grid.cellSize.x + 1, _grid.cellSize.y + 1, 0);
            serializedObject.ApplyModifiedProperties();
        }

        if (GUILayout.Button("-"))
        {
            CellSize.intValue = CellSize.intValue > 0 ? CellSize.intValue - 1 : 0;
            _grid.cellSize = new Vector3(_grid.cellSize.x - 1, _grid.cellSize.y - 1, 0);
            serializedObject.ApplyModifiedProperties();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("Grid Size", "window");
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Label($"x: {GridSize.vector3IntValue.x}");
        if (GUILayout.Button("+"))
        {
            GridSize.vector3IntValue = new Vector3Int(GridSize.vector3IntValue.x + 1, GridSize.vector3IntValue.y, GridSize.vector3IntValue.z);
        }

        if (GUILayout.Button("-"))
        {
            GridSize.vector3IntValue = new Vector3Int(GridSize.vector3IntValue.x - 1, GridSize.vector3IntValue.y, GridSize.vector3IntValue.z);
        }
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        GUILayout.Label($"y: {GridSize.vector3IntValue.y}");
        if (GUILayout.Button("+"))
        {
            GridSize.vector3IntValue = new Vector3Int(GridSize.vector3IntValue.x, GridSize.vector3IntValue.y + 1, GridSize.vector3IntValue.z);
            GameObject newTilemapLayer = new GameObject();
            newTilemapLayer.transform.parent = _grid.transform;
            newTilemapLayer.AddComponent<Tilemap>();
            newTilemapLayer.name = $"Layer{GridSize.vector3IntValue.y - 1}";
            newTilemapLayer.transform.position = new Vector3(
                _grid.transform.position.x, 
                _grid.transform.position.y + (CellSize.intValue * (GridSize.vector3IntValue.y - 1)), 
                _grid.transform.position.z);
        }

        if (GUILayout.Button("-"))
        {
            if (_grid.transform.childCount == 0)
            {
                GridSize.vector3IntValue = new Vector3Int(GridSize.vector3IntValue.x, 0, GridSize.vector3IntValue.z);
                return;
            }

            Transform child = _grid.transform.GetChild(_grid.transform.childCount - 1);
            if (child.childCount > 0)
            {
                if (EditorUtility.DisplayDialog("Delete layer?", "Removing this layer would remove one or more Game Tiles. Proceed?", "OK", "Cancel"))
                {
                    GridSize.vector3IntValue = new Vector3Int(GridSize.vector3IntValue.x, GridSize.vector3IntValue.y - 1, GridSize.vector3IntValue.z);
                    Undo.DestroyObjectImmediate(_grid.transform.GetChild(_grid.transform.childCount - 1).gameObject);
                }
            }

            else
            {
                GridSize.vector3IntValue = new Vector3Int(GridSize.vector3IntValue.x, GridSize.vector3IntValue.y - 1, GridSize.vector3IntValue.z);
                Undo.DestroyObjectImmediate(_grid.transform.GetChild(_grid.transform.childCount - 1).gameObject);
            }
        }
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        GUILayout.Label($"z: {GridSize.vector3IntValue.z}");
        if (GUILayout.Button("+"))
        {
            GridSize.vector3IntValue = new Vector3Int(GridSize.vector3IntValue.x, GridSize.vector3IntValue.y, GridSize.vector3IntValue.z + 1);
        }

        if (GUILayout.Button("-"))
        {
            GridSize.vector3IntValue = new Vector3Int(GridSize.vector3IntValue.x, GridSize.vector3IntValue.y, GridSize.vector3IntValue.z - 1);
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();


        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_gameTileGrid, "Game Tile Grid Changed");
        }
    }
}
