using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class StaticGuard : GuardEntity
{
    public Vector2Int StartDirection;
    private void Awake()
    {
        AttackLine = GetComponent<LineRenderer>();
        DetectionRangeMax = DetectionRangePatrol;
        alertStatus = Alert.Patrol;
        PatrolTarget = 0;
    }

    public override IEnumerator Patrol()
    {
        if (currentTile == LevelGenerator.instance.listOfTiles[PatrolPoints[0].x, PatrolPoints[0].y])
        {
            yield break;
        }
        Pathfinder.instance.CalculatePathfinding(currentTile, LevelGenerator.instance.listOfTiles[PatrolPoints[PatrolTarget].x, PatrolPoints[PatrolTarget].y], movementLeft, true, true);
        TileData nextTile = Pathfinder.instance.CurrentAvailableMoveTarget;  //moves towards the next patrol point
        Vector2Int nextDirection = nextTile.gridPosition - currentTile.gridPosition;

        if (nextDirection != direction)
        {
            direction = nextDirection;
            CalculateTiles();
        }
        else
        {
            //print("moving too " + nextTile.gridPosition);
            if (nextTile.myEntity == null)
            {
                StartCoroutine(MoveTile(nextTile));
                //footsteps.Post(gameObject);
            }
            movementLeft--;
        }
        yield return new WaitForSeconds(movePauseTime);
        print("Checking New Action");
        yield return newAction();
    }

    public override IEnumerator persue()
    {
        print(DistractionPoints.Count);
        print(currentTile.gridPosition + "checking distraction");
        if (DistractionPoints.Count == 0)
        {
            print("False Distraction");
            yield return (newAction());
            yield break;
        }
        if (currentTile.gridPosition == DistractionPoints[^1])
        {
            print("on distraction point");
            LevelGenerator.instance.FindTile(DistractionPoints[^1]).currentGuardTarget = false;
            DistractionPoints.RemoveAt(DistractionPoints.Count - 1);
            if (DistractionPoints.Count == 0)
            {
                print("no more distractions");
                alertStatus = Alert.Patrol;
                CheckForPlayer();
                if (alertStatus == Alert.Attack)
                {
                    if (attacksLeft > 0 || movementLeft > 0)
                    {
                        yield return Attack(CurrentTarget);
                    }
                    else
                        yield break;
                }
                else if (alertStatus == Alert.Patrol)
                {
                    if (movementLeft > 0)
                    {
                        yield return Patrol();
                    }
                    else
                        yield break;
                }
            }
            else if (currentTile.gridPosition == DistractionPoints[DistractionPoints.Count - 1])
            {
                print("restarting persuit");
                yield return persue();
            }
        }
        if (movementLeft > 0)
        {
            //print(movementLeft);
            TileData nextTile;
            Pathfinder.instance.CalculatePathfinding(currentTile, LevelGenerator.instance.FindTile(DistractionPoints[^1]), movementLeft, true, true);
            nextTile = Pathfinder.instance.CurrentAvailableMoveTarget;  //moves towards the next patrol point
            Vector2Int nextDirection = nextTile.gridPosition - currentTile.gridPosition;

            if (nextDirection != direction)
            {
                direction = nextDirection;
                foreach (GuardEntity guard in LevelGenerator.instance.listOfGuards)
                {
                    guard.CalculateTiles();
                }
            }
            else
            {
                //print("moving too " + nextTile.gridPosition);
                if (nextTile.myEntity == null)
                {
                    StartCoroutine(MoveTile(nextTile)); //footsteps.Post(gameObject);
                }
                movementLeft--;
            }

            yield return new WaitForSeconds(movePauseTime);
            yield return newAction();
        }
    }
}
