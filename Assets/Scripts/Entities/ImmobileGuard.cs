using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmobileGuard : GuardEntity
{
    private void Awake()
    {
        maxMovement = 0;
        movementLeft = 0;
        AttackLine = GetComponent<LineRenderer>();
        DetectionRangeMax = DetectionRangePatrol;
        alertStatus = Alert.Patrol;
        PatrolTarget = 0;
    }

    public override IEnumerator EndOfTurn()
    {
        return base.EndOfTurn();
    }
}
