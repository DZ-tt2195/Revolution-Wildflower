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

    public override IEnumerator ObjectiveComplete(PlayerEntity player)
    {
        foreach (GuardEntity guard in NewManager.instance.listOfGuards)
        {
            if (guard.CurrentTarget == player)
                guard.resetAlert();
        }

        NewManager.instance.listOfPlayers.Remove(player);
        NewManager.instance.lastSelectedPlayer = null;
        Destroy(player.myBar.gameObject);
        Destroy(player.gameObject);

        if (NewManager.instance.listOfPlayers.Count == 0)
        {
            NewManager.instance.GameOver("You won!", true);
            yield return base.ObjectiveComplete(player);
        }
        else
        {
            NewManager.instance.BackToStart(false);
        }
    }
}
