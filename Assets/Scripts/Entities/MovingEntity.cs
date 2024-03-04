using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using TMPro;

public class MovingEntity : Entity
{
    [Foldout("Moving Entity", true)]
        [Tooltip("Remaining moves")] [ReadOnly] public int movementLeft;
        [Tooltip("How many tiles this moves per turn")] public int movesPerTurn = 4;
        [Tooltip("Turns which this does nothing")][ReadOnly] public int stunned = 0;
        [Tooltip("stunned indicator")][SerializeField] GameObject stunObject;
        [Tooltip("stunned number")][SerializeField] TMP_Text stunText;

    private void Start()
    {
        stunObject.SetActive(false);
    }

    public virtual IEnumerator EndOfTurn()
    {
        yield return null;
    }

    public void stunChange(int changeSum)
    {
        stunned += changeSum;
        if (stunned > 0)
        {
            stunObject.SetActive(true);
            stunText.text = stunned.ToString();

        }
    }
}
