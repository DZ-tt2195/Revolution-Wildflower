using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerClickHandler
{

#region Variables

    [Foldout("Choices", true)]
        [ReadOnly] public Image image;
        bool enableBorder;
        [ReadOnly] public Image border;
        [ReadOnly] public Button button;
        [SerializeField] Collector collector;

    [Foldout("Card stats", true)]
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
        [ReadOnly] TileData currentTarget;
        [ReadOnly] List<TileData> adjacentTilesWithPlayers = new();
        [ReadOnly] List<TileData> adjacentTilesWithGuards = new();
        [ReadOnly] List<TileData> adjacentTilesWithWalls = new();

    [Foldout("Audio files", true)]
        public AK.Wwise.Event cardMove;
        public AK.Wwise.Event cardPlay;

    #endregion

#region Setup

    private void Awake()
    {
        image = GetComponent<Image>();
        border = this.transform.GetChild(0).GetComponent<Image>();
        button = this.GetComponent<Button>();
        button.onClick.AddListener(SendMe);

        textName = this.transform.GetChild(1).GetComponent<TMP_Text>();
        textCost = this.transform.GetChild(2).GetComponent<TMP_Text>();
        textDescr = this.transform.GetChild(3).GetComponent<TMP_Text>();
    }

    void SendMe()
    {
        NewManager.instance.ReceiveChoice(this);
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

    public void EnableCard()
    {
        enableBorder = true;
        button.interactable = true;
    }

    public void DisableCard()
    {
        enableBorder = false;
        button.interactable = false;
    }

    private void FixedUpdate()
    {
        if (border != null && enableBorder)
        {
            border.color = new Color(1, 1, 1, NewManager.instance.opacity);
        }
        else if (border != null && !enableBorder)
        {
            border.color = new Color(1, 1, 1, 0);
        }
    }

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
            "ISPLAYER" => SearchAdjacentPlayers(currentPlayer.currentTile).Count > 0,
            "ISGUARD" => SearchAdjacentGuard(currentPlayer.currentTile).Count > 0,
            "ISWALL" => SearchAdjacentWall(currentPlayer.currentTile).Count > 0,
            "ISOCCUPIED" => OccupiedAdjacent(currentPlayer.currentTile).Count > 0,
            "EMPTYHAND" => currentPlayer.myHand.Count > 0,
            "NOENERGY" => currentPlayer.myEnergy > 0,
            "TARGETTED" => IsTargetted(),
            _ => true,
        };
    }

    bool IsTargetted()
    {
        foreach (GuardEntity guard in NewManager.instance.listOfGuards)
        {
            if (guard.CurrentTarget == currentPlayer)
                return true;
        }

        return false;
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

    List<TileData> SearchAdjacentPlayers(TileData playerTile)
    {
        List<TileData> tilesInRange = NewManager.instance.CalculateReachableGrids(playerTile, range, false);
        List<TileData> playersInRange = new();

        foreach (TileData tile in tilesInRange)
        {
            if (tile.myEntity != null && tile.myEntity.CompareTag("Player"))
                playersInRange.Add(tile);
        }

        adjacentTilesWithPlayers = playersInRange;
        return playersInRange;
    }

    List<TileData> SearchAdjacentGuard(TileData playerTile)
    {
        List<TileData> tilesInRange = NewManager.instance.CalculateReachableGrids(playerTile, range, false);
        List<TileData> guardsInRange = new();

        foreach (TileData tile in tilesInRange)
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
        List<TileData> wallsInRange = new();

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
        return costChangeCondition switch
        {
            "COSTS 2+" => (defaultCost >= 2) ? changeInEP : 0,
            _ => changeInEP,
        };
    }

    IEnumerator ResolveList(string effects)
    {
        string divide = effects.Replace(" ", "");
        divide = divide.ToUpper().Trim();
        string[] methodsInStrings = divide.Split('/');

        foreach (string nextMethod in methodsInStrings)
        {
            NewManager.instance.DisableAllTiles();
            NewManager.instance.DisableAllCards();

            if (nextMethod == "" || nextMethod == "NONE")
            {
                continue;
            }
            else
            {
                yield return ResolveMethod(nextMethod);
                NewManager.instance.UpdateStats(currentPlayer);
            }
        }
    }

    IEnumerator ResolveMethod(string methodName)
    {
        currentTarget = currentPlayer.currentTile;
        if (methodName.Contains("CHOOSEBUTTON("))
        {
            string[] choices = methodName.Replace("CHOOSEBUTTON(", "").Replace(")", "").Replace("]","").Trim().Split('|');
            yield return ChooseOptions(choices);
        }
        else
        {
            switch (methodName)
            {
                case "DRAWCARDS":
                    yield return DrawCards(currentPlayer);
                    break;
                case "CHOOSEDISCARD":
                    yield return ChooseDiscard(currentPlayer);
                    break;
                case "ALLDRAWCARDS":
                    yield return AllDrawCards(NewManager.instance.listOfPlayers);
                    break;
                case "CHANGEHP":
                    yield return ChangeHealth(currentPlayer);
                    break;
                case "CHANGEADJACENTHP":
                    yield return ChoosePlayer();
                    PlayerEntity player = adjacentTilesWithPlayers[0].myEntity.GetComponent<PlayerEntity>();
                    currentTarget = player.currentTile;
                    yield return ChangeHealth(player);
                    break;
                case "CHANGEEP":
                    yield return ChangeEnergy(currentPlayer);
                    break;
                case "CHANGEMP":
                    yield return ChangeMovement(currentPlayer);
                    break;
                case "DISCARDHAND":
                    yield return DiscardHand(currentPlayer);
                    break;
                case "CHANGECOST":
                    yield return ChangeCost(currentPlayer);
                    break;
                case "CHANGECOSTTWOPLUS":
                    yield return ChangeCostTwoPlus(currentPlayer);
                    break;
                case "STUNADJACENTGUARD":
                    yield return ChooseGuard();
                    yield return StunGuard(adjacentTilesWithGuards[0].myEntity.GetComponent<GuardEntity>());
                    break;
                case "ATTACKADJACENTWALL":
                    yield return ChooseWall();
                    yield return AttackWall(adjacentTilesWithWalls[0].myEntity.GetComponent<WallEntity>());
                    break;
                case "CENTERDISTRACTION":
                    yield return CalculateDistraction(currentPlayer.currentTile);
                    break;
                case "TARGETDISTRACTION&DAMAGE":
                    yield return AttackOrDistraction();
                    break;
                default:
                    Debug.LogError($"{methodName} isn't a method");
                    yield return null;
                    break;
            }
            if (distractionIntensity > 0)
            {
                StartCoroutine(CalculateDistraction(currentTarget));
            }
        }
    }

    IEnumerator ChooseOptions(string[] choices)
    {
        Collector newCollector = Instantiate(collector);
        newCollector.StatsSetup("Make a choice.", 0);

        foreach (string choice in choices)
        {
            switch (choice)
            {
                case "DRAWCARDS":
                    newCollector.AddTextButton($"Draw {changeInDraw}");
                    break;
                case "CHANGEMP":
                    newCollector.AddTextButton($"+{changeInMP} Movement");
                    break;
                case "CHANGEEP":
                    newCollector.AddTextButton($"+{changeInEP} Energy");
                    break;
                case "CHANGEHP":
                    newCollector.AddTextButton($"+{changeInHP} Health");
                    break;
            }
        }

        while (newCollector.chosenButton == -1)
            yield return null;

        yield return ResolveMethod(choices[newCollector.chosenButton]);
        Destroy(newCollector.gameObject);
    }

    public IEnumerator CalculateDistraction(TileData source)
    {
        print("distracting for " + textDescr.text);
        print("Intensity:" + distractionIntensity);
        List<TileData> affectedTiles = NewManager.instance.CalculateIntensity(source, distractionIntensity, true);
        print(affectedTiles.Count);
        for (int i = 0; i < affectedTiles.Count; i++)
        {
            StartCoroutine(affectedTiles[i].NoiseFlash(NewManager.instance.GetDistance(source.gridPosition, affectedTiles[i].gridPosition)));
            if (affectedTiles[i].myEntity != null)
            {
                //print("Tile has entity " + affectedTiles[i].myEntity.tag);
                if (affectedTiles[i].myEntity.CompareTag("Enemy"))
                {
                    //print("guard is notified");
                    GuardEntity noticer = affectedTiles[i].myEntity.GetComponent<GuardEntity>();
                    noticer.addDistraction(source.gridPosition);
                }
            }
        }
        yield return null;
    }

    public IEnumerator OnPlayEffect()
    {
        NewManager.instance.DisableAllCards();
        NewManager.instance.DisableAllTiles();
        yield return ResolveList(effectsInOrder);
    }

    public IEnumerator NextRoundEffect()
    {
        NewManager.instance.DisableAllCards();
        NewManager.instance.DisableAllTiles();
        yield return ResolveList(nextRoundEffectsInOrder);
    }

#endregion

#region Interacts With Cards

    internal IEnumerator DrawCards(PlayerEntity player)
    {
        player.PlusCards(changeInDraw);
        
        yield return null;
    }

    internal IEnumerator AllDrawCards(List<PlayerEntity> listOfPlayers)
    {
        CalculateDistraction(currentPlayer.currentTile);
        foreach (PlayerEntity player in listOfPlayers)
        {
            player.PlusCards(changeInDraw);
        }
        yield return null;
    }

    internal IEnumerator ChooseDiscard(PlayerEntity player)
    {
        CalculateDistraction(player.currentTile);
        for (int i = 0; i < chooseHand; i++)
        {
            NewManager.instance.UpdateInstructions($"Discard a card from your hand ({chooseHand - i} more).");
            if (player.myHand.Count >= 2)
            {
                NewManager.instance.WaitForDecision(player.myHand);
                while (NewManager.instance.chosenCard == null)
                    yield return null;
                yield return player.DiscardFromHand(NewManager.instance.chosenCard);
            }
            else if (player.myHand.Count == 1)
            {
                yield return player.DiscardFromHand(player.myHand[0]);
            }
            NewManager.instance.UpdateStats(currentPlayer);
        }
    }

    internal IEnumerator DiscardHand(PlayerEntity player)
    {
        CalculateDistraction(player.currentTile);
        while (player.myHand.Count > 0)
        {
            yield return NewManager.Wait(0.05f);
            StartCoroutine(player.DiscardFromHand(player.myHand[0]));
        }
    }

#endregion

#region Interacts With Stats

    internal IEnumerator ChangeCost(PlayerEntity player)
    {
        CalculateDistraction(player.currentTile);
        player.costChange.Add(this);
        yield return null;
    }

    internal IEnumerator ChangeCostTwoPlus(PlayerEntity player)
    {
        CalculateDistraction(player.currentTile);
        costChangeCondition = "COSTS 2+";
        yield return ChangeCost(player);
    }

    internal IEnumerator ChangeHealth(PlayerEntity player)
    {
        CalculateDistraction(player.currentTile);
        NewManager.instance.ChangeHealth(player, changeInHP);
        yield return null;
    }

    internal IEnumerator ChangeEnergy(PlayerEntity player)
    {
        CalculateDistraction(player.currentTile);
        NewManager.instance.ChangeEnergy(player, changeInEP);
        yield return null;
    }

    internal IEnumerator ZeroEnergy(PlayerEntity player)
    {
        CalculateDistraction(player.currentTile);
        NewManager.instance.SetEnergy(player, 0);
        yield return null;
    }

    internal IEnumerator ChangeMovement(PlayerEntity player)
    {
        CalculateDistraction(player.currentTile);
        NewManager.instance.ChangeMovement(player, changeInMP);
        yield return null;
    }

    internal IEnumerator ZeroMovement(PlayerEntity player)
    {
        CalculateDistraction(player.currentTile);
        NewManager.instance.SetMovement(player, 0);
        yield return null;
    }

    #endregion

    #region Interacts With Entities

    IEnumerator AttackOrDistraction()
    {
        yield return null;
    }
    IEnumerator ChoosePlayer()
    {
        TileData targetPlayer = null;

        if (adjacentTilesWithPlayers.Count == 1)
        {
            targetPlayer = adjacentTilesWithPlayers[0];
        }
        else
        {
            NewManager.instance.UpdateInstructions("Choose a player in range.");
            NewManager.instance.WaitForDecision(adjacentTilesWithPlayers);

            while (NewManager.instance.chosenTile == null)
                yield return null;
            targetPlayer = NewManager.instance.chosenTile;
        }

        adjacentTilesWithPlayers.Clear();
        adjacentTilesWithPlayers.Add(targetPlayer);
    }

    IEnumerator ChooseWall()
    {
        TileData targetWall = null;

        if (adjacentTilesWithWalls.Count == 1)
        {
            targetWall = adjacentTilesWithWalls[0];
        }
        else
        {
            NewManager.instance.UpdateInstructions("Choose a wall in range.");
            NewManager.instance.WaitForDecision(adjacentTilesWithWalls);
            while (NewManager.instance.chosenTile == null)
                yield return null;
            targetWall = NewManager.instance.chosenTile;
        }

        adjacentTilesWithWalls.Clear();
        adjacentTilesWithWalls.Add(targetWall);
    }

    internal IEnumerator AttackWall(WallEntity wall)
    {
        wall.AffectWall(changeInWall);
        currentTarget = wall.currentTile;
        yield return null;
    }

    IEnumerator ChooseGuard()
    {
        TileData targetGuard = null;

        if (adjacentTilesWithGuards.Count == 1)
        {
            targetGuard = adjacentTilesWithGuards[0];
        }
        else
        {
            NewManager.instance.UpdateInstructions("Choose a guard in range.");
            NewManager.instance.WaitForDecision(adjacentTilesWithGuards);
            while (NewManager.instance.chosenTile == null)
                yield return null;
            targetGuard = NewManager.instance.chosenTile;
        }

        adjacentTilesWithWalls.Clear();
        adjacentTilesWithWalls.Add(targetGuard);
    }

    internal IEnumerator StunGuard(GuardEntity guard)
    {
        yield return null;
        guard.stunSound.Post(guard.gameObject);
        guard.stunned += stunDuration;
        currentTarget = guard.currentTile;
    }

#endregion

}