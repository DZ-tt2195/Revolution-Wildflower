using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class StaticGuard : GuardEntity
{
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
}
