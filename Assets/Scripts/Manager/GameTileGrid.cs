using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class GameTile : MonoBehaviour, IGameTile
{

}
public interface IGameTile
{
}

public class GameTileGrid : MonoBehaviour
{
    [SerializeField] private Grid _grid;
    [SerializeField] private int _cellSize;
    [SerializeField] private Vector3Int _gridSize;
    [SerializeField] private SerializableDictionary<Vector3Int, GameTile> _tiles;

    public Vector3Int GridSize { get => _gridSize; }

    private void OnDrawGizmos()
    {
        PreviewGrid();
    }

    public void AddTile(Vector3Int position, GameTile tile)
    {
        _tiles[position] = tile;
        Debug.Log("Set Tile @" + position);

    }

    private void Start()
    {
        Debug.Log(_tiles.Count);
        string debug = "";
        foreach (KeyValuePair<Vector3Int, GameTile> pair in _tiles)
        {
            debug += $"({pair.Key}, {pair.Value})";
        }
        Debug.Log(debug);
    }

    public void RemoveTile(Vector3Int position)
    {
        if (! _tiles.ContainsKey(position))
        {
            return;
        }
        _tiles.Remove(position);
        string debug = "";
        foreach (KeyValuePair<Vector3Int, GameTile> pair in _tiles)
        {
            debug += $"({pair.Key}, {pair.Value})";
        }
        Debug.Log(debug);
    }

    private Vector3 GetWorldPoint(Vector3Int gridPosition)
    {
        return new Vector3((_cellSize / 2f) + (_cellSize * gridPosition.x), gridPosition.z * _cellSize, (_cellSize / 2f) + (_cellSize * gridPosition.y));
    }

    public List<IGameTile> GetNeighbors(Vector3Int position)
    {
        List<IGameTile> neighbors = new()
        {
            FindTile(new Vector3Int(position.x + 1, position.y, position.z)),
            FindTile(new Vector3Int(position.x - 1, position.y, position.z)),
            FindTile(new Vector3Int(position.x, position.y, position.z + 1)),
            FindTile(new Vector3Int(position.x, position.y, position.z - 1))
        };
        neighbors.RemoveAll(item => item == null); //delete all tiles that are null
        return neighbors;
    }
    private IGameTile FindTile(Vector3Int position)
    {
        try { return _tiles[position]; }
        catch (IndexOutOfRangeException) { return null; }
    }
    private void PreviewGrid()
    {
        for (int i = 0; i < _gridSize.y; i++)
        {
            for (int j = 0; j < _gridSize.x; j++)
            {
                for (int k = 0; k < _gridSize.z; k++)
                {
                    Vector3 cellPosition = GetWorldPoint(new Vector3Int(j, k, i));
                    Gizmos.color = new Color((float)j / _gridSize.x, (float)k / _gridSize.z, (float)k / _gridSize.z, 0.6f);
                    Gizmos.DrawCube(cellPosition, Vector3.one);
                }
            }
        }
    }
}

/// <summary>
/// Serialized Dictionary, courtesy of https://discussions.unity.com/t/solved-how-to-serialize-dictionary-with-unity-serialization-system/71474. 
/// The Grid Dictionary is modified in Edit Mode, so we need serialialization to make the changes stick. 
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<TKey> _keys = new();

    [SerializeField]
    private List<TValue> _values = new();

    // save the dictionary to lists
    public void OnBeforeSerialize()
    {
        _keys.Clear();
        _values.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            _keys.Add(pair.Key);
            _values.Add(pair.Value);
        }
    }

    // load dictionary from lists
    public void OnAfterDeserialize()
    {
        Clear();

        if (_keys.Count != _values.Count)
        {
            Debug.LogError($"there are {_keys.Count} keys and {_values.Count} values after deserialization. Make sure that both key and value types are serializable.");
        }

        for (int i = 0; i < _keys.Count; i++)
        {
            Add(_keys[i], _values[i]);
        }
    }
}
