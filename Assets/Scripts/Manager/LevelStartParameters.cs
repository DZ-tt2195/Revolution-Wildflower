using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "IvyUnderground/LevelStartParameters")]
public class LevelStartParameters : ScriptableObject
{
    [Header("Environment")]
    public FloorGraphicsData floorGraphicsData; 
    public WallGraphicsData wallGraphicsData;

    [Header("Dialogue")]
    public bool dialogueOnStart = false;
    public TextAsset dialogueAsset;
    public LevelStartDialogueVariable[] dialogueVariables;

    [Header("Gameplay")]
    public ForceCharacterHand[] forcedHands;
    public ForceCharacterDeck[] forcedDecks; 


    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
    }
}

[System.Serializable]
public class LevelStartDialogueVariable
{
    public string name;
    public string value;
}

[System.Serializable]
public class WallGraphicsData
{
    public GameObject plusWall;
    public GameObject tWall;
    public GameObject cornerWall;
    public GameObject defaultWall; 
}

[System.Serializable]
public class FloorGraphicsData
{
    public Texture2D baseTexture;
    public Color textureColor;
    public Color paletteColor;
    public Texture2D normalMap;
    public Texture2D printOverlay;
    public float overlayIntensity;
    public float metallic;
    public float smoothness; 
}