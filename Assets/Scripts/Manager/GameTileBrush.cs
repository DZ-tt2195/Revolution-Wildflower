using MyBox;
using System.Drawing;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[CustomGridBrush(true, true, true, "Game Tile Brush")]
public class GameTileBrush : GameObjectBrush
{
    [SerializeField] private GameTileGrid _tileGrid;
    private Grid _grid;

    private void OnEnable()
    {
        _tileGrid = FindObjectOfType<GameTileGrid>();
        if (!_tileGrid)
        {
            Debug.LogError($"{this} could not find a GameTileGrid in the scene! You'll need to assign it before placing any Game Tiles!");
        }

        _grid = _tileGrid.gameObject.GetComponent<Grid>();
    }

    public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
    {
        GameObject selection = Selection.activeGameObject;
        if (!selection.name.Contains("Layer") || selection.transform.parent != _grid.transform)
        {
            Debug.LogError($"{this}: tried drawing on something that isn't a layer; draw on a layer instead!");
            return;
        }

        if (position.x < 0 || position.y < 0)
        {
            Debug.LogError($"{this}: tried drawing a tile at a negative position");
            return;
        }

        if (position.x > _tileGrid.GridSize.x - 1 || position.y > _tileGrid.GridSize.z - 1)
        {
            Debug.LogError($"{this}: tried drawing out of bounds");
            return;
        }

        base.Paint(gridLayout, brushTarget, position);
        GameObject go = GetObjectInCell(gridLayout, brushTarget.transform, position, new Vector3(0.5f, 0.5f, 0), Vector3.zero);
        GameTile tile = go.GetComponent<GameTile>();

        if (tile == null)
        {
            Debug.LogError($"{this}: couldn't find component that implements ITile!");
            Destroy(go);
            return;
        }

        _tileGrid.AddTile(position, tile);
        GameTileRemovalCheck editMode = Undo.AddComponent<GameTileRemovalCheck>(go);
        editMode.Init(_tileGrid, position);
        Debug.Log($"{position.x}, {selection.transform.GetSiblingIndex()}, {position.y}");
    }

    public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
    {
        GameObject go = GetObjectInCell(gridLayout, brushTarget.transform, position, new Vector3(0.5f, 0.5f, 0), Vector3.zero);
        if (go == null)
        {
            base.Erase(gridLayout, brushTarget, position);
            return;
        }

        if (go.TryGetComponent(out GameTile tile))
        {
            _tileGrid.RemoveTile(position);
        }
        base.Erase(gridLayout, brushTarget, position);
    }



    private GameObject GetObjectInCell(GridLayout grid, Transform parent, Vector3Int position, Vector3 anchor, Vector3 offset)
    {
        int childCount;
        GameObject[] sceneChildren = null;
        if (parent == null)
        {
            var scene = SceneManager.GetActiveScene();
            sceneChildren = scene.GetRootGameObjects();
            childCount = scene.rootCount;
        }
        else
        {
            childCount = parent.childCount;
        }

        var anchorRatio = GetAnchorRatio(grid, anchor);
        var anchorLocal = grid.CellToLocalInterpolated(anchorRatio);
        for (var i = 0; i < childCount; i++)
        {
            var child = sceneChildren == null ? parent.GetChild(i) : sceneChildren[i].transform;
            var childCell = grid.LocalToCell(grid.WorldToLocal(child.position) - anchorLocal - offset);
            if (position == childCell)
                return child.gameObject;
        }
        return null;
    }

    private static Vector3 GetAnchorRatio(GridLayout grid, Vector3 cellAnchor)
    {
        var cellSize = grid.cellSize;
        var cellStride = cellSize + grid.cellGap;
        cellStride.x = Mathf.Approximately(0f, cellStride.x) ? 1f : cellStride.x;
        cellStride.y = Mathf.Approximately(0f, cellStride.y) ? 1f : cellStride.y;
        cellStride.z = Mathf.Approximately(0f, cellStride.z) ? 1f : cellStride.z;
        var anchorRatio = new Vector3(
            cellAnchor.x * cellSize.x / cellStride.x,
            cellAnchor.y * cellSize.y / cellStride.y,
            cellAnchor.z * cellSize.z / cellStride.z
        );
        return anchorRatio;
    }
}
