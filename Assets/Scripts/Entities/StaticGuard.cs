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
        print("current tile" + currentTile.gridPosition);
        print("Target " + PatrolPoints[0]);
        if (currentTile.gridPosition == PatrolPoints[0])
        {
            print("Break patrol on target");
            StopAllCoroutines();
            yield break;
        }
        Pathfinder.instance.CalculatePathfinding(currentTile, LevelGenerator.instance.listOfTiles[PatrolPoints[0].x, PatrolPoints[0].y], movementLeft, true, true);
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

    public override IEnumerator newAction()
    {
        alertStatus = Alert.Patrol;
        if (DistractionPoints.Count > 0)
        {
            alertStatus = Alert.Persue;
            LevelGenerator.instance.FindTile(DistractionPoints[^1]).currentGuardTarget = true;
        }
        foreach (GuardEntity guard in LevelGenerator.instance.listOfGuards)
            guard.CheckForPlayer();

        print("New Action" + alertStatus);

        if (alertStatus == Alert.Attack)
        {
            if (movementLeft > 0 || attacksLeft > 0)
            {
                yield return Attack(CurrentTarget);
            }
        }
        else if (alertStatus == Alert.Persue)
        {
            if (movementLeft > 0)
            {
                print("New Action starting persue");
                yield return persue();
            }
        }
        else if (alertStatus == Alert.Patrol)
        {
            if (movementLeft > 0)
            {
                yield return Patrol();
            }
        }
    }
}
