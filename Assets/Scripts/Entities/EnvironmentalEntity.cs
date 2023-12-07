using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;

public class EnvironmentalEntity : MovingEntity
{
    [Foldout("Enviromental Entity", true)]
        [Tooltip("Store this entity's instructions")][ReadOnly] public Card card;
        [Tooltip("Store this entity's delay time")][ReadOnly] public int delay;

    public override IEnumerator EndOfTurn()
    {
        delay--;
        if (delay == 0)
        {
            Debug.Log(this.name + " went off");
            yield return ResolveList();
            Destroy(this.gameObject);
        }
    }

    IEnumerator ResolveList()
    {
        string divide = card.enviroEffect.Replace(" ", "");
        divide = divide.ToUpper().Trim();
        string[] methodsInStrings = divide.Split('/');

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
            }
        }
    }

    IEnumerator ResolveMethod(string methodName)
    {
        methodName = methodName.Replace("]", "").Trim();

        switch (methodName)
        {
            case "STUNALL":
                yield return StunAll(FindEntitiesInRange());
                break;
            default:
                Debug.LogError($"{methodName} isn't a method");
                yield return null;
                break;
        }
    }

    List<MovingEntity> FindEntitiesInRange()
    {
        List<TileData> tilesInRange = NewManager.instance.CalculateReachableGrids(this.currentTile, card.areaOfEffect, false);
        List<MovingEntity> entitiesInRange = new();

        foreach (TileData tile in tilesInRange)
        {
            try { entitiesInRange.Add(tile.myEntity.GetComponent<MovingEntity>()); }
            catch (NullReferenceException) { continue; }
        }

        entitiesInRange.RemoveAll(item => item == null); //delete all tiles that are null
        return entitiesInRange;
    }

    IEnumerator StunAll(List<MovingEntity> allEntities)
    {
        foreach (MovingEntity entity in allEntities)
        {
            if (entity == null)
                Debug.Log("null");
            else
                Debug.Log($"{entity.name}");
        }

        foreach (MovingEntity entity in allEntities)
        {
            entity.stunned += card.stunDuration;
        }
        yield return null;
    }
}   
