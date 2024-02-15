using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class ObjectiveEntity : Entity
{
    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;
    [ReadOnly] public string objective;

    public virtual bool CanInteract()
    {
        return true;
    }

    public virtual IEnumerator ObjectiveComplete(PlayerEntity player)
    {
        NewManager.instance.listOfObjectives.Remove(this);
        NewManager.instance.objectiveButton.gameObject.SetActive(false);

        currentTile.myEntity = null;
        player.adjacentObjective = null;

        if (!DialogueManager.GetInstance().dialogueIsPlaying)
        {
            DialogueManager.GetInstance().StartStory(inkJSON);
            DialogueManager.dialogueVariables.globalVariablesStory.variablesState["current_player"] = player.name;
            DialogueManager.dialogueVariables.globalVariablesStory.variablesState["current_objective"] = objective;
            DialogueManager.GetInstance().EnterDialogueMode();

        }

        Destroy(this.gameObject);
        yield return null;
    }
}

