using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using System.Linq;

public class Card : MonoBehaviour, IPointerClickHandler
{

#region Variables

    [Foldout("Choices", true)]
        [ReadOnly] public Image image;
        [ReadOnly] public Image background;
        bool enableBorder;
        [ReadOnly] public Image border;
        [ReadOnly] public Button button;
        [SerializeField] Collector buttonCollector;
        [SerializeField] SliderChoice sliderChoice;
        [Tooltip("the front of a card")][SerializeField] Sprite cardFront;
        [Tooltip("the back of a card")][SerializeField] Sprite cardBack;

    [Foldout("Texts", true)]
        public CanvasGroup canvasgroup;
        public TMP_Text textName;
        public TMP_Text textCost;
        public TMP_Text textDescr;

    [Foldout("Card Presentation", true)]
        public Color AttackColor = Color.red;
        public Color DrawColor = Color.green;
        public Color DistractionColor = Color.blue;
        public Color EnergyColor = Color.cyan;
        public Color MovementColor = Color.yellow;
        public Color MiscColor = Color.gray;
        public Color FallbackColor = Color.white;
        private Material material;
        private MaterialPropertyBlock materialPropertyBlock;

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
        [ReadOnly] public int changeInWall { get; private set; }
        [ReadOnly] public int volumeIntensity { get; private set; }
        [ReadOnly] public int vision { get; private set; }

        [ReadOnly] string selectCondition;
        [ReadOnly] public string effectsInOrder{ get; private set; }
        [ReadOnly] public string enviroEffect { get; private set; }
        [ReadOnly] public string nextRoundEffectsInOrder { get; private set; }
        [ReadOnly] public string costChangeCondition{ get; private set; }

    [Foldout("Saved information", true)]
        [ReadOnly] int sliderData;
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
        background = transform.Find("Canvas Group").Find("Background").GetComponent<Image>();
        border = transform.GetChild(0).GetComponent<Image>();
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
            CardDisplay.instance.ChangeCard(this);
            cardMove.Post(gameObject);
        }
    }

    public void CardSetup(CardData data)
    {
        textName.text = data.name;
        textDescr.text = ChangeAllKeywords(data.desc);

        typeOne = ConvertToType(data.cat1);
        typeTwo = ConvertToType(data.cat2);

        Material mat = new Material(image.material);
        mat.SetColor("_GradientColorTop", ConvertToColor(typeOne));
        mat.SetColor("_GradientColorBottom", ConvertToColor(typeTwo));
        mat.SetTexture("_MainTex", image.mainTexture);
        image.material = mat;

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
        volumeIntensity = data.volume;
        vision = data.vision;

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

    public Color ConvertToColor(CardType cardType)
    {
        return cardType switch
        {
            CardType.Attack => Color.red,
            CardType.Draw => Color.green,
            CardType.Distraction => Color.blue,
            CardType.Energy => Color.cyan,
            CardType.Movement => Color.yellow,
            CardType.Misc => Color.gray,
            CardType.None => ConvertToColor(typeOne),
            _ => Color.black,
        };
    }

    string ChangeAllKeywords(string cardText)
    {
        cardText = $"<color=#000000>{cardText}";

        cardText = BoldKeyword(cardText, @"[-+]\d+ Cards", "000000");
        cardText = BoldKeyword(cardText, @"[-+]\d+ Card", "000000");

        cardText = ChangeToSymbol(cardText, "Energy", $"\"Symbols\" name=\"Energy\"");
        cardText = ChangeToSymbol(cardText, "EP", $"\"Symbols\" name=\"Energy\"");

        cardText = ChangeToSymbol(cardText, "Movement", $"\"Symbols\" name=\"Movement\"");
        cardText = ChangeToSymbol(cardText, "MP", $"\"Symbols\" name=\"Movement\"");

        cardText = ChangeToSymbol(cardText, "Health", $"\"Symbols\" name=\"Health\"");
        cardText = ChangeToSymbol(cardText, "HP", $"\"Symbols\" name=\"Health\"");

        cardText = BoldKeyword(cardText, @"\d+ Damage", "8B0000");
        cardText = BoldKeyword(cardText, @"Intensity \d+", "FFA500");
        cardText = BoldKeyword(cardText, @"Range \d+", "0000FF");
        cardText = BoldKeyword(cardText, @"Stun \d+", "800080");
        cardText = BoldKeyword(cardText, @"Delay \d+", "00FFFF");
        
        return cardText;
    }

    string ChangeToSymbol(string cardText, string keyword, string symbol)
    {
        return cardText.Replace(keyword, $"<sprite={symbol}>");
    }

    string BoldKeyword(string cardText, string keyword, string color)
    {
        MatchCollection matches = Regex.Matches(cardText, keyword);
        foreach (Match match in matches.Cast<Match>())
        {
            string replacement = $"<color=#{color}><b>{match.Value}</b><color=#000000>";
            cardText = cardText.Replace(match.Value, replacement);
        }

        return cardText;
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

        if (changedEnergyCost < 0)
            changedEnergyCost = 0;

        textCost.text = $"{changedEnergyCost}";
        return changedEnergyCost;
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
            "INDISCARD" => (currentPlayer.myDiscardPile.Count > 0),
            "ISPLAYER" => SearchAdjacentPlayers(currentPlayer.currentTile, true).Count > 0,
            "ISDIFFERENTPLAYER" => SearchAdjacentPlayers(currentPlayer.currentTile, false).Count > 0,
            "ISGUARD" => SearchAdjacentGuard(currentPlayer.currentTile).Count > 0,
            "ISWALL" => SearchAdjacentWall(currentPlayer.currentTile).Count > 0,
            "ISUNOCCUPIED" => UnoccupiedAdjacent(currentPlayer.currentTile).Count > 0,
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

    List<TileData> UnoccupiedAdjacent(TileData playerTile)
    {
        List<TileData> occupiedTiles = new();
        List<TileData> tilesInRange = NewManager.instance.CalculateReachableGrids(playerTile, range, false);

        foreach (TileData tile in tilesInRange)
        {
            if (tile.myEntity == null)
                occupiedTiles.Add(tile);
        }
        return occupiedTiles;

    }

    List<TileData> SearchAdjacentPlayers(TileData playerTile, bool includeSelf)
    {
        List<TileData> tilesInRange = NewManager.instance.CalculateReachableGrids(playerTile, range, false);
        List<TileData> playersInRange = new();

        foreach (TileData tile in tilesInRange)
        {
            if (tile.myEntity != null && tile.myEntity.CompareTag("Player"))
            {
                Debug.Log(tile.myEntity.name);
                playersInRange.Add(tile);
            }
        }

        if (!includeSelf)
            playersInRange.Remove(currentPlayer.currentTile);

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
        //canvasgroup.alpha = 0;
    }

    public IEnumerator RevealCard(float totalTime)
    {
        if (image.sprite != cardFront)
        {
            transform.localEulerAngles = new Vector3(0, 0, 0);
            float elapsedTime = 0f;

            Vector3 originalRot = transform.localEulerAngles;
            Vector3 newRot = new(0, 90, 0);

            while (elapsedTime < totalTime)
            {
                transform.localEulerAngles = Vector3.Lerp(originalRot, newRot, elapsedTime / totalTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            image.sprite = cardFront;
            //canvasgroup.alpha = 1;
            elapsedTime = 0f;

            while (elapsedTime < totalTime)
            {
                transform.localEulerAngles = Vector3.Lerp(newRot, originalRot, elapsedTime / totalTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.localEulerAngles = originalRot;
        }
    }

    public IEnumerator MoveCard(Vector3 newPos, Vector3 finalPos, Vector3 newRot, float waitTime)
    {
        float elapsedTime = 0;
        Vector3 originalPos = transform.localPosition;
        Vector3 originalRot = transform.localEulerAngles;

        while (elapsedTime < waitTime)
        {
            transform.localPosition = Vector3.Lerp(originalPos, newPos, elapsedTime / waitTime);
            transform.localEulerAngles = Vector3.zero;
            transform.localEulerAngles = Vector3.Lerp(originalRot, new Vector3(0, 0, newRot.z), elapsedTime / waitTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = finalPos;
    }

    public void EnableCard()
    {
        image.color = Color.white;
        background.color = Color.white;
        enableBorder = true;
        button.interactable = true;
    }

    public void DisableCard()
    {
        image.color = Color.gray;
        background.color = Color.gray;
        enableBorder = false;
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

        StartCoroutine(Unfade(3f));
    }

    public IEnumerator Unfade(float time)
    {
        yield return NewManager.Wait(time);
        this.image.SetAlpha(1);
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
            NewManager.instance.UpdateStats(currentPlayer);

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
        NewManager.instance.violentCards += (violent) ? 1 : 0;
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
                case "DAMAGEDRAW":
                    foreach (PlayerEntity nextPlayer in NewManager.instance.listOfPlayers)
                    {
                        if (nextPlayer.damageTaken > 0)
                            yield return DrawCards(nextPlayer);
                    }
                    break;
                case "DRAWCARDS":
                    yield return DrawCards(currentPlayer);
                    break;
                case "PICKUPCARD":
                    yield return PickupCard(currentPlayer);
                    break;
                case "ALLDRAWCARDS":
                    yield return AllDrawCards(NewManager.instance.listOfPlayers);
                    break;
                case "FINDATTACK":
                    yield return FindCard(currentPlayer, CardType.Attack);
                    break;
                case "FINDMOVEMENT":
                    yield return FindCard(currentPlayer, CardType.Movement);
                    break;

                case "CHOOSEDISCARD":
                    yield return ChooseDiscard(currentPlayer);
                    break;
                case "PASSTOPLAYER":
                    yield return ChoosePlayer();
                    yield return PassCard(currentPlayer, adjacentTilesWithPlayers[0].myEntity.GetComponent<PlayerEntity>());
                    currentTarget = currentPlayer.currentTile;
                    break;
                /*case "CHOOSEEXHAUST":
                               yield return ChooseExhaust(currentPlayer);
                               break;*/
                case "DISCARDHAND":
                    yield return DiscardHand(currentPlayer);
                    break;

                case "CHANGEHP":
                    yield return ChangeHealth(currentPlayer);
                    break;
                case "CHANGEADJACENTHP":
                    yield return ChoosePlayer();
                    PlayerEntity player = adjacentTilesWithPlayers[0].myEntity.GetComponent<PlayerEntity>();
                    currentTarget = currentPlayer.currentTile;
                    yield return ChangeHealth(player);
                    break;

                case "CHANGEEP":
                    yield return ChangeEnergy(currentPlayer);
                    break;
                case "MAXENERGY":
                    yield return MaxEnergy(currentPlayer);
                    break;
                case "ZEROENERGY":
                    yield return ZeroEnergy(currentPlayer);
                    break;
                case "CONVERTMOVEMENTTOENERGY":
                    yield return ChooseFromSlider("Pay how much movement?", 0, currentPlayer.movementLeft);
                    changeInMP = -1 * sliderData;
                    yield return ChangeMovement(currentPlayer);
                    changeInEP = sliderData;
                    yield return ChangeEnergy(currentPlayer);
                    break;

                case "CHANGEMP":
                    yield return ChangeMovement(currentPlayer);
                    break;
                case "MAXMOVEMENT":
                    yield return MaxMovement(currentPlayer);
                    break;
                case "ZEROMOVEMENT":
                    yield return ZeroMovement(currentPlayer);
                    break;

                case "CHANGECOST":
                    yield return ChangeCost(currentPlayer);
                    break;
                case "CHANGECOSTTWOPLUS":
                    yield return ChangeCostTwoPlus();
                    break;

                case "STUNADJACENTGUARD":
                    yield return ChooseGuard();
                    yield return StunGuard(adjacentTilesWithGuards[0].myEntity.GetComponent<GuardEntity>());
                    break;

                case "ATTACKADJACENTWALL":
                    yield return ChooseWall();
                    yield return AttackWall(adjacentTilesWithWalls[0].myEntity.GetComponent<WallEntity>());
                    break;

                case "THROWNBOTTLE":
                    yield return ChooseTileLOS();
                    if (currentTarget.myEntity.CompareTag("Guard"))
                        yield return CalculateDistraction(currentPlayer.currentTile);
                    else
                        yield return CalculateDistraction(currentTarget);
                    break;
                case "CHOOSEDISTRACTION":
                    yield return ChooseTileLOS();
                    yield return CalculateDistraction(currentTarget);
                    break;
                case "TARGETDISTRACTION":
                    yield return CalculateDistraction(currentTarget);
                    break;
                case "CENTERDISTRACTION":
                    yield return CalculateDistraction(currentPlayer.currentTile);
                    break;

                /*case "TARGETDISTRACTION&DAMAGE":
                    yield return ChooseTileLOS();
                    yield return AttackOrDistraction(currentTarget);
                    break;*/
                case "THROWENVIRONMENTAL":
                    yield return ChooseTile();
                    yield return CreateEnvironmental();
                    break;

                default:
                    Debug.LogError($"{methodName} isn't a method");
                    yield return null;
                    break;
            }
        }
    }

    public IEnumerator CalculateDistraction(TileData source)
    {
        //print("distracting for " + textDescr.text);
        //print("Intensity:" + distractionIntensity);
        List<TileData> affectedTiles = NewManager.instance.CalculateIntensity(source, volumeIntensity, true);
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
        if (adjacentTilesWithGuards.Count != 1)
        {
            NewManager.instance.UpdateInstructions("Choose a guard in range.");
            NewManager.instance.WaitForDecision(adjacentTilesWithGuards);

            while (NewManager.instance.chosenTile == null)
                yield return null;

            Collector confirmDecision = NewManager.instance.ConfirmDecision("Confirm guard?", new Vector2(0, 200));
            if (confirmDecision != null)
            {
                NewManager.instance.selectedTile = NewManager.instance.chosenTile;
                yield return confirmDecision.WaitForChoice();
                int decision = confirmDecision.chosenButton;
                Destroy(confirmDecision.gameObject);

                if (decision == 1)
                {
                    yield return (ChooseGuard());
                    yield break;
                }
            }

            adjacentTilesWithGuards.Clear();
            adjacentTilesWithGuards.Add(NewManager.instance.chosenTile);
        }

    }

    IEnumerator ChoosePlayer()
    {
        if (adjacentTilesWithPlayers.Count != 1)
        {
            NewManager.instance.UpdateInstructions("Choose a player in range.");
            NewManager.instance.WaitForDecision(adjacentTilesWithPlayers);

            while (NewManager.instance.chosenTile == null)
                yield return null;

            Collector confirmDecision = NewManager.instance.ConfirmDecision("Confirm player?", new Vector2(0, 200));
            if (confirmDecision != null)
            {
                NewManager.instance.selectedTile = NewManager.instance.chosenTile;
                yield return confirmDecision.WaitForChoice();
                int decision = confirmDecision.chosenButton;
                Destroy(confirmDecision.gameObject);

                if (decision == 1)
                {
                    yield return (ChoosePlayer());
                    yield break;
                }
            }

            adjacentTilesWithPlayers.Clear();
            adjacentTilesWithPlayers.Add(NewManager.instance.chosenTile);
        }
    }

    IEnumerator ChooseWall()
    {
        if (adjacentTilesWithWalls.Count != 1)
        {
            NewManager.instance.UpdateInstructions("Choose a wall in range.");
            NewManager.instance.WaitForDecision(adjacentTilesWithWalls);

            while (NewManager.instance.chosenTile == null)
                yield return null;

            Collector confirmDecision = NewManager.instance.ConfirmDecision("Confirm wall?", new Vector2(0, 200));
            if (confirmDecision != null)
            {
                NewManager.instance.selectedTile = NewManager.instance.chosenTile;
                yield return confirmDecision.WaitForChoice();
                int decision = confirmDecision.chosenButton;
                Destroy(confirmDecision.gameObject);

                if (decision == 1)
                {
                    yield return (ChooseWall());
                    yield break;
                }
            }

            adjacentTilesWithWalls.Clear();
            adjacentTilesWithWalls.Add(NewManager.instance.chosenTile);
        }
    }

    //selects a tile, using line of sight around the player,
    IEnumerator ChooseTileLOS()
    {
        List<HashSet<Vector2Int>> DetectLines = new List<HashSet<Vector2Int>>();
        HashSet<Vector2Int> tilesInRange = new HashSet<Vector2Int>();

        for (int i = 0; i < 360; i += 5)
        {
            float lineAngle = (i * Mathf.Deg2Rad);
            Vector2 newVector = new Vector2(Mathf.Cos(lineAngle), Mathf.Sin(lineAngle));
            DetectLines.Add(NewManager.instance.line(currentPlayer.currentTile.gridPosition, currentPlayer.currentTile.gridPosition + Vector2Int.RoundToInt(newVector.normalized * range)));
        }
        for (int i = 0; i < DetectLines.Count; i++)
        {
            foreach (Vector2Int point in DetectLines[i])
            {
                TileData TileToAdd = NewManager.instance.FindTile(point);
                if (TileToAdd == null)
                {
                    break;
                }
                if (TileToAdd.myEntity != null)
                {
                    if (TileToAdd.myEntity.Occlusion && point != currentPlayer.currentTile.gridPosition)
                    {
                        break;
                    }
                }
                tilesInRange.Add(point);
            }
        }
        List<TileData> tilesToSelect = new List<TileData>();
        foreach (Vector2Int point in tilesInRange)
        {
            tilesToSelect.Add(NewManager.instance.FindTile(point));
        }

        NewManager.instance.UpdateInstructions("Choose a tile in range.");
        NewManager.instance.WaitForDecision(tilesToSelect);

        while (NewManager.instance.chosenTile == null)
            yield return null;

        Collector confirmDecision = NewManager.instance.ConfirmDecision("Confirm tile?", new Vector2(0, 200));
        if (confirmDecision != null)
        {
            NewManager.instance.selectedTile = NewManager.instance.chosenTile;
            yield return confirmDecision.WaitForChoice();
            int decision = confirmDecision.chosenButton;
            Destroy(confirmDecision.gameObject);

            if (decision == 1)
            {
                yield return (ChooseTileLOS());
                yield break;
            }
        }

        currentTarget = NewManager.instance.chosenTile;
    }

    IEnumerator ChooseTile()
    {
        List<TileData> tilesInRange = NewManager.instance.CalculateReachableGrids(currentPlayer.currentTile, range, false);
        NewManager.instance.UpdateInstructions("Choose a tile in range.");
        NewManager.instance.WaitForDecision(tilesInRange);

        while (NewManager.instance.chosenTile == null)
            yield return null;

        Collector confirmDecision = NewManager.instance.ConfirmDecision("Confirm tile?", new Vector2(0, 200));
        if (confirmDecision != null)
        {
            NewManager.instance.selectedTile = NewManager.instance.chosenTile;
            yield return confirmDecision.WaitForChoice();
            int decision = confirmDecision.chosenButton;
            Destroy(confirmDecision.gameObject);

            if (decision == 1)
            {
                yield return (ChooseTile());
                yield break;
            }
        }

        currentTarget = NewManager.instance.chosenTile;
    }

    IEnumerator ChooseFromSlider(string header, int min, int max)
    {
        SliderChoice newCollector = Instantiate(sliderChoice);
        newCollector.StatsSetup(header, min, max, new Vector3(0, 0, 0));

        while (newCollector.makingDecision)
            yield return null;

        sliderData = newCollector.currentSliderValue;
        Destroy(newCollector.gameObject);
    }

    IEnumerator ChooseOptions(string[] choices)
    {
        Collector newCollector = Instantiate(buttonCollector);
        newCollector.StatsSetup("Make a choice.", new Vector2(0, 0));

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

        yield return newCollector.WaitForChoice();
        yield return ResolveMethod(choices[newCollector.chosenButton]);
        Destroy(newCollector.gameObject);
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

                Collector confirmDecision = NewManager.instance.ConfirmDecision($"Discard {NewManager.instance.chosenCard.name}?", new Vector2(0, -85));
                if (confirmDecision != null)
                {
                    yield return confirmDecision.WaitForChoice();
                    int decision = confirmDecision.chosenButton;
                    Destroy(confirmDecision.gameObject);

                    if (decision == 1)
                    {
                        i--;
                        continue;
                    }
                }

                yield return player.DiscardFromHand(NewManager.instance.chosenCard);
            }
            else if (player.myHand.Count == 1)
            {
                yield return player.DiscardFromHand(player.myHand[0]);
            }
            NewManager.instance.UpdateStats(currentPlayer);
        }
    }

    internal IEnumerator PassCard(PlayerEntity thisPlayer, PlayerEntity otherPlayer)
    {
        for (int i = 0; i < chooseHand; i++)
        {
            NewManager.instance.UpdateInstructions($"Give {otherPlayer.name} a card from your hand.");
            if (thisPlayer.myHand.Count >= 2)
            {
                NewManager.instance.WaitForDecision(thisPlayer.myHand);
                while (NewManager.instance.chosenCard == null)
                    yield return null;

                Collector confirmDecision = NewManager.instance.ConfirmDecision($"Pass {NewManager.instance.chosenCard.name}?", new Vector2(0, -85));
                if (confirmDecision != null)
                {
                    yield return confirmDecision.WaitForChoice();
                    int decision = confirmDecision.chosenButton;
                    Destroy(confirmDecision.gameObject);

                    if (decision == 1)
                    {
                        i--;
                        continue;
                    }
                }

                otherPlayer.PlusCards(NewManager.instance.chosenCard);
            }
            else if (thisPlayer.myHand.Count == 1)
            {
                otherPlayer.PlusCards(thisPlayer.myHand[0]);
            }
            thisPlayer.SortHand();
            NewManager.instance.UpdateStats(currentPlayer);
        }
    }

    /*
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

                Collector confirmDecision = NewManager.instance.ConfirmDecision($"Exhaust {NewManager.instance.chosenCard.name}?", new Vector2(0, -85));
                if (confirmDecision != null)
                {
                    yield return confirmDecision.WaitForChoice();
                    int decision = confirmDecision.chosenButton;
                    Destroy(confirmDecision.gameObject);

                    if (decision == 1)
                    {
                        i--;
                        continue;
                    }
                }

                yield return player.ExhaustFromHand(NewManager.instance.chosenCard);
            }
            else if (player.myHand.Count == 1)
            {
                yield return player.ExhaustFromHand(player.myHand[0]);
            }
            NewManager.instance.UpdateStats(currentPlayer);
        }
    }
    */

#endregion

#region Interacts With Cards

    internal IEnumerator DrawCards(PlayerEntity player)
    {
        player.PlusCards(changeInDraw);      
        yield return null;
    }

    internal IEnumerator PickupCard(PlayerEntity player)
    {
        player.PlusCards(player.myDiscardPile[^2]);
        yield return null;
    }

    internal IEnumerator AllDrawCards(List<PlayerEntity> listOfPlayers)
    {
        foreach (PlayerEntity player in listOfPlayers)
        {
            yield return DrawCards(player);
        }
    }

    internal IEnumerator DiscardHand(PlayerEntity player)
    {
        while (player.myHand.Count > 0)
        {
            StartCoroutine(player.DiscardFromHand(player.myHand[0]));
        }
        yield return NewManager.Wait(PlayerPrefs.GetFloat("Animation Speed"));
    }

    internal IEnumerator FindCard(PlayerEntity player, CardType targetType)
    {
        yield return null;
        List<Card> shuffledBack = new();
        Card foundCard = null;
        foreach (Card card in player.myDrawPile)
        {
            if (card.typeOne == targetType || card.typeTwo == targetType)
            {
                foundCard = card;
                break;
            }
            else
            {
                shuffledBack.Add(card);
            }
        }

        if (foundCard == null)
            player.PlusCards(changeInDraw);
        else
            player.PlusCards(foundCard);
        player.myDrawPile.Shuffle();
    }

    #endregion

#region Interacts With Stats

    internal IEnumerator ChangeCost(PlayerEntity player)
    {
        player.costChange.Add(this);
        yield return null;
    }

    internal IEnumerator ChangeCostTwoPlus()
    {
        costChangeCondition = "COSTS 2+";
        foreach (PlayerEntity player in NewManager.instance.listOfPlayers)
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

    internal IEnumerator MaxEnergy(PlayerEntity player)
    {
        player.maxEnergy += changeInEP;
        NewManager.instance.UpdateStats(player);
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

    internal IEnumerator MaxMovement(PlayerEntity player)
    {
        player.movesPerTurn += changeInMP;
        NewManager.instance.UpdateStats(player);
        yield return null;
    }

    internal IEnumerator ZeroMovement(PlayerEntity player)
    {
        NewManager.instance.SetMovement(player, 0);
        yield return null;
    }

#endregion

#region Interacts With Entities

    /*
    internal IEnumerator AttackOrDistraction(TileData target)
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
    */
    internal IEnumerator AttackWall(WallEntity wall)
    {
        if (wall != null)
        {
            wall.AffectWall(changeInWall);
            currentTarget = wall.currentTile;
        }
        yield return null;
    }

    /*internal IEnumerator SwapGuard(GuardEntity guard)
    {
        TileData guardsOriginalTile = guard.currentTile;
        TileData playersOriginalTile = currentPlayer.currentTile;

        currentPlayer.MoveTile(guardsOriginalTile);
        guard.MoveTile(playersOriginalTile);
        yield return NewManager.Wait(0.2f);
    }*/

    internal IEnumerator StunPlayer(PlayerEntity player)
    {
        yield return null;
        //guard.stunSound.Post(guard.gameObject);
        //guard.DetectionRangePatrol = 0;
        player.stunned += stunDuration;
        //guard.CalculateTiles();
        //currentTarget = guard.currentTile;
    }

    internal IEnumerator StunGuard(GuardEntity guard)
    {
        yield return null;
        guard.stunSound.Post(guard.gameObject);
        guard.DetectionRangePatrol = 0;
        guard.stunned += stunDuration;
        guard.CalculateTiles();
        currentTarget = guard.currentTile;
    }

    internal IEnumerator CreateEnvironmental()
    {
        EnvironmentalEntity newEnviro = NewManager.instance.CreateEnvironmental();
        newEnviro.currentTile = currentTarget;
        newEnviro.spriteRenderer.sortingOrder = 10;
        newEnviro.transform.SetParent(newEnviro.currentTile.transform);
        newEnviro.transform.localPosition = new Vector3(0, 1, 0);
        newEnviro.name = this.name;
        newEnviro.card = this;
        newEnviro.delay = delay;
        NewManager.instance.listOfEnvironmentals.Add(newEnviro);
        yield return null;
    }

    internal IEnumerator AffectGuardMovement(GuardEntity guard)
    {
        yield return null;
        guard.movementLeft += changeInMP;
    }

#endregion

}