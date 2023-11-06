using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using UnityEngine.EventSystems;

public class StringAndMethod
{
    [ReadOnly] public Dictionary<string, IEnumerator> dictionary = new Dictionary<string, IEnumerator>();

    public StringAndMethod(Card card)
    {
        dictionary["DRAWCARDS"] = card.DrawCards();
        dictionary["CHOOSEDISCARD"] = card.ChooseDiscard();
        dictionary["ALLDRAWCARDS"] = card.AllDrawCards();
        dictionary["CHANGEHP"] = card.ChangeHealth();
        dictionary["CHANGEEP"] = card.ChangeEnergy();
        dictionary["CHANGEMP"] = card.ChangeMovement();
        dictionary["FINDONE"] = card.FindOne();
        dictionary["DISCARDHAND"] = card.DiscardHand();
        dictionary["CHANGESCOST"] = card.ChangeCost();
        dictionary["CHANGECOSTTWOPLUS"] = card.ChangeCostTwoPlus();
        dictionary["STUNADJACENTGUARD"] = card.StunAdjacentGuard();
        dictionary["AFFECTADJACENTWALL"] = card.AffectAdjacentWall();
    }
}

public class Card : MonoBehaviour, IPointerClickHandler
{

#region Card Stats

    [ReadOnly] public Image image;
    [ReadOnly] public SendChoice choiceScript;

    [ReadOnly] public int energyCost;
    public enum CardType { Attack, Draw, Distraction, Energy, Movement, Misc, None };
    [ReadOnly] public CardType typeOne;
    [ReadOnly] public CardType typeTwo;
    [ReadOnly] public bool violent;

    [ReadOnly] int changeInHP;
    [ReadOnly] int changeInMP;
    [ReadOnly] int changeInEP;
    [ReadOnly] int changeInDraw;
    [ReadOnly] int chooseHand;

    [ReadOnly] int stunDuration;
    [ReadOnly] int range;
    [ReadOnly] int areaOfEffect;
    [ReadOnly] int delay;
    [ReadOnly] int changeInWall;
    [ReadOnly] int burnDuration;
    [ReadOnly] int distractionIntensity;

    [ReadOnly] string selectCondition;
    [ReadOnly] string effectsInOrder;
    [ReadOnly] string nextRoundEffectsInOrder;
    [ReadOnly] string costChangeCondition;

    [ReadOnly] public TMP_Text textName { get; private set; } 
    [ReadOnly] public TMP_Text textCost { get; private set; }
    [ReadOnly] public TMP_Text textDescr { get; private set; }

    [ReadOnly] PlayerEntity currentPlayer;
    [ReadOnly] List<TileData> adjacentTilesWithGuards = new List<TileData>();
    [ReadOnly] List<TileData> adjacentTilesWithWalls = new List<TileData>();

    public AK.Wwise.Event cardMove;
    public AK.Wwise.Event cardPlay;
    
#endregion

#region Setup

    private void Awake()
    {
        image = GetComponent<Image>();
        choiceScript = GetComponent<SendChoice>();

        textName = this.transform.GetChild(1).GetComponent<TMP_Text>();
        textCost = this.transform.GetChild(2).GetComponent<TMP_Text>();
        textDescr = this.transform.GetChild(3).GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            RightClick.instance.ChangeCard(this);
            cardMove.Post(gameObject);
        }
    }

    public void CardSetup(CardData data)
    {
        textName.text = data.name;
        textDescr.text = data.desc;

        typeOne = ConvertToType(data.cat1);
        typeTwo = ConvertToType(data.cat2);

        energyCost = data.epCost;
        textCost.text = $"{data.epCost}";
        violent = data.isViolent;

        changeInHP = data.chHP;
        changeInMP = data.chMP;
        changeInEP = data.chEP;
        changeInDraw = data.draw;
        chooseHand = data.chooseHand;

        stunDuration = data.stun;
        range = data.range;
        areaOfEffect = data.aoe;
        delay = data.delay;
        changeInWall = data.wHP;

        burnDuration = data.burn;
        distractionIntensity = data.intn;

        selectCondition = data.select;
        effectsInOrder = data.action;
        nextRoundEffectsInOrder = data.nextAct;
    }

    CardType ConvertToType(string type)
    {
        return type switch
        {
            "draw" => CardType.Draw,
            "atk" => CardType.Attack,
            "dist" => CardType.Distraction,
            "eng" => CardType.Energy,
            "mvmt" => CardType.Movement,
            "misc" => CardType.Misc,
            _ => CardType.None,
        };
    }

#endregion

#region Play Condition

    int ApplyCostChange()
    {
        int changedEnergyCost = energyCost;

        foreach (Card nextEffect in currentPlayer.costChange)
        {
            changedEnergyCost += nextEffect.CostChanger(energyCost);
        }

        textCost.text = $"{changedEnergyCost}";
        return changedEnergyCost < 0 ? 0 : changedEnergyCost;
    }

    public bool CanPlay(PlayerEntity player)
    {
        currentPlayer = player;
        image.color = Color.white;

        if (player.myEnergy >= ApplyCostChange())
        {
            string divide = selectCondition.Replace(" ", "");
            divide = divide.ToUpper();
            string[] methodsInStrings = divide.Split('/');

            foreach (string nextMethod in methodsInStrings)
            {
                if (!CheckIfCanPlay(nextMethod))
                {
                    image.color = Color.gray;
                    return false;
                }
            }

            image.color = Color.white;
            return true;
        }
        else
        {
            image.color = Color.gray;
            return false;
        }
    }

    bool CheckIfCanPlay(string nextMethod)
    {
        return nextMethod switch
        {
            "ISGUARD" => SearchAdjacentGuard(currentPlayer.currentTile).Count > 0,
            "ISWALL" => SearchAdjacentWall(currentPlayer.currentTile).Count > 0,
            "ISOCCUPIED" => OccupiedAdjacent(currentPlayer.currentTile).Count > 0,
            "EMPTYHAND" => currentPlayer.myHand.Count > 0,
            "NOENERGY" => currentPlayer.myEnergy > 0,
            _ => true,
        };
    }

    List<TileData> OccupiedAdjacent(TileData playerTile)
    {
        List<TileData> occupiedTiles = new List<TileData>();
        List<TileData> tilesInRange = NewManager.instance.CalculateReachableGrids(playerTile, range, false);

        foreach (TileData tile in tilesInRange)
        {
            if (tile.myEntity != null)
                occupiedTiles.Add(tile);
        }
        return occupiedTiles;
    }

    List<TileData> SearchAdjacentGuard(TileData playerTile)
    {
        List<TileData> tilesInRange = NewManager.instance.CalculateReachableGrids(playerTile, range, false);
        List<TileData> guardsInRange = new List<TileData>();

        foreach(TileData tile in tilesInRange)
        {
            if (tile.myEntity != null && tile.myEntity.CompareTag("Enemy"))
                guardsInRange.Add(tile);
        }

        adjacentTilesWithGuards = guardsInRange;
        return guardsInRange;
    }

    List<TileData> SearchAdjacentWall(TileData playerTile)
    {
        List<TileData> tilesInRange = NewManager.instance.CalculateReachableGrids(playerTile, range, false);
        List<TileData> wallsInRange = new List<TileData>();

        foreach (TileData tile in tilesInRange)
        {
            if (tile.myEntity != null && tile.myEntity.CompareTag("Wall"))
                wallsInRange.Add(tile);
        }

        adjacentTilesWithWalls = wallsInRange;
        return wallsInRange;
    }

#endregion

#region Play Effect

    public int CostChanger(int defaultCost)
    {
        switch (costChangeCondition)
        {
            case "COSTS 2+":
                return (defaultCost >= 2) ? changeInEP : 0;
            default:
                return changeInEP;
        }
    }

    IEnumerator ResolveList(string divide)
    {
        StringAndMethod dic = new StringAndMethod(this);
        divide = divide.Replace(" ", "");
        divide = divide.ToUpper();
        string[] methodsInStrings = divide.Split('/');

        foreach(string nextMethod in methodsInStrings)
        {
            if (nextMethod == "" || nextMethod == "NONE")
            {
                continue;
            }
            else if (dic.dictionary.TryGetValue(nextMethod, out IEnumerator method))
            {
                yield return method;
            }
            else
            {
                Debug.LogError($"\"{nextMethod}\" for {this.name} isn't in the dictionary");
            }
        }
    }

    public IEnumerator OnPlayEffect()
    {
        ChoiceManager.instance.DisableAllCards();
        ChoiceManager.instance.DisableAllTiles();
        yield return ResolveList(effectsInOrder);
    }

    public IEnumerator NextRoundEffect()
    {
        ChoiceManager.instance.DisableAllCards();
        ChoiceManager.instance.DisableAllTiles();
        yield return ResolveList(nextRoundEffectsInOrder);
    }

#endregion

#region Interacts With Cards

    internal IEnumerator DrawCards()
    {
        currentPlayer.PlusCards(changeInDraw);
        yield return null;
    }

    internal IEnumerator AllDrawCards()
    {
        foreach(PlayerEntity player in NewManager.instance.listOfPlayers)
        {
            player.PlusCards(changeInDraw);
        }
        yield return null;
    }

    internal IEnumerator ChooseDiscard()
    {
        for (int i = 0; i<chooseHand; i++)
        {
            NewManager.instance.UpdateInstructions($"Discard a card from your hand ({chooseHand-i} more).");
            if (currentPlayer.myHand.Count >= 2)
            {
                ChoiceManager.instance.ChooseCard(currentPlayer.myHand);
                while (ChoiceManager.instance.chosenCard == null)
                    yield return null;
                currentPlayer.DiscardFromHand(ChoiceManager.instance.chosenCard);
            }
            else if (currentPlayer.myHand.Count == 1)
            {
                currentPlayer.DiscardFromHand(currentPlayer.myHand[0]);
            }
        }
    }

    internal Card FindCardType(CardType type)
    {
        List<Card> invalidCards = new List<Card>();
        Card foundCard = null;
        while (foundCard == null)
        {
            Card nextCard = currentPlayer.GetTopCard();
            if (nextCard == null)
            {
                break;
            }
            else if (nextCard.typeOne == type || nextCard.typeTwo == type)
            {
                foundCard = nextCard;
            }
            else
            {
                invalidCards.Add(nextCard);
                nextCard.transform.SetParent(null);
            }
        }

        foreach (Card card in invalidCards)
            currentPlayer.PutIntoDiscard(card);
        return foundCard;
    }

    internal Card FindCardCost(int cost)
    {
        List<Card> invalidCards = new List<Card>();
        Card foundCard = null;
        while (foundCard == null)
        {
            Card nextCard = currentPlayer.GetTopCard();
            if (nextCard == null)
            {
                break;
            }
            else if (nextCard.energyCost == cost)
            {
                foundCard = nextCard;
            }
            else
            {
                invalidCards.Add(nextCard);
                nextCard.transform.SetParent(null);
            }
        }
        foreach (Card card in invalidCards)
            currentPlayer.PutIntoDiscard(card);

        return foundCard;
    }

    internal IEnumerator DiscardHand()
    {
        while (currentPlayer.myHand.Count>0)
        {
            yield return NewManager.Wait(0.05f);
            currentPlayer.DiscardFromHand(currentPlayer.myHand[0]);
        }
    }

    internal IEnumerator FindOne()
    {
        for (int i = 0; i < 2; i++)
        {
            yield return NewManager.Wait(0.05f);
            currentPlayer.PutIntoHand(FindCardCost(1));
        }
    }

#endregion

#region Interacts With Stats

    internal IEnumerator ChangeCost()
    {
        currentPlayer.costChange.Add(this);
        yield return null;
    }

    internal IEnumerator ChangeCostTwoPlus()
    {
        costChangeCondition = "COSTS 2+";
        yield return ChangeCost();
    }

    internal IEnumerator ChangeHealth()
    {
        NewManager.instance.ChangeHealth(currentPlayer, changeInHP);
        yield return null;
    }

    internal IEnumerator ChangeEnergy()
    {
        NewManager.instance.ChangeEnergy(currentPlayer, changeInEP);
        yield return null;
    }

    internal IEnumerator ZeroEnergy()
    {
        NewManager.instance.SetEnergy(currentPlayer, 0);
        yield return null;
    }

    internal IEnumerator ChangeMovement()
    {
        NewManager.instance.ChangeMovement(currentPlayer, changeInMP);
        yield return null;
    }

    internal IEnumerator ZeroMovement()
    {
        NewManager.instance.SetMovement(currentPlayer, 0);
        yield return null;
    }

    #endregion

#region Interacts With Entities

    internal IEnumerator AffectAdjacentWall()
    {
        WallEntity targetWall = null;

        if (adjacentTilesWithWalls.Count == 1)
        {
            targetWall = adjacentTilesWithWalls[0].myEntity.GetComponent<WallEntity>();
        }
        else
        {
            NewManager.instance.UpdateInstructions("Choose a wall in range.");
            ChoiceManager.instance.ChooseTile(adjacentTilesWithWalls);
            while (ChoiceManager.instance.chosenTile == null)
                yield return null;
            targetWall = ChoiceManager.instance.chosenTile.myEntity.GetComponent<WallEntity>();
        }
        targetWall.AffectWall(changeInWall);
    }

    internal IEnumerator StunAdjacentGuard()
    {
        GuardEntity targetGuard = null;

        if (adjacentTilesWithWalls.Count == 1)
        {
            targetGuard = adjacentTilesWithGuards[0].myEntity.GetComponent<GuardEntity>();
        }
        else
        {
            NewManager.instance.UpdateInstructions("Choose a guard in range.");
            ChoiceManager.instance.ChooseTile(adjacentTilesWithGuards);
            while (ChoiceManager.instance.chosenTile == null)
                yield return null;
            targetGuard = ChoiceManager.instance.chosenTile.myEntity.GetComponent<GuardEntity>();
        }

        targetGuard.stunSound.Post(targetGuard.gameObject);
        targetGuard.stunned += stunDuration;
    }
#endregion
}
