using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;

public class PlayerEntity : MovingEntity
{
    [Foldout("Player Entity", true)]
        [Tooltip("Where this player's located in the list")][ReadOnly] public int myPosition;
        [Tooltip("turns where you can't be caught")][ReadOnly] public int health = 3;
        [Tooltip("turns where you can't be caught")] [ReadOnly] public int hidden = 0;
        //[Tooltip("normal player appearance")] [SerializeField] Material DefaultPlayerMaterial;
        //[Tooltip("appearance when hidden")] [SerializeField] Material HiddenPlayerMaterial;
        [Tooltip("adjacent objective")][ReadOnly] public ObjectiveEntity adjacentObjective;

    [Foldout("Player's Cards", true)]
        [Tooltip("energy count")][ReadOnly] public int myEnergy;
        [Tooltip("keep cards in hand here")][ReadOnly] public Transform handTransform;
        [Tooltip("list of cards in hand")][ReadOnly] public List<Card> myHand;
        [Tooltip("list of cards in draw pile")][ReadOnly] public List<Card> myDrawPile;
        [Tooltip("list of cards in discard pile")][ReadOnly] public List<Card> myDiscardPile;
        [Tooltip("list of cards played this turn")][ReadOnly] public List<Card> cardsPlayed;

    #region Entity stuff

    public override string HoverBoxText()
    {
        return $"Moves left: {movementLeft}";
    }

    public override void MoveTile(TileData newTile)
    {
        base.MoveTile(newTile);
        foreach (GuardEntity guard in NewManager.instance.listOfGuards)
        {
            guard.CheckForPlayer();
        }
        NewManager.instance.objectiveButton.gameObject.SetActive(CheckForObjectives());
    }

    public bool CheckForObjectives()
    {
        for (int i = 0; i < this.currentTile.adjacentTiles.Count; i++)
        {
            TileData nextTile = this.currentTile.adjacentTiles[i];
            if (nextTile.myEntity != null && nextTile.myEntity.CompareTag("Objective"))
            {
                this.adjacentObjective = nextTile.myEntity.GetComponent<ObjectiveEntity>();
                return adjacentObjective.CanInteract();
            }
        }

        return false;
    }

    public override IEnumerator EndOfTurn()
    {
        yield return null;
        if (hidden > 0)
            hidden--;

        //meshRenderer.material = (hidden > 0) ? HiddenPlayerMaterial : DefaultPlayerMaterial;
    }
    #endregion

    #region Card Stuff

    public void DrawCards(int num)
    {
        for (int i = 0; i < num; i++)
        {
            try
            {
                PutIntoHand(GetTopCard());
            }
            catch (NullReferenceException)
            {
                break;
            }
        }
    }

    public Card GetTopCard()
    {
        if (myDrawPile.Count == 0)
        {
            myDiscardPile.Shuffle();
            while (myDiscardPile.Count > 0)
            {
                myDrawPile.Add(myDiscardPile[0]);
                myDiscardPile.RemoveAt(0);
            }
        }

        if (myDrawPile.Count > 0) //get the top card of the deck if there is one
        {
            Card card = myDrawPile[0];
            myDrawPile.RemoveAt(0);
            return card;
        }
        else
            return null;
    }

    public void PutIntoHand(Card drawMe)
    {
        if (drawMe != null)
        {
            myHand.Add(drawMe);
            drawMe.transform.SetParent(handTransform);
            drawMe.transform.localScale = new Vector3(1, 1, 1);
            SoundManager.instance.PlaySound(drawMe.cardMove);
        }
    }

    public void DiscardFromHand(Card discardMe)
    {
        myHand.Remove(discardMe);
        PutIntoDiscard(discardMe);
    }

    public void PutIntoDiscard(Card discardMe)
    {
        if (discardMe != null)
        {
            myDiscardPile.Add(discardMe);
            discardMe.transform.SetParent(null);
            discardMe.transform.localPosition = new Vector3(10000, 10000, 0); //send the card far away where you can't see it anymore
            SoundManager.instance.PlaySound(discardMe.cardMove);
        }
    }

    #endregion
}
