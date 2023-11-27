using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitEntity : ObjectiveEntity
{
    public override bool CanInteract()
    {
        return NewManager.instance.listOfObjectives.Count == 1;
    }

    public override string HoverBoxText()
    {
        return "Exit here when you've completed all other objectives";
    }

    public override IEnumerator ObjectiveComplete()
    {
        NewManager.instance.GameOver("You won!");
        yield return base.ObjectiveComplete();
    }
}
