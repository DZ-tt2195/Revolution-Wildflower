using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;
using System.Linq;

public class PlayerEntity : MovingEntity
{
    [Foldout("Player Entity", true)]
        [Tooltip("The bar on screen")][ReadOnly] public PlayerBar myBar;
        [Tooltip("Where this player's located in the list")][ReadOnly] public int myPosition;
        [Tooltip("turns where you can't be caught")][ReadOnly] public int health = 3;
        [Tooltip("turns where you can't be caught")] [ReadOnly] public int hidden = 0;
        //[Tooltip("normal player appearance")] [SerializeField] Material DefaultPlayerMaterial;
        //[Tooltip("appearance when hidden")] [SerializeField] Material HiddenPlayerMaterial;
        [Tooltip("adjacent objective")][ReadOnly] public ObjectiveEntity adjacentObjective;

    [Foldout("Sprites", true)]
        [Tooltip("Gail's sprite")][SerializeField] Sprite gailSprite;
        [Tooltip("Frankie's sprite")][SerializeField] Sprite frankieSprite;
        [Tooltip("WK's sprite")][SerializeField] Sprite wkSprite;
        [Tooltip("HazardBox Sprite")][ReadOnly] CanvasGroup HazardBox;
        [Tooltip("HazardBox fade speed")][SerializeField] float FadeSpeed = 0.08f;


    [Foldout("Player's Cards", true)]
        [Tooltip("energy count")][ReadOnly] public int myEnergy;
        [Tooltip("keep cards in hand here")] Transform handTransform;
        [Tooltip("list of cards in hand")][ReadOnly] public List<Card> myHand;
        [Tooltip("list of cards in draw pile")][ReadOnly] public List<Card> myDrawPile;
        [Tooltip("list of cards in discard pile")][ReadOnly] public List<Card> myDiscardPile;
        [Tooltip("list of cards that're exhausted")][ReadOnly] public List<Card> myExhaust;
        [Tooltip("list of cards played this turn")][ReadOnly] public List<Card> cardsPlayed;
        [Tooltip("list of cost reduction effects")][ReadOnly] public List<Card> costChange;

#region Entity stuff

    public void PlayerSetup(string name, Transform hand)
    {
        HazardBox = NewManager.instance.ManagerHazardBox;
        HazardBox.alpha = 0;
        handTransform = hand;
        movementLeft = movesPerTurn;
        this.name = name;
        myBar.playerName.text = name;

        switch (name)
        {
            case "Gail":
                spriteRenderer.sprite = gailSprite;
                GetCards(0);
                break;
            case "Frankie":
                spriteRenderer.sprite = frankieSprite;
                GetCards(1);
                break;
            case "WK":
                spriteRenderer.sprite = wkSprite;
                GetCards(2);
                break;
        }

        PlayerEntity me = this;
        myBar.button.onClick.AddListener(() => NewManager.instance.FocusOnPlayer(me));
    }

    void GetCards(int n)
    {
        List<Card> addToDeck = SaveManager.instance.GenerateCards(SaveManager.instance.currentSaveData.chosenDecks[n]);
        foreach (Card card in addToDeck)
        {
            card.transform.SetParent(this.transform);
            this.myDrawPile.Add(card);
            card.transform.localPosition = new Vector3(10000, 10000, 0); //send the card far away where you can't see it anymore
            card.DisableCard();
        }

        this.myDrawPile.Shuffle(); //shuffle your deck
        this.PlusCards(5);
    }

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

    public IEnumerator TakeDamage(int damage)
    {
        HazardBox.alpha = 0;
        while (HazardBox.alpha < 1)
        {
            HazardBox.alpha += FadeSpeed;
            yield return null;
        }
        while (HazardBox.alpha > 0)
        {
            HazardBox.alpha -= FadeSpeed;
            yield return null;
        }
        health -= damage;
        yield return null;
    }

    public override IEnumerator EndOfTurn()
    {
        yield return null;
        costChange.Clear();
        hidden = hidden > 0 ? hidden - 1 : 0;

        //meshRenderer.material = (hidden > 0) ? HiddenPlayerMaterial : DefaultPlayerMaterial;
    }
    #endregion

#region Card Stuff

    void SortHand(float waitTime)
    {
        myHand = myHand.OrderBy(o => o.energyCost).ToList();

        for (int i = 0; i < myHand.Count; i++)
        {
            Card nextCard = myHand[i];
            float startingX = (myHand.Count >= 8) ? -900 : (myHand.Count - 1) * -150;
            float difference = (myHand.Count >= 8) ? 1800f / (myHand.Count - 1) : 300;
            Vector2 newPosition = new(startingX + difference * i, -500);
            StartCoroutine(MoveCard(nextCard, newPosition, newPosition, new Vector3(0, 0, 0), waitTime));
        }
    }

    public void PlusCards(int num)
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
        SortHand(0.4f);
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
        {
            return null;
        }
    }

    public void PutIntoHand(Card drawMe)
    {
        if (drawMe != null)
        {
            myHand.Add(drawMe);
            drawMe.transform.SetParent(handTransform);
            drawMe.transform.localScale = new Vector3(1, 1, 1);
            drawMe.transform.localPosition = new Vector3(0, -1000, 0);
            drawMe.cardMove.Post(drawMe.gameObject);
        }
    }

    public IEnumerator DiscardFromHand(Card discardMe)
    {
        myHand.Remove(discardMe);
        discardMe.transform.SetAsLastSibling();
        StartCoroutine(MoveCard(discardMe, new Vector2(1200, -440), new Vector2(0, -1000), new Vector3(0, 0, 0), 0.25f));
        SortHand(0.2f);
        yield return NewManager.Wait(0.4f);

        myDiscardPile.Add(discardMe);
        discardMe.transform.SetParent(null);
        discardMe.cardMove.Post(discardMe.gameObject);
    }

    public IEnumerator ExhaustFromHand(Card exhaustMe)
    {
        myHand.Remove(exhaustMe);
        myDrawPile.Remove(exhaustMe);
        myDiscardPile.Remove(exhaustMe);

        float zRot = UnityEngine.Random.Range(-45f, 45f);
        exhaustMe.transform.SetAsLastSibling();
        StartCoroutine(MoveCard(exhaustMe, new Vector2(exhaustMe.transform.localPosition.x, -700), new Vector2(0, -1000), new Vector3(0, 0, zRot), 0.3f));
        SortHand(0.2f);
        yield return NewManager.Wait(0.4f);

        myExhaust.Add(exhaustMe);
        exhaustMe.transform.SetParent(null);
    }

    IEnumerator MoveCard(Card card, Vector2 newPos, Vector2 finalPos, Vector3 newRot, float waitTime)
    {
        float elapsedTime = 0;
        Vector2 originalPos = card.transform.localPosition;
        Vector3 originalRot = card.transform.localEulerAngles;

        while (elapsedTime < waitTime)
        {
            card.transform.localPosition = Vector2.Lerp(originalPos, newPos, elapsedTime / waitTime);
            card.transform.localEulerAngles = Vector3.Lerp(originalRot, newRot, elapsedTime / waitTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        card.transform.localPosition = finalPos;
    }

    #endregion
}
