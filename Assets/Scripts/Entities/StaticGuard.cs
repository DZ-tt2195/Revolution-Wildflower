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

        //return base.Patrol();
    }
}
