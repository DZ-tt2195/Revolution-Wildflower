using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class MovingEntity : Entity
{
    [Foldout("Moving Entity", true)]
        [Tooltip("Remaining moves")] [ReadOnly]public int movementLeft;
        [Tooltip("How many tiles this moves per turn")][ReadOnly] public int movesPerTurn = 3;

public virtual IEnumerator EndOfTurn()
    {
        yield return null;
    }
}
