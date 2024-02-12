using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using TMPro;

public class ObjectiveEntity : Entity
{
    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;
    [ReadOnly] public string objective;
    [ReadOnly] public string instructionsWhenCompleted;

    public virtual bool CanInteract()
    {
        return true;
    }

    public virtual IEnumerator ObjectiveComplete(PlayerEntity player)
    {
        NewManager.instance.listOfObjectives.Remove(this);
        NewManager.instance.objectiveButton.gameObject.SetActive(false);

        if (!DialogueManager.GetInstance().dialogueIsPlaying)
        {
            DialogueManager.GetInstance().StartStory(inkJSON);
            DialogueManager.dialogueVariables.globalVariablesStory.variablesState["current_player"] = player.name;
            DialogueManager.dialogueVariables.globalVariablesStory.variablesState["current_objective"] = objective;
            DialogueManager.GetInstance().EnterDialogueMode();
        }

        string[] pointList = instructionsWhenCompleted.Split('|');
        foreach (string nextInstruction in pointList)
        {
            switch (nextInstruction)
            {
                case "ALLDRAW":
                    foreach (PlayerEntity nextPlayer in NewManager.instance.listOfPlayers)
                        nextPlayer.PlusCards(2);
                    break;
            }
        }

        NewManager.instance.listOfObjectives.Remove(this);
        NewManager.instance.objectiveButton.gameObject.SetActive(false);
        currentTile.myEntity = null;
        player.adjacentObjective = null;

        yield return null;
        Destroy(this.gameObject);
    }
}