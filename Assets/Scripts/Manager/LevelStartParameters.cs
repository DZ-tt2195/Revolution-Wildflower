using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "IvyUnderground/LevelStartParameters")]
public class LevelStartParameters : ScriptableObject
{
    public bool dialogueOnStart = false;
    public TextAsset dialogueAsset;
    public LevelStartDialogueVariable[] dialogueVariables;
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