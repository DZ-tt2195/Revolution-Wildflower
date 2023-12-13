using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;

public class Card : MonoBehaviour, IPointerClickHandler
{

#region Variables

    [Foldout("Choices", true)]
        [ReadOnly] public Image image;
        bool enableBorder;
        [ReadOnly] public Image border;
        [ReadOnly] public Button button;
        [SerializeField] Collector collector;
        [Tooltip("the front of a card")][SerializeField] Sprite cardFront;
        [Tooltip("the back of a card")][SerializeField] Sprite cardBack;

    [Foldout("Texts", true)]
        public CanvasGroup canvasgroup;
        public TMP_Text textName;
        public TMP_Text textCost;
        public TMP_Text textDescr;

    [Foldout("Card stats", true)]
        [ReadOnly] public int energyCost;
        public enum CardType { Attack, Draw, Distraction, Energy, Movement, Misc, None };
        [ReadOnly] public CardType typeOne { get; private set; }
        [ReadOnly] public CardType typeTwo { get; private set; }
        [ReadOnly] public bool violent { get; private set; }

        [ReadOnly] public int changeInHP { get; private set; }
        [ReadOnly] public int changeInMP { get; private set; }
        [ReadOnly] public int changeInEP { get; private set; }
        [ReadOnly] public int changeInDraw { get; private set; }
        [ReadOnly] public int chooseHand { get; private set; }

        [ReadOnly] public int stunDuration { get; private set; }
        [ReadOnly] int range;
        [ReadOnly] public int areaOfEffect { get; private set; }
        [ReadOnly] public int delay { get; private set; }
<<<<<<< HEAD
        [ReadOnly] int changeInWall;
=======
        [ReadOnly] public int changeInWall { get; private set; }
>>>>>>> main
        [ReadOnly] int burnDuration;
        [ReadOnly] int distractionIntensity;

        [ReadOnly] string selectCondition;
        [ReadOnly] public string effectsInOrder{ get; private set; }
        [ReadOnly] public string enviroEffect { get; private set; }
        [ReadOnly] public string nextRoundEffectsInOrder { get; private set; }
        [ReadOnly] public string costChangeCondition{ get; private set; }

        [ReadOnly] PlayerEntity currentPlayer;
        [ReadOnly] TileData currentTarget;
        [ReadOnly] List<TileData> adjacentTilesWithPlayers = new();
        [ReadOnly] List<TileData> adjacentTilesWithGuards = new();
        [ReadOnly] List<TileData> adjacentTilesWithWalls = new();

    [Foldout("Audio files", true)]
        public AK.Wwise.Event cardMove;
        public AK.Wwise.Event cardPlay;
        [SerializeField] AK.Wwise.Event addDistractionSound;

#endregion

#region Setup

    private void Awake()
    {
        image = GetComponent<Image>();
        border = this.transform.GetChild(0).GetComponent<Image>();
        button = this.GetComponent<Button>();
        button.onClick.AddListener(SendMe);
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
        enviroEffect = data.enviroaction;
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

        if (player.myEnergy >= ApplyCostChange())
        {
            string divide = selectCondition.Replace(" ", "");
            divide = divide.ToUpper();
            string[] methodsInStrings = divide.Split('/');

            foreach (string nextMethod in methodsInStrings)
            {
                if (!CheckIfCanPlay(nextMethod))
                {
                    return false;
                }
            }

            return true;
        }
        else
        {
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
            "CARDSINHAND" => currentPlayer.myHand.Count >= 2,
            "EMPTYHAND" => currentPlayer.myHand.Count == 1,
            "NOENERGY" => currentPlayer.myEnergy == 0,
            "TARGETTED" => IsTargetted(),
            _ => true,
        };
    }

    bool IsTargetted()
    {
        foreach (GuardEntity guard in NewManager.instance.listOfGuards)
        {
            if (guard.CurrentTarget == currentPlayer && guard.stunned == 0)
                return true;
        }

        return false;
    }

    List<TileData> OccupiedAdjacent(TileData playerTile)
    {
        List<TileData> occupiedTiles = new();
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

#region Animations

    public void HideCard()
    {
        image.sprite = cardBack;
        canvasgroup.alpha = 0;
    }

    public IEnumerator RevealCard(float totalTime)
    {
        if (image.sprite != cardFront)
        {
            transform.localEulerAngles = new Vector3(0, 0, 0);
            float elapsedTime = 0f;

            Vector3 originalRot = this.transform.localEulerAngles;
            Vector3 newRot = new(0, 90, 0);

            while (elapsedTime < totalTime)
            {
                this.transform.localEulerAngles = Vector3.Lerp(originalRot, newRot, elapsedTime / totalTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            image.sprite = cardFront;
            canvasgroup.alpha = 1;
            elapsedTime = 0f;

            while (elapsedTime < totalTime)
            {
                this.transform.localEulerAngles = Vector3.Lerp(newRot, originalRot, elapsedTime / totalTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            this.transform.localEulerAngles = originalRot;
        }
    }

    public IEnumerator MoveCard(Vector2 newPos, Vector2 finalPos, Vector3 newRot, float waitTime)
    {
        float elapsedTime = 0;
        Vector2 originalPos = this.transform.localPosition;
        Vector3 originalRot = this.transform.localEulerAngles;

        while (elapsedTime < waitTime)
        {
            this.transform.localPosition = Vector2.Lerp(originalPos, newPos, elapsedTime / waitTime);
            this.transform.localEulerAngles = Vector3.Lerp(originalRot, newRot, elapsedTime / waitTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        this.transform.localPosition = finalPos;
    }

    public void EnableCard()
    {
        enableBorder = true;
        image.color = Color.white;
        button.interactable = true;
    }

    public void DisableCard()
    {
        enableBorder = false;
        image.color = Color.gray;
        button.interactable = false;
    }

    void FixedUpdate()
    {
        if (border != null && enableBorder)
            border.SetAlpha(NewManager.instance.opacity);
        else if (border != null && !enableBorder)
            border.SetAlpha(0);
    }

    public IEnumerator FadeAway(float totalTime)
    {
        float elapsedTime = 0;
        while (elapsedTime < totalTime)
        {
            this.image.SetAlpha(1f-(elapsedTime/totalTime));
            this.canvasgroup.alpha = 1f - (elapsedTime / totalTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        this.image.SetAlpha(0);
        this.canvasgroup.alpha = 0;

        StartCoroutine(Unfade());
    }

    IEnumerator Unfade()
    {
        yield return NewManager.Wait(5f);
        image.SetAlpha(0);
        canvasgroup.alpha = 1;
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
        currentTarget = currentPlayer.currentTile;

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
        NewManager.instance.selectedTile = currentPlayer.currentTile;
    }

    IEnumerator ResolveMethod(string methodName)
    {
        methodName = methodName.Replace("]", "").Trim();

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
                case "CHOOSEEXHAUST":
                    yield return ChooseExhaust(currentPlayer);
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
                case "ZEROENERGY":
                    yield return ZeroEnergy(currentPlayer);
                    break;
                case "CHANGEMP":
                    yield return ChangeMovement(currentPlayer);
                    break;
                case "ZEROMOVEMENT":
                    yield return ZeroMovement(currentPlayer);
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
                case "SWAPADJACENTGUARD":
                    Debug.Log("swapping adjacent guards");
                    yield return ChooseGuard();
                    yield return SwapGuard(adjacentTilesWithGuards[0].myEntity.GetComponent<GuardEntity>());
                    break;
                case "ATTACKADJACENTWALL":
                    yield return ChooseWall();
                    yield return AttackWall(adjacentTilesWithWalls[0].myEntity.GetComponent<WallEntity>());
                    break;
                case "CENTERDISTRACTION":
                    yield return CalculateDistraction(currentPlayer.currentTile);
                    break;
                case "TARGETDISTRACTION&DAMAGE":
                    yield return ChooseTile();
                    yield return AttackOrDistraction(currentTarget);
                    break;
                case "THROWENVIRONMENTAL":
                    yield return ChooseTile();
                    yield return CreateEnvironmental();
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
        //print("distracting for " + textDescr.text);
        //print("Intensity:" + distractionIntensity);
        List<TileData> affectedTiles = NewManager.instance.CalculateIntensity(source, distractionIntensity, true);
        //print(affectedTiles.Count);
        if (affectedTiles.Count > 0) addDistractionSound.Post(source.gameObject);
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

#region Choose Options

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

    IEnumerator ChooseTile()
    {
        List<TileData> tilesInRange = NewManager.instance.CalculateReachableGrids(currentPlayer.currentTile, range, false);
        NewManager.instance.UpdateInstructions("Choose a tile in range.");
        NewManager.instance.WaitForDecision(tilesInRange);
        while (NewManager.instance.chosenTile == null)
            yield return null;
        currentTarget = NewManager.instance.chosenTile;
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
        foreach (PlayerEntity player in listOfPlayers)
        {
            player.PlusCards(changeInDraw);
        }
        yield return null;
    }

    internal IEnumerator ChooseDiscard(PlayerEntity player)
    {
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

    internal IEnumerator ChooseExhaust(PlayerEntity player)
    {
        for (int i = 0; i < chooseHand; i++)
        {
            NewManager.instance.UpdateInstructions($"Exhaust a card from your hand ({chooseHand - i} more).");
            if (player.myHand.Count >= 2)
            {
                NewManager.instance.WaitForDecision(player.myHand);
                while (NewManager.instance.chosenCard == null)
                    yield return null;
                yield return player.ExhaustFromHand(NewManager.instance.chosenCard);
            }
            else if (player.myHand.Count == 1)
            {
                yield return player.ExhaustFromHand(player.myHand[0]);
            }
            NewManager.instance.UpdateStats(currentPlayer);
        }
    }

    internal IEnumerator DiscardHand(PlayerEntity player)
    {
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
        player.costChange.Add(this);
        yield return null;
    }

    internal IEnumerator ChangeCostTwoPlus(PlayerEntity player)
    {
        costChangeCondition = "COSTS 2+";
        yield return ChangeCost(player);
    }

    internal IEnumerator ChangeHealth(PlayerEntity player)
    {
        NewManager.instance.ChangeHealth(player, changeInHP);
        yield return null;
    }

    internal IEnumerator ChangeEnergy(PlayerEntity player)
    {
        NewManager.instance.ChangeEnergy(player, changeInEP);
        yield return null;
    }

    internal IEnumerator ZeroEnergy(PlayerEntity player)
    {
        NewManager.instance.SetEnergy(player, 0);
        yield return null;
    }

    internal IEnumerator ChangeMovement(PlayerEntity player)
    {
        NewManager.instance.ChangeMovement(player, changeInMP);
        yield return null;
    }

    internal IEnumerator ZeroMovement(PlayerEntity player)
    {
        NewManager.instance.SetMovement(player, 0);
        yield return null;
    }

#endregion

#region Interacts With Entities

    IEnumerator AttackOrDistraction(TileData target)
    {
        if (target.myEntity != null)
        {
            if (target.myEntity.CompareTag("Enemy"))
            {
                Debug.Log("stunned guard");
                yield return StunGuard(target.myEntity.GetComponent<GuardEntity>());
            }
        }
        else
        {
            yield return CalculateDistraction(target);
        }
    }

    internal IEnumerator AttackWall(WallEntity wall)
    {
        wall.AffectWall(changeInWall);
        currentTarget = wall.currentTile;
        yield return null;
    }

    IEnumerator SwapGuard(GuardEntity guard)
    {
        TileData guardsOriginalTile = guard.currentTile;
        TileData playersOriginalTile = currentPlayer.currentTile;

        currentPlayer.MoveTile(guardsOriginalTile);
        guard.MoveTile(playersOriginalTile);
        yield return NewManager.Wait(0.2f);
    }

    internal IEnumerator StunGuard(GuardEntity guard)
    {
        yield return null;
        guard.stunSound.Post(guard.gameObject);
        guard.DetectionRangePatrol = 0;
        guard.stunned += stunDuration;
<<<<<<< HEAD
=======
        guard.CalculateTiles();
>>>>>>> main
        currentTarget = guard.currentTile;
    }

    IEnumerator CreateEnvironmental()
    {
        EnvironmentalEntity newEnviro = NewManager.instance.CreateEnvironmental();
        newEnviro.currentTile = currentTarget;
<<<<<<< HEAD
=======
        newEnviro.spriteRenderer.sortingOrder = 10;
        newEnviro.transform.SetParent(newEnviro.currentTile.transform);
        newEnviro.transform.localPosition = new Vector3(0, 1, 0);
>>>>>>> main
        newEnviro.name = this.name;
        newEnviro.card = this;
        newEnviro.delay = delay;
        NewManager.instance.listOfEnvironmentals.Add(newEnviro);
        yield return null;
    }

#endregion

}