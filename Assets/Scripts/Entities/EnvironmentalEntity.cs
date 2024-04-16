using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;
using TMPro;

public class EnvironmentalEntity : MovingEntity
{
    [Foldout("Enviromental Entity", true)]
    [Tooltip("Store this entity's instructions")][ReadOnly] public Card card;
    [Tooltip("Store this entity's delay time")][ReadOnly] public int delay;
    public int delayMax;
    [SerializeField] GameObject TimeRim;
    [SerializeField] public TMP_Text ValueDisplay;
    [SerializeField] public SpriteRenderer timerRen;
    [SerializeField] float timeTick = 0.2f;
    public Material radial;

    #region Entity Stuff

    public override IEnumerator EndOfTurn()
    {
        if (delay > 0)
        {
            yield return ResolveList("CONTINUOUS");
        }
        float oldValue = delay;
        delay--;
        ValueDisplay.text = (delay).ToString();
        float elapsedTime = 0f;
        while (elapsedTime < timeTick)
        {
            elapsedTime += Time.deltaTime;
            float lerpValue = Mathf.Lerp(oldValue, delay, elapsedTime / timeTick);
            MaterialPropertyBlock matBlock = new();
            Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAA" + lerpValue / delayMax);
            matBlock.SetFloat("_Fill", lerpValue / delayMax);
            timerRen.SetPropertyBlock(matBlock);
            yield return null;
        }



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
        string divide = card.data.enviroaction.Replace(" ", "");
        divide = divide.ToUpper().Trim();
        string[] methodsInStrings = divide.Split('/');

        if (condition == methodsInStrings[0])
        {
            foreach (string nextMethod in methodsInStrings)
            {
                LevelGenerator.instance.DisableAllTiles();
                LevelGenerator.instance.DisableAllCards();

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
        List<GuardEntity> guardsInRange = FindGuardsInRange();
        List<PlayerEntity> playersInRange = FindPlayersInRange();
        List<WallEntity> wallsInRange = FindWallsInRange();

        yield return card.CalculateDistraction(this.currentTile);
        switch (methodName)
        {

            case "SETVISION":
                foreach (GuardEntity guard in guardsInRange)
                    guard.DetectionRangePatrol = card.data.vision;
                break;

            case "SLOWGUARDMOVEMENT":
                foreach (GuardEntity guard in guardsInRange)
                    guard.movementLeft -= card.data.chMP;
                break;
            case "SLOWPLAYERMOVEMENT":
                foreach (PlayerEntity player in playersInRange)
                    player.movementLeft -= card.data.chMP;
                break;

            case "STUNPLAYERS":
                foreach (PlayerEntity player in playersInRange)
                    yield return card.StunPlayer(player);
                break;
            case "STUNGUARDS":
                foreach (GuardEntity guard in guardsInRange)
                    yield return card.StunGuard(guard);
                break;

            case "DAMAGEWALLS":
                foreach (WallEntity wall in wallsInRange)
                    yield return card.AttackWall(wall);
                break;

            default:
                Debug.LogError($"{methodName} isn't a method");
                yield return null;
                break;

        }
    }

    List<WallEntity> FindWallsInRange()
    {
        List<TileData> tilesInRange = Pathfinder.instance.CalculateReachableGrids(this.currentTile, card.data.aoe, false);
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
        List<TileData> tilesInRange = Pathfinder.instance.CalculateReachableGrids(this.currentTile, card.data.aoe, false);
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
        List<TileData> tilesInRange = Pathfinder.instance.CalculateReachableGrids(this.currentTile, card.data.aoe, false);
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

#endregion

}