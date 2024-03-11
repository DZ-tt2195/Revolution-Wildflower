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
            case "SETVISION":
                yield return SetGuardVision(FindGuardsInRange());
                break;

            case "SLOWGUARDMOVEMENT":
                yield return SlowGuardMovement(FindGuardsInRange());
                break;
            case "SLOWPLAYERMOVEMENT":
                yield return SlowPlayerMovement(FindPlayersInRange());
                break;

            case "STUNPLAYERS":
                yield return StunPlayers(FindPlayersInRange());
                break;
            case "STUNGUARDS":
                yield return StunGuards(FindGuardsInRange());
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

        try
        {
            entitiesInRange.Add(this.currentTile.myEntity.GetComponent<WallEntity>());
        }
        catch
        {
            //do nothing
        }

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

        try
        {
            entitiesInRange.Add(this.currentTile.myEntity.GetComponent<GuardEntity>());
        }
        catch
        {
            //do nothing
        }


        foreach (TileData tile in tilesInRange)
        {
            try { entitiesInRange.Add(tile.myEntity.GetComponent<GuardEntity>()); }
            catch (NullReferenceException) { continue; }
        }

        entitiesInRange.RemoveAll(item => item == null);
        return entitiesInRange;
    }

    List<PlayerEntity> FindPlayersInRange()
    {
        List<TileData> tilesInRange = NewManager.instance.CalculateReachableGrids(this.currentTile, card.areaOfEffect, false);
        List<PlayerEntity> entitiesInRange = new();

        try
        {
            entitiesInRange.Add(this.currentTile.myEntity.GetComponent<PlayerEntity>());
        }
        catch
        {
            //do nothing
        }

        foreach (TileData tile in tilesInRange)
        {
            try { entitiesInRange.Add(tile.myEntity.GetComponent<PlayerEntity>()); }
            catch (NullReferenceException) { continue; }
        }

        entitiesInRange.RemoveAll(item => item == null);
        return entitiesInRange;
    }

    List<MovingEntity> FindEntitiesInRange()
    {
        List<TileData> tilesInRange = NewManager.instance.CalculateReachableGrids(this.currentTile, card.areaOfEffect, false);
        List<MovingEntity> entitiesInRange = new();

        try
        {
            entitiesInRange.Add(this.currentTile.myEntity.GetComponent<MovingEntity>());
        }
        catch
        {
            //do nothing
        }

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

    IEnumerator StunGuards(List<GuardEntity> allGuards)
    {
        foreach (GuardEntity guard in allGuards)
        {
            yield return card.StunGuard(guard);
        }
    }

    IEnumerator StunPlayers(List<PlayerEntity> allPlayers)
    {
        foreach (PlayerEntity player in allPlayers)
        {
            yield return card.StunPlayer(player);
        }
    }

    IEnumerator DamageWalls(List<WallEntity> allWalls)
    {
        foreach (WallEntity entity in allWalls)
        {
            yield return card.AttackWall(entity);
        }
        yield return null;
    }

    IEnumerator SlowPlayerMovement(List<PlayerEntity> allPlayers)
    {
        foreach (PlayerEntity player in allPlayers)
            yield return player.movementLeft-=card.changeInMP;
    }

    IEnumerator SlowGuardMovement(List<GuardEntity> allGuards)
    {
        foreach (GuardEntity guard in allGuards)
            yield return card.AffectGuardMovement(guard);
    }

    IEnumerator SetGuardVision(List<GuardEntity> allGuards)
    {
        foreach (GuardEntity guard in allGuards)
        {
            guard.DetectionRangePatrol = card.vision;
            yield return null;
        }
    }

#endregion

}