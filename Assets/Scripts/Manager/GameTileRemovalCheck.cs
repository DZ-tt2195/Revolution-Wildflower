using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class GameTileRemovalCheck : MonoBehaviour
{
    [SerializeField] private GameTileGrid _grid;
    [SerializeField] private Vector3Int _position;
    [SerializeField] private IGameTile _tile;
    public void Init(GameTileGrid grid, Vector3Int position)
    {
        _grid = grid;
        _position = position;   
    }

    public void OnDestroy()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
        {
            _grid.RemoveTile(_position);
        }
    }
}
