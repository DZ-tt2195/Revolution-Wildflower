using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using UnityEngine.EventSystems;
using System;

public class Card : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
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
    [SerializeField] Color AttackColor = Color.red;
    [SerializeField] Color DrawColor = Color.green;
    [SerializeField] Color DistractionColor = Color.blue;
    [SerializeField] Color EnergyColor = Color.cyan;
    [SerializeField] Color MovementColor = Color.yellow;
    [SerializeField] Color MiscColor = Color.gray;
    [SerializeField] Color FallbackColor = Color.white;
    private Material material;
    private MaterialPropertyBlock materialPropertyBlock;
    bool mouseOver = false;
    float growthTimer = 0;
    Vector3 cardSize;
    [SerializeField] AnimationCurve growthCurve;
    [SerializeField] AnimationCurve moveCurve;
    private Canvas canvas;
    [SerializeField] private float animationSpeed;
    [SerializeField] private float moveAmount = 250;
    [SerializeField] private float growthAmount;

    [Foldout("Types", true)]
    [SerializeField] Sprite attackSprite;
    [SerializeField] Sprite distractSprite;
    [SerializeField] Sprite drawSprite;
    [SerializeField] Sprite energySprite;
    [SerializeField] Sprite moveSprite;
    public Image typeOneSprite { get; private set; }
    public Image typeTwoSprite { get; private set; }

    public enum CardType { Attack, Draw, Distraction, Energy, Movement, Misc, None };
    [Foldout("Card stats", true)]
    [ReadOnly] public int energyCost;
    [ReadOnly] public CardType typeOne { get; private set; }
    [ReadOnly] public CardType typeTwo { get; private set; }
    [ReadOnly] public string costChangeCondition { get; private set; }

    [Foldout("Saved information", true)]
    [ReadOnly] public CardData data;
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

    private EventHandler[] events;

    public static event EventHandler OnCardResolved;
    public static event EventHandler OnChoiceMade;

    #endregion

#region Setup

    private void Awake()
    {
        cardSize = transform.localScale;
        image = GetComponent<Image>();
        background = transform.Find("Canvas Group").Find("Background").GetComponent<Image>();
        border = transform.GetChild(0).GetComponent<Image>();
        button = this.GetComponent<Button>();
        button.onClick.AddListener(SendMe);
        typeOneSprite = transform.Find("Canvas Group").Find("Card Type 1").GetComponent<Image>();
        typeTwoSprite = transform.Find("Canvas Group").Find("Card Type 2").GetComponent<Image>();
        canvas = GetComponent<Canvas>();
        if (GameObject.Find("Camera (1)"))
        {
            GameObject.Find("Camera (1)").TryGetComponent<Camera>(out Camera cam);
            canvas.worldCamera = cam;
            canvas.overrideSorting = true;
        }
    }

    void SendMe()
    {
        PhaseManager.instance.ReceiveChoice(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            CardDisplay.instance.ChangeCard(this);
            cardMove.Post(gameObject);
        }
    }

    public void OnPointerEnter(PointerEventData data)
    {
        mouseOver = true;
    }

    public void OnPointerExit(PointerEventData data)
    {
        mouseOver = false;
    }

    public void CardSetup(CardData data)
    {
        this.data = data;
        textName.text = data.name;
        textDescr.text = KeywordTooltip.instance.EditText(data.desc);

        typeOne = ConvertToType(data.cat1);
        switch (typeOne)
        {
            case CardType.Attack:
                typeOneSprite.sprite = attackSprite;
                break;
            case CardType.Draw:
                typeOneSprite.sprite = drawSprite;
                break;
            case CardType.Distraction:
                typeOneSprite.sprite = distractSprite;
                break;
            case CardType.Energy:
                typeOneSprite.sprite = energySprite;
                break;
            case CardType.Movement:
                typeOneSprite.sprite = moveSprite;
                break;
            default:
                typeOneSprite.gameObject.SetActive(false);
                break;
        }
        typeTwo = ConvertToType(data.cat2);
        switch (typeTwo)
        {
            case CardType.Attack:
                typeTwoSprite.sprite = attackSprite;
                break;
            case CardType.Draw:
                typeTwoSprite.sprite = drawSprite;
                break;
            case CardType.Distraction:
                typeTwoSprite.sprite = distractSprite;
                break;
            case CardType.Energy:
                typeTwoSprite.sprite = energySprite;
                break;
            case CardType.Movement:
                typeTwoSprite.sprite = moveSprite;
                break;
            default:
                typeTwoSprite.gameObject.SetActive(false);
                break;
        }

        Material mat = new Material(image.material);
        mat.SetColor("_GradientColorTop", ConvertToColor(typeOne));
        mat.SetColor("_GradientColorBottom", ConvertToColor(typeTwo));
        mat.SetTexture("_MainTex", image.mainTexture);
        image.material = mat;

        energyCost = data.epCost;
        textCost.text = $"{data.epCost}";
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
            string divide = data.select.Replace(" ", "");
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
        foreach (GuardEntity guard in LevelGenerator.instance.listOfGuards)
        {
            if (guard.CurrentTarget == currentPlayer && guard.stunned == 0)
                return true;
        }

        return false;
    }

    List<TileData> OccupiedAdjacent(TileData playerTile)
    {
        List<TileData> occupiedTiles = new();
        List<TileData> tilesInRange = Pathfinder.instance.CalculateReachableGrids(playerTile, data.range, false);

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
        List<TileData> tilesInRange = Pathfinder.instance.CalculateReachableGrids(playerTile, data.range, false);

        foreach (TileData tile in tilesInRange)
        {
            if (tile.myEntity == null)
                occupiedTiles.Add(tile);
        }
        return occupiedTiles;

    }

    List<TileData> SearchAdjacentPlayers(TileData playerTile, bool includeSelf)
    {
        List<TileData> tilesInRange = Pathfinder.instance.CalculateReachableGrids(playerTile, data.range, false);
        List<TileData> playersInRange = new();

        foreach (TileData tile in tilesInRange)
        {
            if (tile.myEntity != null && tile.myEntity.CompareTag("Player"))
            {
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
        List<TileData> tilesInRange = Pathfinder.instance.CalculateReachableGrids(playerTile, data.range, false);
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
        List<TileData> tilesInRange = Pathfinder.instance.CalculateReachableGrids(playerTile, data.range, false);
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

    public void Update()
    {
        if (this.transform.localPosition.y >= SaveManager.instance.cardBaseHeight)
        {
            if (mouseOver)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -1);
                if (growthTimer + Time.deltaTime < growthCurve.keys[^1].time) growthTimer += Time.deltaTime * animationSpeed;
                else growthTimer = growthCurve.keys[^1].time;
                canvas.sortingOrder = 1;
            }
            else
            {
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
                if (growthTimer - Time.deltaTime > 0) growthTimer -= Time.deltaTime * animationSpeed;
                else growthTimer = 0;
                canvas.sortingOrder = 0;
            }
            float sizeValue = growthCurve.Evaluate(growthTimer);
            float positionValue = moveCurve.Evaluate(growthTimer);
            transform.localScale = Vector3.Lerp(cardSize, cardSize * growthAmount, sizeValue);
            transform.localPosition = new Vector3(transform.localPosition.x, SaveManager.instance.cardBaseHeight + Mathf.Lerp(0, moveAmount, positionValue), transform.localPosition.z);
        }
    }

    public void HideCard()
    {
        image.sprite = cardBack;
        canvasgroup.alpha = 0;
    }

    public IEnumerator RevealCard(float totalTime)
    {
        if (image.sprite != cardFront)
        {
            transform.localEulerAngles = Vector3.zero;
            float elapsedTime = 0f;
            Vector3 newRot = new(0, 90, 0);

            while (elapsedTime < totalTime)
            {
                Vector3 nextStep = Vector3.Lerp(Vector3.zero, newRot, elapsedTime / totalTime);
                transform.localEulerAngles = nextStep;
                Debug.Log($"{nextStep} - {transform.localEulerAngles}");

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            image.sprite = cardFront;
            canvasgroup.alpha = 1;
            elapsedTime = 0f;

            while (elapsedTime < totalTime)
            {
                Vector3 nextStep = Vector3.Lerp(newRot, Vector3.zero, elapsedTime / totalTime);
                transform.localEulerAngles = nextStep;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.localEulerAngles = Vector3.zero;
        }
    }

    public IEnumerator MoveCard(Vector3 newPos, Vector3 finalPos, float waitTime)
    {
        float elapsedTime = 0;
        Vector3 originalPos = transform.localPosition;

        while (elapsedTime < waitTime)
        {
            transform.localPosition = Vector3.Lerp(originalPos, newPos, elapsedTime / waitTime);
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
            border.SetAlpha(LevelUIManager.instance.opacity);
        else if (border != null && !enableBorder)
            border.SetAlpha(0);
    }

    public IEnumerator FadeAway(float totalTime)
    {
        float elapsedTime = 0;
        while (elapsedTime < totalTime)
        {
            this.image.SetAlpha(1f - (elapsedTime / totalTime));
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
        yield return new WaitForSeconds(time);
        this.image.SetAlpha(1);
        canvasgroup.alpha = 1;
    }

    #endregion

#region Play Effect

    public int CostChanger(int defaultCost)
    {
        return costChangeCondition switch
        {
            "COSTS 2+" => (defaultCost >= 2) ? data.chEP : 0,
            _ => data.chEP,
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
            LevelGenerator.instance.DisableAllTiles();
            LevelGenerator.instance.DisableAllCards();
            LevelUIManager.instance.UpdateStats(currentPlayer);

            if (nextMethod == "" || nextMethod == "NONE")
            {
                continue;
            }
            else
            {
                yield return ResolveMethod(nextMethod);
                LevelUIManager.instance.UpdateStats(currentPlayer);
            }
        }

        OnCardResolved?.Invoke(this, EventArgs.Empty);
        PhaseManager.instance.selectedTile = currentPlayer.currentTile;
        PhaseManager.instance.violentCards += (data.isViolent) ? 1 : 0;
    }

    IEnumerator ResolveMethod(string methodName)
    {
        methodName = methodName.Replace("]", "").Trim();

        if (methodName.Contains("CHOOSEBUTTON("))
        {
            string[] choices = methodName.Replace("CHOOSEBUTTON(", "").Replace(")", "").Replace("]", "").Trim().Split('|');
            yield return ChooseOptions(choices);
        }
        else
        {
            switch (methodName)
            {
                case "DAMAGEDRAW":
                    foreach (PlayerEntity nextPlayer in LevelGenerator.instance.listOfPlayers)
                    {
                        if (nextPlayer.damageTaken > 0)
                            nextPlayer.PlusCards(data.draw);
                    }
                    break;
                case "DRAWCARDS":
                    currentPlayer.PlusCards(data.draw);
                    break;
                case "PICKUPCARD":
                    currentPlayer.PlusCards(currentPlayer.myDiscardPile[^1]);
                    break;
                case "ALLDRAWCARDS":
                    foreach (PlayerEntity nextPlayer in LevelGenerator.instance.listOfPlayers)
                        nextPlayer.PlusCards(data.draw);
                    break;
                case "FINDATTACK":
                    yield return FindCard(currentPlayer, CardType.Attack);
                    break;
                case "FINDMOVEMENT":
                    yield return FindCard(currentPlayer, CardType.Movement);
                    break;
                case "FINDCOST":
                    yield return FindCard(currentPlayer, data.chEP);
                    break;

                case "CHOOSEDISCARD":
                    yield return ChooseDiscard(currentPlayer);
                    break;
                case "PASSTOPLAYER":
                    yield return ChoosePlayer();
                    yield return PassCard(currentPlayer, adjacentTilesWithPlayers[0].myEntity.GetComponent<PlayerEntity>());
                    currentTarget = currentPlayer.currentTile;
                    break;
                case "DISCARDHAND":
                    while (currentPlayer.myHand.Count > 0)
                        StartCoroutine(currentPlayer.DiscardFromHand(currentPlayer.myHand[0]));
                    yield return new WaitForSeconds(PlayerPrefs.GetFloat("Animation Speed"));
                    break;

                case "CHANGEHP":
                    currentPlayer.ChangeHealth(data.chHP);
                    break;
                case "CHANGEADJACENTHP":
                    yield return ChoosePlayer();
                    PlayerEntity player = adjacentTilesWithPlayers[0].myEntity.GetComponent<PlayerEntity>();
                    currentTarget = currentPlayer.currentTile;
                    player.ChangeHealth(data.chHP);
                    break;

                case "CHANGEEP":
                    currentPlayer.ChangeEnergy(data.chEP);
                    break;
                case "MAXENERGY":
                    currentPlayer.maxEnergy += data.chEP;
                    break;
                case "ZEROENERGY":
                    currentPlayer.SetEnergy(0);
                    break;
                case "CONVERTMOVEMENTTOENERGY":
                    yield return ChooseFromSlider("Pay how much movement?", 0, currentPlayer.movementLeft);
                    currentPlayer.ChangeMovement(-1 * sliderData);
                    currentPlayer.ChangeEnergy(sliderData);
                    break;

                case "FREEMOVE":
                    yield return PhaseManager.instance.ChooseMovePlayer(currentPlayer, 3, true);
                    break;
                case "CHANGEMP":
                    currentPlayer.ChangeMovement(data.chMP);
                    break;
                case "MAXMOVEMENT":
                    currentPlayer.maxMovement += data.chMP;
                    break;
                case "ZEROMOVEMENT":
                    currentPlayer.SetMovement(0);
                    break;

                case "CHANGECOST":
                    currentPlayer.costChange.Add(this);
                    break;
                case "CHANGECOSTTWOPLUS":
                    costChangeCondition = "COSTS 2+";
                    foreach (PlayerEntity nextPlayer in LevelGenerator.instance.listOfPlayers)
                        nextPlayer.costChange.Add(this);
                    break;

                case "STUNADJACENTGUARD":
                    yield return ChooseGuard();
                    if (adjacentTilesWithGuards.Count > 0)
                        yield return StunGuard(adjacentTilesWithGuards[0].myEntity.GetComponent<GuardEntity>());
                    break;

                case "ATTACKADJACENTWALL":
                    yield return ChooseWall();
                    yield return AttackWall(adjacentTilesWithWalls[0].myEntity.GetComponent<WallEntity>());
                    break;

                case "THROWNBOTTLE":
                    yield return ChooseTileLOS();
                    if (currentTarget.myEntity != null && currentTarget.myEntity.CompareTag("Guard")) yield return CalculateDistraction(currentPlayer.currentTile);
                    else yield return CalculateDistraction(currentTarget);
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

                case "THROWENVIRONMENTAL":
                    yield return ChooseTile();
                    EnvironmentalEntity newEnviro = LevelGenerator.instance.CreateEnvironmental();
                    MaterialPropertyBlock matBlock = new();
                    matBlock.SetFloat("_Fill", 1);
                    newEnviro.timerRen.SetPropertyBlock(matBlock);
                    newEnviro.ValueDisplay.text = data.delay.ToString();
                    newEnviro.currentTile = currentTarget;
                    newEnviro.spriteRenderer.sortingOrder = 10;
                    newEnviro.transform.SetParent(newEnviro.currentTile.transform);
                    newEnviro.transform.localPosition = new Vector3(0, 1, 0);
                    newEnviro.name = this.name;
                    newEnviro.card = this;
                    newEnviro.delay = data.delay;
                    newEnviro.delayMax = data.delay;
                    LevelGenerator.instance.listOfEnvironmentals.Add(newEnviro);
                    break;
                case "THROWMODIFIER":
                    yield return ChooseTile();
                    TileModifier newModifier = currentTarget.gameObject.AddComponent<TileModifier>();
                    currentTarget.listOfModifiers.Add(newModifier);
                    newModifier.card = this;
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
        List<TileData> affectedTiles = Pathfinder.instance.CalculateIntensity(source, data.volume, true);
        //print(affectedTiles.Count);
        for (int i = 0; i < affectedTiles.Count; i++)
        {
            StartCoroutine(affectedTiles[i].NoiseFlash(Pathfinder.instance.GetDistance(source.gridPosition, affectedTiles[i].gridPosition)));
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
        LevelGenerator.instance.DisableAllCards();
        LevelGenerator.instance.DisableAllTiles();
        yield return ResolveList(data.action);
    }

    public IEnumerator NextRoundEffect()
    {
        LevelGenerator.instance.DisableAllCards();
        LevelGenerator.instance.DisableAllTiles();
        yield return ResolveList(data.nextAct);
    }

    #endregion

#region Choose Options

    IEnumerator ChooseGuard()
    {
        adjacentTilesWithGuards = SearchAdjacentGuard(currentPlayer.currentTile);
        if (adjacentTilesWithGuards.Count == 0)
            yield break;

        //if (adjacentTilesWithGuards.Count != 1)
        //{
            InstructionsManager.UpdateInstructions(this,
                new string[] { "OnChoiceMade" },
                new string[] { "Choose a guard in range." }
            );
            PhaseManager.instance.WaitForDecision(adjacentTilesWithGuards);

            while (PhaseManager.instance.chosenTile == null)
                yield return null;

            yield return PhaseManager.instance.ConfirmUndo($"Confirm guard?", new Vector2(0, 350));
            if (PhaseManager.instance.confirmChoice == 1)
            {
                yield return ChooseGuard();
                yield break;
            }

            OnChoiceMade?.Invoke(this, EventArgs.Empty);
            adjacentTilesWithGuards.Clear();
            adjacentTilesWithGuards.Add(PhaseManager.instance.chosenTile);
        //}
    }

    IEnumerator ChoosePlayer()
    {
        adjacentTilesWithPlayers = SearchAdjacentPlayers(currentPlayer.currentTile, true);
        if (adjacentTilesWithPlayers.Count == 0)
            yield break;

        //if (adjacentTilesWithPlayers.Count != 1)
        //{
            InstructionsManager.UpdateInstructions(this,
                new string[] { "OnChoiceMade", "OnCardResolved" },
                new string[] { "Choose a player in range, ", "then resolve the card." }
                );
            PhaseManager.instance.WaitForDecision(adjacentTilesWithPlayers);

            while (PhaseManager.instance.chosenTile == null)
                yield return null;

            yield return PhaseManager.instance.ConfirmUndo($"Confirm player?", new Vector2(0, 350));
            if (PhaseManager.instance.confirmChoice == 1)
            {
                yield return ChoosePlayer();
                yield break;
            }

            OnChoiceMade?.Invoke(this, EventArgs.Empty);
            adjacentTilesWithPlayers.Clear();
            adjacentTilesWithPlayers.Add(PhaseManager.instance.chosenTile);
        //}
    }

    IEnumerator ChooseWall()
    {
        adjacentTilesWithWalls = SearchAdjacentWall(currentPlayer.currentTile);
        if (adjacentTilesWithWalls.Count == 0)
            yield break;

        //if (adjacentTilesWithWalls.Count != 1)
        //{
            InstructionsManager.UpdateInstructions(this,
                new string[] { "OnChoiceMade" },
                new string[] { "Choose a wall in range." }
            );
            PhaseManager.instance.WaitForDecision(adjacentTilesWithWalls);

            while (PhaseManager.instance.chosenTile == null)
                yield return null;

            yield return PhaseManager.instance.ConfirmUndo($"Confirm wall?", new Vector2(0, 350));
            if (PhaseManager.instance.confirmChoice == 1)
            {
                yield return ChooseWall();
                yield break;
            }

            OnChoiceMade?.Invoke(this, EventArgs.Empty);
            adjacentTilesWithWalls.Clear();
            adjacentTilesWithWalls.Add(PhaseManager.instance.chosenTile);
        //}
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
            DetectLines.Add(Pathfinder.instance.line(currentPlayer.currentTile.gridPosition, currentPlayer.currentTile.gridPosition + Vector2Int.RoundToInt(newVector.normalized * data.range)));
            //Debug.DrawRay(currentPlayer.transform.position, new Vector3(Vector2Int.RoundToInt(newVector.normalized * data.range).x, 0, Vector2Int.RoundToInt(newVector.normalized * data.range).y),Color.green);
        }
        for (int i = 0; i < DetectLines.Count; i++)
        {
            foreach (Vector2Int point in DetectLines[i])
            {
                TileData TileToAdd = LevelGenerator.instance.FindTile(point);
                if (TileToAdd == null)
                {
                    break;
                }
                tilesInRange.Add(point);
                if (TileToAdd.myEntity != null)
                {
                    if (TileToAdd.myEntity.Occlusion && point != currentPlayer.currentTile.gridPosition)
                    {
                        break;
                    }
                }

            }
        }
        List<TileData> tilesToSelect = new List<TileData>();
        foreach (Vector2Int point in tilesInRange)
        {
            tilesToSelect.Add(LevelGenerator.instance.FindTile(point));
        }

        InstructionsManager.UpdateInstructions(this,
            new string[] { "OnChoiceMade" },
            new string[] { "Choose a tile in range." }
        );
        PhaseManager.instance.WaitForDecision(tilesToSelect);

        while (PhaseManager.instance.chosenTile == null)
            yield return null;

        yield return PhaseManager.instance.ConfirmUndo($"Confirm tile?", new Vector2(0, 350));
        if (PhaseManager.instance.confirmChoice == 1)
        {
            yield return ChooseTileLOS();
            yield break;
        }

        OnChoiceMade?.Invoke(this, EventArgs.Empty);
        currentTarget = PhaseManager.instance.chosenTile;
    }

    IEnumerator ChooseTile()
    {
        List<TileData> tilesInRange = Pathfinder.instance.CalculateReachableGrids(currentPlayer.currentTile, data.range, false);
        InstructionsManager.UpdateInstructions(this, new string[] { "OnChoiceMade" }, new string[] { "Choose a tile in range." });
        PhaseManager.instance.WaitForDecision(tilesInRange);

        while (PhaseManager.instance.chosenTile == null)
            yield return null;

        yield return PhaseManager.instance.ConfirmUndo($"Confirm tile?", new Vector2(0, 350));
        if (PhaseManager.instance.confirmChoice == 1)
        {
            yield return ChooseTile();
            yield break;
        }

        OnChoiceMade?.Invoke(this, EventArgs.Empty);
        currentTarget = PhaseManager.instance.chosenTile;
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
                    newCollector.AddTextButton($"Draw {data.draw}");
                    break;
                case "CHANGEMP":
                    newCollector.AddTextButton($"+{data.chMP} Movement");
                    break;
                case "CHANGEEP":
                    newCollector.AddTextButton($"+{data.chEP} Energy");
                    break;
                case "CHANGEHP":
                    newCollector.AddTextButton($"+{data.chHP} Health");
                    break;
            }
        }

        yield return newCollector.WaitForChoice();
        OnChoiceMade?.Invoke(this, EventArgs.Empty);
        yield return ResolveMethod(choices[newCollector.chosenButton]);
        Destroy(newCollector.gameObject);
    }

    internal IEnumerator ChooseDiscard(PlayerEntity player)
    {
        for (int i = 0; i < data.chooseHand; i++)
        {
            InstructionsManager.UpdateInstructions(this,
                new string[] { "OnChoiceMade" },
                new string[] { $"Discard a card from your hand ({data.chooseHand - i} more)." }
            );
            //if (player.myHand.Count >= 2)
            //{
                PhaseManager.instance.WaitForDecision(player.myHand);
                while (PhaseManager.instance.chosenCard == null)
                    yield return null;

                yield return PhaseManager.instance.ConfirmUndo($"Discard {PhaseManager.instance.chosenCard.name}?", new Vector2(0, 350));
                if (PhaseManager.instance.confirmChoice == 1)
                {
                    i--;
                    continue;
                }

                yield return player.DiscardFromHand(PhaseManager.instance.chosenCard);
            //}
            //else if (player.myHand.Count == 1)
            //{
            //    yield return player.DiscardFromHand(player.myHand[0]);
            //}
            LevelUIManager.instance.UpdateStats(currentPlayer);
            OnChoiceMade?.Invoke(this, EventArgs.Empty);
        }
    }

    internal IEnumerator PassCard(PlayerEntity thisPlayer, PlayerEntity otherPlayer)
    {
        for (int i = 0; i < data.chooseHand; i++)
        {
            InstructionsManager.UpdateInstructions(this, new string[] { "OnChoiceMade" }, new string[] { $"Give {otherPlayer.name} a card from your hand." });
            //if (thisPlayer.myHand.Count >= 2)
            //{
            PhaseManager.instance.WaitForDecision(thisPlayer.myHand);
            while (PhaseManager.instance.chosenCard == null)
                yield return null;

            yield return PhaseManager.instance.ConfirmUndo($"Play {PhaseManager.instance.chosenCard.name}?", new Vector2(0, 350));
            if (PhaseManager.instance.confirmChoice == 1)
            {
                i--;
                continue;
            }

            otherPlayer.PlusCards(PhaseManager.instance.chosenCard);

            //else if (thisPlayer.myHand.Count == 1)
            //{
            //    otherPlayer.PlusCards(thisPlayer.myHand[0]);
            //}
        }
        OnChoiceMade?.Invoke(this, EventArgs.Empty);
        StartCoroutine(thisPlayer.SortHandCoroutine());
        LevelUIManager.instance.UpdateStats(currentPlayer);
    }


#endregion

#region Helper Methods

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
            player.PlusCards(data.draw);
        else
            player.PlusCards(foundCard);
        player.myDrawPile.Shuffle();
    }

    internal IEnumerator FindCard(PlayerEntity player, int costToFind)
    {
        yield return null;
        List<Card> shuffledBack = new();
        Card foundCard = null;
        foreach (Card card in player.myDrawPile)
        {
            if (card.energyCost == costToFind)
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
            player.PlusCards(data.draw);
        else
            player.PlusCards(foundCard);
        player.myDrawPile.Shuffle();
    }

    internal IEnumerator StunPlayer(PlayerEntity player)
    {
        yield return null;
        //guard.stunSound.Post(guard.gameObject);
        //guard.DetectionRangePatrol = 0;
        player.stunChange(data.stun);
        //guard.CalculateTiles();
        //currentTarget = guard.currentTile;
    }

    internal IEnumerator StunGuard(GuardEntity guard)
    {
        yield return null;
        guard.stunSound.Post(guard.gameObject);
        guard.DetectionRangePatrol = 0;
        guard.stunChange(data.stun);
        guard.CalculateTiles();
        currentTarget = guard.currentTile;
        MoveCamera.instance.Shake();
    }

    internal IEnumerator AttackWall(WallEntity wall)
    {
        if (wall != null)
        {
            wall.AffectWall(data.wHP);
            currentTarget = wall.currentTile;
        }
        yield return null;
    }
    #endregion

}