using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallEntity : Entity
{
    [Foldout("Wall Entity",true)]
        [Tooltip("Health a wall has")] [ReadOnly] public int health;
        [Tooltip("Wall facing left")] [SerializeField] Sprite leftWall;
        [Tooltip("Wall facing right")][SerializeField] Sprite rightWall;

    public override string HoverBoxText()
    {
        return "Current Health: " + health;
    }

    public void WallDirection(string data)
    {
        switch (data)
        {
            case "l":
                direction = new Vector2Int(-1, 0);
                spriteRenderer.sprite = leftWall;
                break;
            case "r":
                direction = new Vector2Int(1, 0);
                spriteRenderer.sprite = rightWall;
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
