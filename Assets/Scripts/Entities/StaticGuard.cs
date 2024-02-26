using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class StaticGuard : GuardEntity
{

    public override IEnumerator Patrol()
    {
        if (currentTile == NewManager.instance.listOfTiles[PatrolPoints[0].x, PatrolPoints[0].y])
        {
            yield break;
        }
        NewManager.instance.CalculatePathfinding(currentTile, NewManager.instance.listOfTiles[PatrolPoints[PatrolTarget].x, PatrolPoints[PatrolTarget].y], movementLeft, true, true);
        TileData nextTile = NewManager.instance.CurrentAvailableMoveTarget;  //moves towards the next patrol point
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
                MoveTile(nextTile);//move to the tile
                //footsteps.Post(gameObject);
            }
            movementLeft--;
        }
        //return base.Patrol();
    }
}
