using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using System;

public class ChoiceManager : MonoBehaviour
{
    public static ChoiceManager instance;
    [ReadOnly] public Card chosenCard;
    [ReadOnly] public TileData chosenTile;
    [ReadOnly] public float opacity = 1;
    [ReadOnly] public bool decrease = true;
    [SerializeField] AK.Wwise.Event button;
    [SerializeField] AK.Wwise.Event tileSelect;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void FixedUpdate()
    {
        //dicates how cards flash when they can be chosen
        if (decrease)
            opacity -= 0.05f;
        else
            opacity += 0.05f;
        if (opacity < 0 || opacity > 1)
            decrease = !decrease;
    }

    public void ReceiveChoice(Card chosenCard)
    {
        //Debug.Log($"chosen {chosenCard.name}");
        this.chosenCard = chosenCard;
    }

    public void ReceiveChoice(TileData chosenTile)
    {
        //Debug.Log($"chosen {chosenTile.name}");
        button.Post(chosenTile.gameObject);
        tileSelect.Post(chosenTile.gameObject);
        this.chosenTile = chosenTile;
    }

    public void ChooseTile(List<TileData> choices)
    {
        chosenCard = null;
        chosenTile = null;

        for (int i = 0; i < choices.Count; i++)
        {
            choices[i].moveable = true;
        }
    }

    public void DisableAllTiles()
    {
        for (int i = 0; i < NewManager.instance.listOfTiles.GetLength(0); i++)
        {
            for (int j = 0; j < NewManager.instance.listOfTiles.GetLength(1); j++)
            {
                try
                {
                    NewManager.instance.listOfTiles[i, j].moveable = false;
                }
                catch (NullReferenceException)
                {
                    continue;
                }
            }
        }
    }

    public void ChooseCard(List<Card> choices)
    {
        chosenCard = null;
        chosenTile = null;

        for (int i = 0; i < choices.Count; i++)
            choices[i].choiceScript.EnableButton(true);
    }

    public void DisableAllCards()
    {
        chosenCard = null;
        chosenTile = null;

        foreach (Card card in SaveManager.instance.allCards)
        {
            card.choiceScript.DisableButton();
        }
    }
}
