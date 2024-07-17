using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using UnityEngine.EventSystems;

public class TileData : MonoBehaviour
{
    public enum TileType { Regular, Exit, AllExit };
    [Foldout("Tile information", true)]
        [Tooltip("Attached arrow")] public SpriteRenderer directionIndicator;
        [Tooltip("arrow sprites")] public List<Sprite> arrowSprites = new();
        [Tooltip("Exit tile graphic")] public GameObject exitGraphic;
        [Tooltip("All adjacent tiles")] [ReadOnly] public List<TileData> adjacentTiles;
        [Tooltip("Position in the grid")] [ReadOnly] public Vector2Int gridPosition;
        [Tooltip("The entity on this tile")] [ReadOnly] public Entity myEntity;
        [Tooltip("What kind of tile this is")] [ReadOnly] public TileType myType;
        [Tooltip("Modifiers on this tile")][ReadOnly] public List<TileModifier> listOfModifiers = new();
        [Tooltip("Container for all Children for movement")][SerializeField] GameObject childContainer;
        [Tooltip("animation curve for mouseOver movement")][SerializeField] AnimationCurve mouseOverCurve;
        [Tooltip("Timer for mouseOverCurve")][SerializeField] float mouseOverAnimTimerMax;
        [Tooltip("internalTimer")] private float mouseOverAnimTimer = 0;
        [Tooltip("height of mouseOver")][SerializeField] float mouseOverDisplace;
        [Tooltip("Baseheight of the tile")] float baseHeight;
    [Tooltip("Guard currently surveilling this area")] public GuardEntity surveillingGuard;

    [Foldout("Tile conditions", true)]
        [Tooltip("Defines whether you can click this tile")][ReadOnly] public bool clickable = false;
        [Tooltip("Defines whether you can move onto this tile")][ReadOnly] public bool moveable = false;
        [Tooltip("if a guard is looking at this tile")][ReadOnly] public bool currentGuardTarget = false;
        [Tooltip("Defines whether an arrow should be hovering over this tile.")][ReadOnly] public bool indicatorArrow = false;
        [Tooltip("Defines whether you can select this tile for a card action")][ReadOnly] public bool CardSelectable = false;
        [Tooltip("If your mouse is over this")] public bool moused = false;

    [Foldout("Mouse", true)]
        [Tooltip("Layer mask that mouse raycasts ignore")] [SerializeField] LayerMask mask;
        [Tooltip("timer that controls how long until a tool tip appears on hover")] float timeTillToolTip = 0.25f;
        [Tooltip("timer that controls how long until a tool tip appears on hover")] float toolTipHoverTimer = 0;

    [Foldout("Colors", true)]
        [Tooltip("Tile's sprite renderer")] SpriteRenderer myRenderer;
        [Tooltip("Tile's materal")] [SerializeField] Renderer renderer3d;
        [Tooltip("Glowing border's sprite renderer")][SerializeField] SpriteRenderer border;
        [Tooltip("Indication arrow for forced tiles")][SerializeField] SpriteRenderer indicator;
        [Tooltip("Danger stripes for when under surveillance")] [SerializeField] SpriteRenderer dangerStripes;
        [Tooltip("Material Property Block for this specific tile's danger stripe material")] MaterialPropertyBlock dangerStripesPropertyBlock; 
        [Tooltip("Indication arrow transform")]private Transform indicatorTransform;
        [Tooltip("color used for unselected moused over tiles")][SerializeField] Color mouseOverColor = new Color(0.9f,0.9f,0.9f,1);
        [Tooltip("color used for selected tiles")][SerializeField] Color SelectedColor = new Color(0.6f, 0.6f, 0.6f, 1);
        [Tooltip("color used for unselected moused over tiles (general)")][SerializeField] Color ClickableColor = new Color(0.9f, 0.9f, 0.9f, 1);
        [Tooltip("color used for unselected moused over tiles you can move onto")][SerializeField] Color MoveableColor = new Color(0.9f, 0.9f, 0.9f, 1);
        [Tooltip("color used for guard's distraction target tiles")] [SerializeField] Color AlertColor = new Color(0.9f, 0.7f, 0.1f, 1);
        [Tooltip("color used for unselected moused over tiles you can select for a card action")][SerializeField] Color CardSelectableColor = new Color(0.9f, 0.7f, 0.1f, 1);     
        [Tooltip("Time for noise indecator to show")] [SerializeField] float AlertDelay = 0.2f;
        [Tooltip("Base delay noise indecator")] [SerializeField] float BaseAlertDelay = 0.2f;
        [Tooltip("Variable indicating when tile should highlight for noise")][ReadOnly] bool noiseThrough = false;
    [SerializeField] bool underSurvey = false;
    bool surveyFlashing;
    [SerializeField] Color defaultDangerStripesColor;
    [SerializeField] Color triggeredDangerStripesColor;
    [SerializeField] float dangerStripesSpeed = 3;
    private Color currentStripesColor;

    private void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        myRenderer.sortingOrder = 0;
        border.color = new Color(1f, 1f, 1f, 0);
        indicatorTransform = indicator.transform;
        directionIndicator.enabled = false;
        baseHeight = transform.position.y;
        dangerStripesPropertyBlock = new MaterialPropertyBlock();
        defaultDangerStripesColor = dangerStripes.material.GetColor("_StripeColor");
        currentStripesColor = defaultDangerStripesColor;
    }

    private void Start()
    {
        if (myType != TileType.Regular)
        {
            exitGraphic.SetActive(true);
        }
    }

    void FixedUpdate()
    {
        if (border == null)
        {
            Debug.Log("border is null");
        }
        else if (noiseThrough)
        {
            border.color = AlertColor;
        }
        else if (PhaseManager.instance.selectedTile == this)
        {
            border.color = SelectedColor;
            border.SetAlpha(LevelUIManager.instance.opacity);
        }
        else if (currentGuardTarget)
        {
            border.color = AlertColor;
        }
        else if (moused)
        {
            border.color = mouseOverColor;
        }
        else if (moveable)
        {
            border.color = MoveableColor;
            border.SetAlpha(LevelUIManager.instance.opacity);
        }
        else if (CardSelectable)
        {
            border.color = CardSelectableColor;
            border.SetAlpha(LevelUIManager.instance.opacity);
        }
        else if (clickable)
        {
            border.color = ClickableColor;
            border.SetAlpha(LevelUIManager.instance.opacity);
        }
        else
        {
            border.SetAlpha(0);
        }

        if (indicatorArrow)
        {
            Debug.Log("arrow should render");
            indicator.SetAlpha(1);
            indicatorTransform.position = new Vector3(indicatorTransform.position.x, Mathf.Sin(Time.time * 1.5f) * 0.5f + 2, indicatorTransform.position.z);
        }

        else { indicator.SetAlpha(0); }

    }

    public IEnumerator NoiseFlash(int distance)
    {
        yield return new WaitForSeconds(AlertDelay * distance);
        noiseThrough = true;
        yield return new WaitForSeconds(BaseAlertDelay + AlertDelay * distance);
        noiseThrough = false;
    }

    private void MouseEnter()
    {
        moused = true;
        //generates a visible path the player is going to take to get to the space (clearing the last list and ignoring the first and last tile)
        if (moveable)
        {
            GuardEntity tileSurveillingGuard = null;
            foreach (TileData tile in Pathfinder.instance.FullPath)
            {
                tile.directionIndicator.enabled = false;
                if (tile.underSurvey)
                {
                    Debug.Log("Previous list had a tile under surveillance; disabling flash, then erasing list.");
                    tileSurveillingGuard = tile.surveillingGuard;
                }
            }

            bool containsSurveilledTile = false;

            Pathfinder.instance.CalculatePathfinding(PhaseManager.instance.lastSelectedPlayer.currentTile, this, PhaseManager.instance.lastSelectedPlayer.movementLeft, false, false);

            foreach (TileData tile in Pathfinder.instance.FullPath)
            {
                tile.directionIndicator.enabled = true;
                if (tile.underSurvey)
                {
                    tileSurveillingGuard = tile.surveillingGuard;
                    containsSurveilledTile = true;
                }
            }

            LevelUIManager.instance.movementBar.Preview(-Pathfinder.instance.FullPath.Count);


            if (containsSurveilledTile)
            {
                if (tileSurveillingGuard != null)
                {
                    Debug.Log("Path contains surveilled tile, enabling flash.");
                    tileSurveillingGuard.ToggleSurveillingTileFlash(true);
                }
            }

            else
            {
                if (tileSurveillingGuard != null)
                {
                    Debug.Log("Path does not contain surveilled tile, disabling flash.");
                    tileSurveillingGuard.ToggleSurveillingTileFlash(false);
                }
            }
        }
    }

    private void MouseExit() 
    {
        moused = false;
        if (moveable)
        {
            /*
            for (int i = 0; i < PathfindingManager.instance.FullPath.Count; i++)
            {
                //NewManager.instance.FullPath[i].directionIndicator.enabled = false;
            }
            */
        }
    }

    private void MouseOver()
    {
        if (mouseOverAnimTimer < mouseOverAnimTimerMax) mouseOverAnimTimer += Time.deltaTime;
        else mouseOverAnimTimer = mouseOverAnimTimerMax;

        if (clickable && Input.GetKeyDown(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (moveable)
            {
                foreach (TileData tile in Pathfinder.instance.FullPath)
                    tile.directionIndicator.enabled = false;
            }
            PhaseManager.instance.selectedTile = this;
            if (moveable || CardSelectable)
            {
                PhaseManager.instance.ReceiveChoice(this);
            }
            else if (myEntity != null)
            {
                if (myEntity.CompareTag("Player"))
                {
                    PlayerEntity player = myEntity.GetComponent<PlayerEntity>();
                    if (player.stunned == 0)
                    {
                        PhaseManager.instance.ControlCharacter(player);
                    }
                }
            }
        }

        if (myEntity != null)
        {
            toolTipHoverTimer += Time.deltaTime;
            if (toolTipHoverTimer >= timeTillToolTip)
            {
                EntityToolTip.instance.SetInfo(myEntity.name, myEntity.HoverBoxText());
                /*
                EntityToolTip.instance.EntityName.text = myEntity.name;
                EntityToolTip.instance.EntityInfo.text = myEntity.HoverBoxText();
                EntityToolTip.instance.gameObject.SetActive(true);
                EntityToolTip.instance.isActive = true;
                */
                //if the tile entity is a guard, show their path to their current target
                if (myEntity.CompareTag("Enemy"))
                {
                    foreach (TileData tile in Pathfinder.instance.FullPath)
                        tile.directionIndicator.enabled = false;
                    GuardEntity currentGuard = myEntity.gameObject.GetComponent<GuardEntity>();
                    if (currentGuard.alertStatus == GuardEntity.Alert.Patrol)
                    {
                        Pathfinder.instance.CalculatePathfinding(this, LevelGenerator.instance.listOfTiles[currentGuard.PatrolPoints[currentGuard.PatrolTarget].x, currentGuard.PatrolPoints[currentGuard.PatrolTarget].y],99,false,false);
                        foreach (TileData tile in Pathfinder.instance.FullPath)
                            tile.directionIndicator.enabled = true;
                    }
                    else if (currentGuard.alertStatus == GuardEntity.Alert.Attack)
                    {
                        Pathfinder.instance.CalculatePathfinding(this, currentGuard.CurrentTarget.currentTile, 99, false, false);
                        foreach (TileData tile in Pathfinder.instance.FullPath)
                            tile.directionIndicator.enabled = true;
                    }
                    else if (currentGuard.alertStatus == GuardEntity.Alert.Persue)
                    {
                        Pathfinder.instance.CalculatePathfinding(this, LevelGenerator.instance.listOfTiles[currentGuard.DistractionPoints[0].x,currentGuard.DistractionPoints[0].y], 99, false, false);
                        foreach (TileData tile in Pathfinder.instance.FullPath)
                            tile.directionIndicator.enabled = true;
                    }
                }
            }
        }
    }

    public void SurveillanceState(GuardEntity guard, bool underSurveillance)
    {
        if (underSurveillance)
        {
            surveillingGuard = guard;
        }

        else
        {
            surveillingGuard = null;
        }

        underSurvey = underSurveillance;
        dangerStripes.gameObject.SetActive(underSurveillance);
        //Debug.Log(dangerStripes.gameObject.activeSelf);
        //renderer3d.material.color = (underSurveillance) ? Color.red : Color.gray;
        //renderer3d.material.SetColor("_palette_color", underSurveillance ? Color.red : new Color(0, 0.3686275f, 0.2352941f));
    }

    public void SetSurveillanceFlash(bool flash)
    {
        surveyFlashing = flash;
        if (!surveyFlashing)
        {
            dangerStripesPropertyBlock.SetColor("_StripeColor", defaultDangerStripesColor);
            dangerStripes.SetPropertyBlock(dangerStripesPropertyBlock);
        }
    }

    private void Update()
    {
        transform.position = new Vector3(transform.position.x, baseHeight + (mouseOverDisplace * mouseOverCurve.Evaluate(mouseOverAnimTimer / mouseOverAnimTimerMax)), transform.position.z);

        if (surveyFlashing)
        {
            dangerStripesPropertyBlock.SetColor("_StripeColor", Color.Lerp(defaultDangerStripesColor, triggeredDangerStripesColor, (Mathf.Sin(Time.time * dangerStripesSpeed) / 2) + 0.5f));
            dangerStripes.SetPropertyBlock(dangerStripesPropertyBlock);
        }

        else
        {
            dangerStripesPropertyBlock.SetColor("_StripeColor", currentStripesColor);
            dangerStripes.SetPropertyBlock(dangerStripesPropertyBlock);
        }

        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(mouseRay, out hit, Mathf.Infinity, mask) && hit.collider.gameObject == gameObject)
        {
            if (!moused) MouseEnter();
            MouseOver();
        }
        else if (moused) MouseExit();
        
        if (!moused)
        {
            if (mouseOverAnimTimer > 0) mouseOverAnimTimer -= Time.deltaTime;
            else mouseOverAnimTimer = 0;
            if (toolTipHoverTimer > 0)
            {
                toolTipHoverTimer = 0;
                foreach (TileData tile in Pathfinder.instance.FullPath)
                {
                    tile.directionIndicator.enabled = false;
                }
            }
        }
    }
}