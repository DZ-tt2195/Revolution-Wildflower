using System;
using UnityEngine;

[Serializable]
public abstract class GameTile : MonoBehaviour, IGameTile
{
    public abstract void OnSelectionEnter();
    public abstract void OnSelectionLeave();
}
