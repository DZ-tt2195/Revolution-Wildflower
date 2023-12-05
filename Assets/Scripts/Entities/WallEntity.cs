using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallEntity : Entity
{
    [Foldout("Wall Entity", true)]
    [Tooltip("Health a wall has")] [ReadOnly] public int health;


    public override string HoverBoxText()
    {
        return "Current Health: " + health;
    }

    public void WallDirection(string data)
    {
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

    public void AffectWall(int effect)
    {
        health += effect;
        if (health <= 0)
        {
            NewManager.instance.listOfWalls.Remove(this);
            Destroy(this.gameObject);
        }
    }
}
