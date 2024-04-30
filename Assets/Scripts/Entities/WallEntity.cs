using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AK.Wwise;

public class WallEntity : Entity
{
    [Foldout("Wall Entity", true)]
    [Tooltip("Health a wall has")] [ReadOnly] public int health;
    [Tooltip("semi damaged wall sprite")] [SerializeField] Sprite damagedSprite;
    [Tooltip("heavily damaged wall sprite")][SerializeField] Sprite heavilyDamagedSprite;
    [SerializeField] List<GameObject> wallList = new();
    public static int wallsCurrentlyBreaking = 0;
    [SerializeField] AK.Wwise.Event breakSound;
    int maxHealth;

    public override string HoverBoxText()
    {
        return $"Current Health: {health}";
    }

    public void WallSetup(int initialHealth, string data)
    {
        maxHealth = initialHealth;
        health = initialHealth;

        switch (data)
        {
            case "l": //wall faces left
                direction = new Vector2Int(-1, 0);
                transform.Rotate(0f, 0f, 0f);
                break;
            case "r": //wall faces right
                direction = new Vector2Int(1, 0);
                transform.Rotate(0f, 90f, 0f);
                break;
            case "n": //wall faces north
                transform.Rotate(0f, 180f, 0f);
                break;
            case "s": //wall faces south
                transform.Rotate(0f, 270f, 0f);
                break;
        }
    }

    void CallBackHitFunction(object in_cookie, AkCallbackType callType, object in_info)
    {
        if (callType == AkCallbackType.AK_EndOfEvent)
        {
            wallsCurrentlyBreaking--;
            Destroy(this.gameObject);
        }
    }

    public void AffectWall(int effect)
    {
        health += effect;
        MoveCamera.instance.Shake();
        if (health <= 0)
        {
            LevelGenerator.instance.listOfWalls.Remove(this);
            currentTile.myEntity = null;
            if (wallsCurrentlyBreaking == 0)
            {
                foreach (GameObject wall in wallList)
                {
                    wall.SetActive(false);
                }
                wallsCurrentlyBreaking++;
                breakSound.Post(gameObject, (uint)AkCallbackType.AK_EndOfEvent, CallBackHitFunction);

            }
            else
            {
                Destroy(this.gameObject);
            }
        }
        else if (health <= maxHealth*(1/3))
        {
            spriteRenderer.sprite = heavilyDamagedSprite;
        }
        else if (health <= maxHealth*(2/3))
        {
            MoveCamera.instance.Shake();
            spriteRenderer.sprite = damagedSprite;
        }
    }
}
