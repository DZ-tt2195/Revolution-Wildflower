using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;
using System.Linq;
using TMPro;
using Ink;

public class PlayerEntity : MovingEntity
{

#region Variables

    [Foldout("Player Entity", true)]
    [Tooltip("The bar on screen")][ReadOnly] public PlayerBar myBar;
    [Tooltip("Where this player's located in the list")][ReadOnly] public int myPosition;
    [Tooltip("amount of health before dying")][ReadOnly] public int health = 4;
    [Tooltip("turns where you can't be caught")][ReadOnly] public int hidden = 0;
    [Tooltip("highest energy you can have")][ReadOnly] public int maxEnergy = 5;
    [Tooltip("damage taken during guard turn")][ReadOnly] public int damageTaken = 0;

    //[Tooltip("normal player appearance")] [SerializeField] Material DefaultPlayerMaterial;
    //[Tooltip("appearance when hidden")] [SerializeField] Material HiddenPlayerMaterial;
    [Tooltip("adjacent objective")][ReadOnly] public ObjectiveEntity adjacentObjective;
    //[Tooltip("delay inbetween each movement")][SerializeField] public float moveDelay = 0.75f;

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
    //[Tooltip("list of cards that're exhausted")][ReadOnly] public List<Card> myExhaust;
    [Tooltip("list of cards played this turn")][ReadOnly] public List<Card> cardsPlayed;
    [Tooltip("list of cost reduction effects")][ReadOnly] public List<Card> costChange;

    #endregion

#region Entity stuff

    public void PlayerSetup(string name)
    {
        HazardBox = LevelUIManager.instance.ManagerHazardBox;
        HazardBox.alpha = 0;
        movementLeft = maxMovement;
        this.name = name;

        switch (name)
        {
            case "Gail":
                spriteRenderer.sprite = gailSprite;
                myPosition = 2;
                tileOffset = new Vector3(0, 0.75f, 0);
                break;
            case "Frankie":
                spriteRenderer.sprite = frankieSprite;
                myPosition = 1;
                tileOffset = new Vector3(-1, 0.75f, 0.8f);
                break;
            case "WK":
                spriteRenderer.sprite = wkSprite;
                myPosition = 0;
                tileOffset = new Vector3(0, 0.75f, 0);
                break;
        }
        Transform playerButtons = GameObject.Find("Player Buttons").transform;
        myBar = playerButtons.GetChild(myPosition).GetComponent<PlayerBar>();
        handTransform = LevelUIManager.instance.handContainer.GetChild(myPosition).GetChild(0);
        GetCards(myPosition);

        PlayerEntity me = this;
        myBar.button.onClick.AddListener(() => PhaseManager.instance.FocusOnTile(me.currentTile, true));
    }

    void GetCards(int n)
    {
        List<Card> addToDeck = SaveManager.instance.GenerateCards(SaveManager.instance.playerDecks[n]);
        foreach (Card card in addToDeck)
        {
            card.transform.SetParent(this.transform);
            this.myDrawPile.Add(card);
            card.transform.localPosition = new Vector3(10000, 10000, 0); //send the card far away where you can't see it anymore
            card.DisableCard();
        }

        this.myDrawPile.Shuffle(); //shuffle your deck
        this.PlusCards(3);
    }

    public override string HoverBoxText()
    {
        string answer = $"Moves left: {movementLeft}\n";
        if (stunned > 0)
            answer += $"Stunned for {stunned} turns\n";
        return answer;
    }

    public bool CheckForObjectives()
    {
        foreach (TileData tile in currentTile.adjacentTiles)
        {
            if (tile.myEntity != null && tile.myEntity.CompareTag("Objective"))
            {
                this.adjacentObjective = tile.myEntity.GetComponent<ObjectiveEntity>();
                return this.adjacentObjective.CanInteract();
            }
        }

        adjacentObjective = null;
        return false;
    }

    public IEnumerator TakeDamage(int damage)
    {

        damageTaken += damage;
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
        ChangeHealth(-damage);
        yield return null;
    }

    public override IEnumerator EndOfTurn()
    {
        yield return null;
        costChange.Clear();
        hidden = hidden > 0 ? hidden - 1 : 0;
    }

    /// <summary>
    /// set energy value
    /// </summary>
    /// <param name="n">to change to 2, n = 2</param>
    public void SetEnergy(int n)
    {
        ChangeEnergy(n - myEnergy);
    }

    /// <summary>
    /// change energy value
    /// </summary>
    /// <param name="n">to subtract 3 energy, n = -3</param>
    public void ChangeEnergy(int n)
    {
        this.myEnergy = Math.Clamp(this.myEnergy + n, 0, this.maxEnergy);
        LevelUIManager.instance.UpdateStats(this);
    }

    /// <summary>
    /// set health value
    /// </summary>
    /// <param name="player">the player</param>
    /// <param name="n">to change to 2, n = 2</param>
    public void SetHealth(int n)
    {
        ChangeHealth(n - health);
    }

    /// <summary>
    /// change health value
    /// </summary>
    /// <param name="n">to subtract 3 health, n = -3</param>
    public void ChangeHealth(int n)
    {
        this.health = Math.Clamp(this.health + n, 0, 3);

        if (n < 0)
            MoveCamera.instance.Shake();

        LevelUIManager.instance.UpdateStats(this);
        if (this.health <= 0)
            PhaseManager.instance.GameOver($"{this.name} lost all their HP.", false);
    }

    /// <summary>
    /// set movement value
    /// </summary>
    /// <param name="n">to change to 2, n = 2</param>
    public void SetMovement(int n)
    {
        ChangeMovement(n - movementLeft);
    }

    /// <summary>
    /// change movement value
    /// </summary>
    /// <param name="player">the player</param>
    /// <param name="n">to subtract 3 movement, n = -3</param>
    public void ChangeMovement(int n)
    {
        this.movementLeft = Math.Clamp(this.movementLeft + n, 0, this.maxMovement);
        LevelUIManager.instance.UpdateStats(this);

        if (this.movementLeft + n < 0)
            this.movementLeft += n;
    }

    #endregion

#region Card Stuff

    public IEnumerator SortHandCoroutine()
    {
        myHand = myHand.OrderBy(o => o.energyCost).ToList();

        for (int i = 0; i < myHand.Count; i++)
        {
            Card nextCard = myHand[i];
            float startingX = (myHand.Count - 1) * -(300 / 2);
            float difference = 300;
            Vector3 newPosition = new(startingX + difference * i, SaveManager.instance.cardBaseHeight, 0);
            nextCard.transform.SetSiblingIndex(i);

            var group = new CoroutineGroup(this);
            group.StartCoroutine(nextCard.RevealCard(PlayerPrefs.GetFloat("Animation Speed")));
            group.StartCoroutine(nextCard.MoveCard(newPosition, newPosition, PlayerPrefs.GetFloat("Animation Speed")));
            while (group.AnyProcessing)
                yield return null;
        }
    }

    public void ForceHand(string[] cards)
    {
        ShuffleIntoDeck(new List<Card>(myHand));
        foreach (string cardName in cards)
        {
            Card card = myDrawPile.Find(x => x.textName.text == cardName);
            if (card == null)
            {
                continue;
            }
            PlusCards(card);
        }
    }

    public void ForceTopDeck(string[] cards)
    {
        if (cards.Length > 0)
        {
            for (int i = cards.Length - 1; i >= 0; i--)
            {
                Card card = myDrawPile.Find(x => x.textName.text == cards[i]);
                if (card == null)
                {
                    continue;
                }

                myDrawPile.Remove(card);
                myDrawPile.Insert(0, card);
            }
        }
    }

    internal void PlusCards(Card card)
    {
        card.transform.SetParent(this.transform);
        card.HideCard();
        myDrawPile.Remove(card);
        myDiscardPile.Remove(card);
        PutIntoHand(card);
        StartCoroutine(SortHandCoroutine());
    }

    internal void PlusCards(int num)
    {
        for (int i = 0; i < num; i++)
        {
            if (myHand.Count < 5)
            {
                try
                {
                    Card nextCard = GetTopCard();
                    nextCard.HideCard();
                    PutIntoHand(nextCard);
                }
                catch (NullReferenceException) { break; }
            }
        }
        StartCoroutine(SortHandCoroutine());
    }

    internal Card GetTopCard()
    {
        /*
        if (myDrawPile.Count == 0)
        {
            myDiscardPile.Shuffle();
            while (myDiscardPile.Count > 0)
            {
                myDrawPile.Add(myDiscardPile[0]);
                myDiscardPile.RemoveAt(0);
            }
        }
        */
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

    void PutIntoHand(Card drawMe)
    {
        if (drawMe != null)
        {
            myHand.Add(drawMe);
            drawMe.transform.SetParent(handTransform);
            drawMe.transform.localScale = Vector3.one;
            drawMe.transform.localPosition = new Vector3(0, -1000, 0);
            drawMe.cardMove.Post(drawMe.gameObject);
        }
    }

    public void ShuffleIntoDeck(List<Card> listOfCards)
    {
        foreach (Card card in listOfCards)
        {
            myHand.Remove(card);
            myDrawPile.Add(card);
            card.transform.SetParent(null);
            card.transform.localPosition = new Vector3(10000, 10000, 0);
        }

        StartCoroutine(SortHandCoroutine());
        myDrawPile.Shuffle();
        Debug.Log(myHand.Count);
    }

    internal IEnumerator DiscardFromHand(Card discardMe)
    {
        if (!myDiscardPile.Contains(discardMe))
        {
            myHand.Remove(discardMe);
            discardMe.transform.SetAsLastSibling();
            StartCoroutine(discardMe.MoveCard(new Vector2(1200, -440), new Vector2(0, -1000), PlayerPrefs.GetFloat("Animation Speed")));
            yield return SortHandCoroutine();

            myDiscardPile.Add(discardMe);
            discardMe.transform.SetParent(null);
            discardMe.cardMove.Post(discardMe.gameObject);
        }
    }

    internal IEnumerator PlayCard(Card playMe, bool payEnergy)
    {
        LevelGenerator.instance.DisableAllCards();
        playMe.cardPlay.Post(playMe.gameObject);
        StartCoroutine(playMe.MoveCard(new Vector2(playMe.transform.localPosition.x, -200), new Vector2(playMe.transform.localPosition.x, -200), PlayerPrefs.GetFloat("Animation Speed")));
        yield return playMe.FadeAway(PlayerPrefs.GetFloat("Animation Speed"));

        StartCoroutine(playMe.Unfade(0f));
        StartCoroutine(this.DiscardFromHand(playMe));

        if (payEnergy)
            ChangeEnergy(int.Parse(playMe.textCost.text) * -1);

        LevelUIManager.instance.UpdateStats(this);
        yield return playMe.OnPlayEffect();

        this.cardsPlayed.Add(playMe);
        if (playMe.data.nextAct != "")
            PhaseManager.instance.futureEffects.Add(playMe);
    }

    internal void MyTurn()
    {
        foreach (Card nextCard in myHand)
        {
            nextCard.transform.localPosition = new Vector3(0, -1000, 0);
            nextCard.HideCard();
        }
        StartCoroutine(SortHandCoroutine());
    }

    #endregion

}
