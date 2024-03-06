using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public ForceCharacterHand[] forcedHands;
    private static TutorialManager instance;

    private void Awake()
    {
        instance = this;
    }

    public static void Setup()
    {
        instance.ForceCharacterHand();
    }

    public void ForceCharacterHand()
    {
        foreach (ForceCharacterHand hand in forcedHands)
        {
            PlayerEntity player = NewManager.instance.listOfPlayers.Find(x => x.name == hand.CharacterName);
            if (player != null)
                player.ForceHand(hand.CardNames);
        }
    }
}

[System.Serializable]
public class ForceCharacterHand
{
    public string CharacterName;
    public bool ForceHand;
    public string[] CardNames; 
}
