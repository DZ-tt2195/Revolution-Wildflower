using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TestGameTile : GameTile, IGameTile
{
    private void Start()
    {
        Debug.Log("hii");
    }
}
