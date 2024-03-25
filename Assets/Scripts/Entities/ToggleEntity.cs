using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class ToggleEntity : ObjectiveEntity
{
    [Foldout("Toggle Entity", true)]
        [ReadOnly] public bool toggledOn;
        [ReadOnly] public string interactCondition;
        [ReadOnly] public string interactInstructions;
        [ReadOnly] public List<Vector2Int> targetPoints = new List<Vector2Int>();

    public override bool CanInteract()
    {
        switch (interactCondition)
        {
            case "NOENTITIES":
                if (!toggledOn)
                {
                    foreach (Vector2Int vector in targetPoints)
                    {
                        TileData getTile = LevelGenerator.instance.FindTile(vector);
                        if (getTile.myEntity != null)
                        {
                            Debug.Log($"{getTile} has entity");
                            return false;
                        }
                    }
                }
                return true;

            default:
                return true;
        }
    }

    public override IEnumerator ObjectiveComplete(PlayerEntity player)
    {
        yield return null;
        toggledOn = !toggledOn;
        spriteRenderer.color = (toggledOn) ? Color.blue : Color.red;
        switch (interactInstructions)
        {
            case "TOGGLEWALLS":
                foreach (Vector2Int vector in targetPoints)
                {
                    TileData getTile = LevelGenerator.instance.FindTile(vector);
                    if (toggledOn)
                    {
                        WallEntity wall = getTile.transform.GetComponentInChildren<WallEntity>();
                        StartCoroutine(wall.MoveTile(getTile));
                    }
                    else
                    {
                        WallEntity wall = getTile.myEntity.GetComponent<WallEntity>();
                        getTile.myEntity = null;
                        wall.transform.localPosition = new Vector3(0, -1000, 0);
                    }
                }
                break;

            default:
                Debug.Log("failed to read");
                break;
        }
    }
}
