using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;
using System.Linq;
using TMPro;

public class PlayerEntity : MovingEntity
{

#region Variables

    [Foldout("Player Entity", true)]
        [Tooltip("The bar on screen")] [ReadOnly] public PlayerBar myBar;
        [Tooltip("Where this player's located in the list")] [ReadOnly] public int myPosition;
        [Tooltip("turns where you can't be caught")] [ReadOnly] public int health = 3;
        [Tooltip("turns where you can't be caught")] [ReadOnly] public int hidden = 0;
        //[Tooltip("normal player appearance")] [SerializeField] Material DefaultPlayerMaterial;
        //[Tooltip("appearance when hidden")] [SerializeField] Material HiddenPlayerMaterial;
        [Tooltip("adjacent objective")] [ReadOnly] public ObjectiveEntity adjacentObjective;
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
                tileOffset = new Vector3(0, 0.75f, 0);
                GetCards(0);
                break;
            case "Frankie":
                spriteRenderer.sprite = frankieSprite;
                tileOffset = new Vector3(-1, 0.75f, 0.8f);
                GetCards(1);
                break;
            case "WK":
                spriteRenderer.sprite = wkSprite;
                tileOffset = new Vector3(0, 0.75f, 0);
                GetCards(2);
                break;
        }

        PlayerEntity me = this;
        myBar.button.onClick.AddListener(() => NewManager.instance.FocusOnTile(me.currentTile, true));
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
        string answer = $"Moves left: {movementLeft}\n";
        if (stunned > 0)
            answer += $"Stunned for {stunned} turns\n";
        return answer;
    }

    public IEnumerator MovePlayer(List<TileData> path)
    {
        foreach(TileData tile in path)
        {
            yield return NewManager.Wait(PlayerPrefs.GetFloat("Animation Speed"));
            MoveTile(tile);
        }
        /*
        float timer = 0;
        for (int i = 0; i < path.Count; i++)
        {
            MoveTile(path[i]);
            while (timer < moveDelay)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            timer = 0;
        }
        */
    }
    public override void MoveTile(TileData newTile)
    {
        base.MoveTile(newTile);
        foreach (GuardEntity guard in NewManager.instance.listOfGuards)
            guard.CheckForPlayer();
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
        NewManager.instance.ChangeHealth(this, -damage);
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

    void SortHand()
    {
        myHand = myHand.OrderBy(o => o.energyCost).ToList();

        for (int i = 0; i < myHand.Count; i++)
        {
            Card nextCard = myHand[i];
            float startingX = (myHand.Count-1)*-50;
            float difference = 100;
            Vector2 newPosition = new(startingX + difference * i, -485);
            nextCard.transform.SetSiblingIndex(i);
            StartCoroutine(nextCard.MoveCard(newPosition, newPosition, Vector3.zero, PlayerPrefs.GetFloat("Animation Speed")));
        }

        foreach (Card card in myHand)
            StartCoroutine(card.RevealCard(PlayerPrefs.GetFloat("Animation Speed")));
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
        SortHand();
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

    internal IEnumerator DiscardFromHand(Card discardMe)
    {
        if (!myDiscardPile.Contains(discardMe))
        {
            myHand.Remove(discardMe);
            discardMe.transform.SetAsLastSibling();
            StartCoroutine(discardMe.MoveCard(new Vector2(1200, -440), new Vector2(0, -1000), Vector3.zero, PlayerPrefs.GetFloat("Animation Speed")));
            SortHand();
            yield return NewManager.Wait(PlayerPrefs.GetFloat("Animation Speed"));

            myDiscardPile.Add(discardMe);
            discardMe.transform.SetParent(null);
            discardMe.cardMove.Post(discardMe.gameObject);
        }
    }

    /*
    internal IEnumerator ExhaustFromHand(Card exhaustMe)
    {
        myHand.Remove(exhaustMe);
        myDrawPile.Remove(exhaustMe);
        myDiscardPile.Remove(exhaustMe);

        float zRot = UnityEngine.Random.Range(-45f, 45f);
        exhaustMe.transform.SetAsLastSibling();
        StartCoroutine(exhaustMe.MoveCard(new Vector2(exhaustMe.transform.localPosition.x, -700), new Vector2(0, -1000), new Vector3(0, 0, zRot), PlayerPrefs.GetFloat("Animation Speed")));
        StartCoroutine(exhaustMe.FadeAway(PlayerPrefs.GetFloat("Animation Speed")));
        SortHand();
        yield return NewManager.Wait(PlayerPrefs.GetFloat("Animation Speed"));

        myExhaust.Add(exhaustMe);
        exhaustMe.transform.SetParent(null);
    }
    */

    internal IEnumerator PlayCard(Card playMe, bool payEnergy)
    {
        NewManager.instance.DisableAllCards();
        playMe.cardPlay.Post(playMe.gameObject);
        StartCoroutine(playMe.MoveCard(new Vector2(playMe.transform.localPosition.x, -200), new Vector2(playMe.transform.localPosition.x, -200), new Vector3(0, 0, 0), PlayerPrefs.GetFloat("Animation Speed")));
        yield return playMe.FadeAway(PlayerPrefs.GetFloat("Animation Speed"));

        StartCoroutine(playMe.Unfade(0f));
        StartCoroutine(this.DiscardFromHand(playMe));

        if (payEnergy)
            NewManager.instance.ChangeEnergy(this, int.Parse(playMe.textCost.text)*-1);

        NewManager.instance.UpdateStats(this);
        yield return playMe.OnPlayEffect();

        this.cardsPlayed.Add(playMe);
        if (playMe.nextRoundEffectsInOrder != "")
            NewManager.instance.futureEffects.Add(playMe);
    }

    internal void MyTurn()
    {
        foreach (Card card in myHand)
        {
            card.transform.localPosition = new Vector3(0, -1000, 0);
            card.HideCard();
        }
        SortHand();
    }

    #endregion

}
