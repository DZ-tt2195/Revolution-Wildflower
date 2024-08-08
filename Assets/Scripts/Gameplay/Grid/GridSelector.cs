using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GridSelector : MonoBehaviour
{
    [SerializeField] private GameTileGrid _tileGrid;
    [SerializeField] private InputActionAsset _controls;
    [SerializeField] private Vector3Int _startingTilePosition; 
    private GameTile _selectedTile;

    private void Start()
    {
        GetStartingTile();
    }

    private GameTile GetStartingTile()
    {
        GameTile tile;
        if (!_tileGrid.Tiles.ContainsKey(_startingTilePosition))
        {
            Debug.LogWarning(_tileGrid.Tiles.Count);
            KeyValuePair<Vector3Int, GameTile> firstEntry = _tileGrid.Tiles.ElementAt(0);
            tile = firstEntry.Value;
            Debug.LogWarning($"{this}: Couldn't find grid position {_startingTilePosition}, defaulting to first index at {firstEntry.Key}");
            return tile;
        }

        return _tileGrid.Tiles[_startingTilePosition];
    }

}
