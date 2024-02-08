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

#region Entity Stuff

    public override IEnumerator EndOfTurn()
    {
        if (delay > 0)
        {
            yield return ResolveList("CONTINUOUS");
        }
        delay--;
        if (delay == 0)
        {
            yield return ResolveList("END");
            Destroy(this.gameObject);
        }
    }

#endregion

#region Activation 

    IEnumerator ResolveList(string condition)
    {
        string divide = card.enviroEffect.Replace(" ", "");
        divide = divide.ToUpper().Trim();
        string[] methodsInStrings = divide.Split('/');

        if (condition == methodsInStrings[0])
        {
            foreach (string nextMethod in methodsInStrings)
            {
                NewManager.instance.DisableAllTiles();
                NewManager.instance.DisableAllCards();

                if (nextMethod == "" || nextMethod == "NONE" || nextMethod == condition)
                {
                    continue;
                }
                else
                {
                    yield return ResolveMethod(nextMethod);
                }
            }
        }
    }

    IEnumerator ResolveMethod(string methodName)
    {
        methodName = methodName.Replace("]", "").Trim();

        switch (methodName)
        {
            case "GUARDMOVEMENT":
                yield return GuardMovement(FindGuardsInRange());
                break;
            case "STUNALL":
                yield return StunAll(FindEntitiesInRange());
                break;
            case "DAMAGEWALLS":
                yield return DamageWalls(FindWallsInRange());
                break;
            default:
                Debug.LogError($"{methodName} isn't a method");
                yield return null;
                break;
        }
    }

    List<WallEntity> FindWallsInRange()
    {
        List<TileData> tilesInRange = NewManager.instance.CalculateReachableGrids(this.currentTile, card.areaOfEffect, false);
        List<WallEntity> entitiesInRange = new();

        foreach (TileData tile in tilesInRange)
        {
            try { entitiesInRange.Add(tile.myEntity.GetComponent<WallEntity>()); }
            catch (NullReferenceException) { continue; }
        }

        entitiesInRange.RemoveAll(item => item == null); //delete all tiles that are null
        return entitiesInRange;
    }

    List<GuardEntity> FindGuardsInRange()
    {
        List<TileData> tilesInRange = NewManager.instance.CalculateReachableGrids(this.currentTile, card.areaOfEffect, false);
        List<GuardEntity> entitiesInRange = new();

        foreach (TileData tile in tilesInRange)
        {
            try { entitiesInRange.Add(tile.myEntity.GetComponent<GuardEntity>()); }
            catch (NullReferenceException) { continue; }
        }

        entitiesInRange.RemoveAll(item => item == null);
        return entitiesInRange;
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

        entitiesInRange.RemoveAll(item => item == null);
        return entitiesInRange;
    }

#endregion

#region Effects 

    IEnumerator StunAll(List<MovingEntity> allEntities)
    {
        foreach (MovingEntity entity in allEntities)
        {
            if (entity.CompareTag("Player"))
                yield return card.StunPlayer(entity.GetComponent<PlayerEntity>());
            if (entity.CompareTag("Guard"))
                yield return card.StunGuard(entity.GetComponent<GuardEntity>());
        }
        yield return null;
    }

    IEnumerator DamageWalls(List<WallEntity> allWalls)
    {
        foreach (WallEntity entity in allWalls)
        {
            yield return card.AttackWall(entity);
        }
        yield return null;
    }

    IEnumerator GuardMovement(List<GuardEntity> allGuards)
    {
        foreach (GuardEntity guard in allGuards)
            yield return card.AffectGuardMovement(guard);
        yield return null;
    }

#endregion

}