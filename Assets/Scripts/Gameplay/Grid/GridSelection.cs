using MyBox.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The <c>GridSelection</c> class is responsible for tracking which tile the player is currently hovering over. 
/// </summary>
public class GridSelection : MonoBehaviour
{
    /// <summary>
    /// A reference to the tile grid. 
    /// </summary>
    [SerializeField] private GameTileGrid _tileGrid;

    /// <summary>
    /// See class definition. Handles the various sources of input that change which tile is selected.
    /// </summary>
    [SerializeField] private GridInput _input;

    /// <summary>
    /// See class definition. A MonoBehaviour that follows the position of the selected tile. 
    /// </summary>
    [SerializeField] private GridSelectionTracker _tracker;

    /// <summary>
    /// The starting position of the selection; should correspond with one of the character spawn points. 
    /// </summary>
    [SerializeField] private Vector3Int _startingTilePosition;

    /// <summary>
    /// The position and script of the currently selected tile. 
    /// </summary>
    private KeyValuePair<Vector3Int, GameTile> _currentTile;
    public KeyValuePair<Vector3Int, GameTile> CurrentTile { get => _currentTile; private set => _currentTile = value; }

    public static event EventHandler OnSelect;

    private void Start()
    {
        _input.Enable();
        _currentTile = GetStartingTile();
        _currentTile.Value.OnSelectionEnter();

        OnSelect += _tracker.OnSelect;
    }

    /// <summary>
    /// If the <c>GameTileGrid</c> contains the given position, deselects the old <c>GameTile</c> and selects the new one. 
    /// </summary>
    /// <param name="position">The position of the new <c>GameTile</c>.</param>
    public void SetSelection(Vector3Int position)
    {
        if (_tileGrid.Tiles.ContainsKey(position))
        {
            _currentTile.Value.OnSelectionLeave();
            _currentTile = new KeyValuePair<Vector3Int, GameTile>(position, _tileGrid.Tiles[position]);
            _currentTile.Value.OnSelectionEnter();
            _tracker.Move(new Vector3(_tileGrid.GetWorldPoint(_currentTile.Key).x, _tileGrid.GetWorldPoint(_currentTile.Key).y + 2.1f, _tileGrid.GetWorldPoint(_currentTile.Key).z));
        }
    }

    public void Fire()
    {
        OnSelect?.Invoke(this, EventArgs.Empty);
    }

    private KeyValuePair<Vector3Int, GameTile> GetStartingTile()
    {
        KeyValuePair<Vector3Int, GameTile> tile;
        if (!_tileGrid.Tiles.ContainsKey(_startingTilePosition))
        {
            KeyValuePair<Vector3Int, GameTile> firstEntry = _tileGrid.Tiles.ElementAt(0);
            tile = firstEntry;
            Debug.LogWarning($"{this}: Couldn't find grid position {_startingTilePosition}, defaulting to first index at {firstEntry.Key}");
            return tile;
        }

        return new KeyValuePair<Vector3Int, GameTile>(_startingTilePosition, _tileGrid.Tiles[_startingTilePosition]);
    }

}
