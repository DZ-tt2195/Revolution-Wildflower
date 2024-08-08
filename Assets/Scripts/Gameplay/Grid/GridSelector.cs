using MyBox.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridSelector : MonoBehaviour
{
    [SerializeField] private GameTileGrid _tileGrid;
    [SerializeField] private GridInputActions _controls;
    [SerializeField] private GridSelectionTracker _tracker;
    [SerializeField] private Vector3Int _startingTilePosition;
    private float _deadZone = 0.3f;
    private float _cooldownWaitSeconds = 0.1f;
    private bool _canMove = false;

    private InputAction _move; 

    private KeyValuePair<Vector3Int, GameTile> _selectedTile;

    private void Start()
    {
        _controls = new GridInputActions();
        

        _selectedTile = GetStartingTile();
        _selectedTile.Value.OnSelectionEnter();

        _move = _controls.Gameplay.Move;
        _move.performed += OnMove;
        _move.Enable();

        _canMove = true;

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

    private void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log(context.control);
        if (!_canMove)
        {
            return; 
        }

        Vector2 input = context.ReadValue<Vector2>();
        Vector3Int position = _selectedTile.Key;
        int xDir;
        int zDir;

        if (input.x < _deadZone && input.x > -_deadZone)
        {
            xDir = 0;
        }

        else
        {
            xDir = (int)Mathf.Sign(input.x); 
        }

        if (input.y < _deadZone && input.y > -_deadZone)
        {
            zDir = 0;
        }

        else
        {
            zDir = (int)Mathf.Sign(input.y);
        }

        Vector3Int newPosition = new Vector3Int(position.x + xDir, position.y + zDir, 0);

        if (_tileGrid.Tiles.ContainsKey(newPosition))
        {
            _selectedTile.Value.OnSelectionLeave();
            _selectedTile = new KeyValuePair<Vector3Int, GameTile>(newPosition, _tileGrid.Tiles[newPosition]);
            _selectedTile.Value.OnSelectionEnter();
            _tracker.Move(new Vector3(_tileGrid.GetWorldPoint(newPosition).x, _tileGrid.GetWorldPoint(newPosition).y + 2.1f, _tileGrid.GetWorldPoint(newPosition).z));

        }

        _canMove = false;
        Debug.Log(input.x + "  " + input.y);
        Debug.Log(xDir + "  " + zDir);
        StartCoroutine(Cooldown(_cooldownWaitSeconds));

    }

    private IEnumerator Cooldown(float cooldownFrames)
    {
        yield return new WaitForSeconds(cooldownFrames);
        _canMove = true;
    }

}
