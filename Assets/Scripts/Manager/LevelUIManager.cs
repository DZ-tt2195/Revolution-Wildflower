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
                Debug.Log("move hands on screen");
                player.MyTurn();
                handContainer.transform.localPosition = new Vector3(player.myPosition * -2000, 0, 0);
            }

            if (healthBar.gameObject.activeInHierarchy) { healthBar.SetValue(player.health); }
            if (movementBar.gameObject.activeInHierarchy) { movementBar.SetValue(player.movementLeft); movementBar.SetMaximumValue(player.movesPerTurn); }
            if (energyBar.gameObject.activeInHierarchy) { energyBar.SetValue(player.myEnergy); energyBar.SetMaximumValue(player.maxEnergy); }
        }

        lvlObjective.text = $"{LevelGenerator.instance.listOfObjectives.Count} Objectives Left";
        turnCountTxt.text = $"{PhaseManager.instance.turnCount} Turns Left";

        foreach (PlayerEntity nextPlayer in LevelGenerator.instance.listOfPlayers)
        {
            nextPlayer.myBar.ChangeText($"{nextPlayer.myHand.Count} Cards; {nextPlayer.health} HP; \n{nextPlayer.movementLeft} Moves; {nextPlayer.myEnergy} Energy");
        }
    }

    /// <summary>
    /// set energy value
    /// </summary>
    /// <param name="player">the player</param>
    /// <param name="n">to change to 2, n = 2</param>
    public void SetEnergy(PlayerEntity player, int n)
    {
        ChangeEnergy(player, n - (int)player.myEnergy);
    }

    /// <summary>
    /// change energy value
    /// </summary>
    /// <param name="player">the player</param>
    /// <param name="n">to subtract 3 energy, n = -3</param>
    public void ChangeEnergy(PlayerEntity player, int n)
    {
        player.myEnergy = Math.Clamp(player.myEnergy + n, 0, player.maxEnergy);
        UpdateStats(player);
    }

    /// <summary>
    /// set health value
    /// </summary>
    /// <param name="player">the player</param>
    /// <param name="n">to change to 2, n = 2</param>
    public void SetHealth(PlayerEntity player, int n)
    {
        ChangeHealth(player, n - (int)player.health);
    }

    /// <summary>
    /// change health value
    /// </summary>
    /// <param name="player">the player</param>
    /// <param name="n">to subtract 3 health, n = -3</param>
    public void ChangeHealth(PlayerEntity player, int n)
    {
        player.health = Math.Clamp(player.health + n, 0, 3);

        if (player != null && n < 0)
            MoveCamera.instance.Shake();

        UpdateStats(player);
        if (player.health <= 0)
            PhaseManager.instance.GameOver($"{player.name} lost all their HP.", false);
    }

    /// <summary>
    /// set movement value
    /// </summary>
    /// <param name="player">the player</param>
    /// <param name="n">to change to 2, n = 2</param>
    public void SetMovement(PlayerEntity player, int n)
    {
        ChangeMovement(player, n - (int)player.movementLeft);
    }

    /// <summary>
    /// change movement value
    /// </summary>
    /// <param name="player">the player</param>
    /// <param name="n">to subtract 3 movement, n = -3</param>
    public void ChangeMovement(PlayerEntity player, int n)
    {
        player.movementLeft = Math.Clamp(player.movementLeft + n, 0, player.movesPerTurn);
        UpdateStats(player);

        if (player.movementLeft + n < 0)
            player.movementLeft += n;
    }

    #endregion

}
