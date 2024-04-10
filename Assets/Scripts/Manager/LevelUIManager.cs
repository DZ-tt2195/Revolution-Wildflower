using System;
using UnityEngine;
using MyBox;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class LevelUIManager : MonoBehaviour
{

#region Variables

    public static LevelUIManager instance;

    [Foldout("Misc UI elements", true)]
    [Tooltip("Hazardbox to give to players")] public CanvasGroup ManagerHazardBox;
    [Tooltip("Your hand in the canvas")] public Transform handContainer;
    [Tooltip("Text for current level's objective")] TMP_Text lvlObjective;
    [Tooltip("Text for the turn count")] TMP_Text turnCountTxt;
    [Tooltip("Tracks number of cards in draw pile")] TMP_Text drawPile;
    [Tooltip("Last player button that mouse was hovered over")] GameObject lastHoveredBar;

    [Foldout("Top left", true)]
    [Tooltip("Current player selected")] TMP_Text currentCharacter;
    [Tooltip("Face of selected character")] Image selected_characterFace;
    [Tooltip("Blank character face")] Sprite emptyFace;
    [Tooltip("Spritesheet of player character faces")] Sprite[] facesSpritesheet;

    [Foldout("Turn alert bar", true)]
    [Tooltip("pop up alerting player of turn")][SerializeField] CanvasGroup turnAlertBar;
    [Tooltip("text on turn banner")][SerializeField] TMP_Text turnText;

    [Foldout("Stat bars", true)]
    [Tooltip("Selected player's health")] public StatBar healthBar;
    [Tooltip("Selected player's moves left")] public StatBar movementBar;
    [Tooltip("Selected player's energy")] public StatBar energyBar;

    [Foldout("Flashing", true)]
    [Tooltip("the transparancy of card/tile borders")][ReadOnly] public float opacity = 1;
    [Tooltip("whether the borders are turning white or black")][ReadOnly] public bool decrease = true;

    #endregion

#region Setup

    private void Awake()
    {
        instance = this;
        facesSpritesheet = Resources.LoadAll<Sprite>("Sprites/selected_portrait_spritesheet");
        drawPile = GameObject.Find("Draw Pile").GetComponentInChildren<TMP_Text>();
        emptyFace = Resources.Load<Sprite>("Sprites/noCharacter");

        Transform playerStats = GameObject.Find("SelectedPlayer_Stats").transform;
        currentCharacter = playerStats.Find("PlayerName").GetComponent<TMP_Text>();
        healthBar = playerStats.Find("Health").GetComponentInChildren<StatBar>();
        movementBar = playerStats.Find("Movement").GetComponentInChildren<StatBar>();
        energyBar = playerStats.Find("Energy").GetComponentInChildren<StatBar>();
        selected_characterFace = playerStats.Find("CharacterFace").GetComponent<Image>();

        Transform informationImage = GameObject.Find("TopLeftUI").transform;
        lvlObjective = informationImage.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
        turnCountTxt = informationImage.GetChild(1).GetChild(0).GetComponent<TMP_Text>();
    }

    private void Start()
    {
        UpdateStats(null);
    }

    private void FixedUpdate()
    {
        opacity += (decrease) ? 0.05f : -0.05f;
        if (opacity < 0 || opacity > 1)
            decrease = !decrease;
    }

    public IEnumerator FadeTurnBar(string message)
    {
        LevelGenerator.instance.DisableAllCards();
        LevelGenerator.instance.DisableAllTiles();

        turnAlertBar.gameObject.SetActive(true);
        turnAlertBar.alpha = 0;
        turnText.text = message;

        float waitTime = PlayerPrefs.GetFloat("Animation Speed");
        float elapsedTime = 0f;

        while (elapsedTime < waitTime)
        {
            turnAlertBar.alpha = elapsedTime / waitTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        turnAlertBar.alpha = 1f;

        yield return new WaitForSeconds(waitTime);

        while (elapsedTime > 0f)
        {
            turnAlertBar.alpha = elapsedTime / waitTime;
            elapsedTime -= Time.deltaTime;
            yield return null;
        }
        turnAlertBar.alpha = 0f;
    }
/*
    public void PlayerBarSelect()
    {
        Debug.Log(PhaseManager.instance.lastSelectedPlayer);

        lastHoveredBar = gameObject;
        lastHoveredBar.GetComponent<Animator>().SetBool("selected", true);
        
        PlayerBarDeselect();
    }

    public void PlayerBarDeselect()
    {
        foreach(PlayerEntity player in LevelGenerator.instance.listOfPlayers)
        {
            if ((PhaseManager.instance.lastSelectedPlayer == null || PhaseManager.instance.lastSelectedPlayer.myBar.playerName == lastHoveredBar.transform.GetChild(0).GetComponent<TMP_Text>()) && (player.myBar.playerName != lastHoveredBar.transform.GetChild(0).GetComponent<TMP_Text>()))
            {
                lastHoveredBar.GetComponent<Animator>().SetBool("selected", false);
               
            }
            else if(player.myBar.playerName != PhaseManager.instance.lastSelectedPlayer.myBar.playerName) 
            { 
                PhaseManager.instance.lastSelectedPlayer.myBar.animator.SetBool("selected", false);
            }


        }
        
    }*/

    #endregion

    #region Stats

    public void UpdateStats(PlayerEntity player)
    {
        if (player == null)
        {
            lvlObjective.text = "";
            currentCharacter.text = "";
            selected_characterFace.sprite = emptyFace;

            if (healthBar.gameObject.activeInHierarchy) { healthBar.SetValue(0); };
            if (movementBar.gameObject.activeInHierarchy) { movementBar.SetValue(0); };
            if (energyBar.gameObject.activeInHierarchy) { energyBar.SetValue(0); }

            handContainer.transform.localPosition = new Vector3(-10000, -10000, 0);
        }

        else
        {
            currentCharacter.text = $"{player.name}";
            selected_characterFace.sprite = player.name switch
            {
                "Frankie" => facesSpritesheet[0],
                "WK" => facesSpritesheet[1],
                "Gail" => facesSpritesheet[2],
                _ => emptyFace,
            };
            drawPile.text = player.myDrawPile.Count.ToString();
            if (player.myDrawPile.Count <= 5)
            {
                drawPile.color = Color.red;
            }
            else
            {
                drawPile.color = Color.black;
            }

            if (player.myPosition * -2000 != handContainer.transform.localPosition.x)
            {
                player.MyTurn();
                handContainer.transform.localPosition = new Vector3(player.myPosition * -2000, 0, 0);
            }

            if (healthBar.gameObject.activeInHierarchy) { healthBar.SetValue(player.health); }
            if (movementBar.gameObject.activeInHierarchy) { movementBar.SetValue(player.movementLeft); movementBar.SetMaximumValue(player.maxMovement); }
            if (energyBar.gameObject.activeInHierarchy) { energyBar.SetValue(player.myEnergy); energyBar.SetMaximumValue(player.maxEnergy); }
        }

        lvlObjective.text = $"{LevelGenerator.instance.listOfObjectives.Count} Objectives Left";
        turnCountTxt.text = $"{PhaseManager.instance.turnCount} Turns Left";

        foreach (PlayerEntity nextPlayer in LevelGenerator.instance.listOfPlayers)
        {
            nextPlayer.myBar.ChangeText($"{nextPlayer.myHand.Count} Cards; {nextPlayer.health} HP; \n{nextPlayer.movementLeft} Moves; {nextPlayer.myEnergy} Energy");
        }
    }

    #endregion

}
